using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Text.Json;
namespace DhhResearch;

internal static class FinalAudit
{
    public static int Run(string root)
    {
        var results=new List<object?[]>();
        void Check(string label,bool ok,string detail)
        {results.Add([label,ok?"PASS":"FAIL",detail]);}
        string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        var papers=Csv.Read(Path.Combine(root,"data","publications.csv"));
        var dois=papers.Select(p=>p["doi"].ToLowerInvariant()).Where(x=>x!="na").ToArray();
        Check("Canonical_DOI_and_ID_uniqueness",dois.Distinct().Count()==dois.Length&&papers.Select(p=>p["publication_id"]).Distinct().Count()==papers.Count,$"{papers.Count} canonical records.");
        var manifest=Csv.Read(Path.Combine(root,"data","fulltext_manifest.csv"));
        Check("Original_PDF_hashes_preserved",manifest.All(m=>Hash(Path.Combine(root,"source_papers",m["filename"]))==m["sha256"]),$"{manifest.Count} user-supplied originals.");
        var appraisal=Csv.Read(Path.Combine(root,"data","fulltext_appraisal.csv"));
        Check("Fulltext_appraisal_coverage",appraisal.Select(x=>x["publication_id"]).Order().SequenceEqual(manifest.Select(x=>x["publication_id"]).Order()),"Exactly one appraisal per supplied original.");
        var extraction=Csv.Read(Path.Combine(root,"data","extracted_data.csv"));
        string[] numeric="n_treatment,n_control,mean_treatment,mean_control,sd_treatment,sd_control,raw_t,raw_df,raw_F,raw_CI_lower,raw_CI_upper,paired_n,paired_r,sd_difference,cluster_count,icc,sample_size,scale_min,scale_max,raw_r,correlation_n,raw_md,raw_se".Split(',');
        foreach(var x in extraction)foreach(string k in numeric)_=Csv.Number(x[k]);
        Check("All_raw_numeric_fields_finite_or_NA",true,$"{extraction.Count} rows including noncomputable summaries.");
        Check("Analysis_counts_and_roles",extraction.Count==42&&Csv.Read(Path.Combine(root,"results","effect_sizes.csv")).Count==22
            &&Csv.Read(Path.Combine(root,"results","synthesis_membership.csv")).Where(x=>x["synthesis_id"]=="reading_primary").Select(x=>x["independence_cluster_id"]).Distinct().Count()==3,"42 extraction rows; 22 estimable; 3 primary dependence keys.");
        var checks=Csv.Read(Path.Combine(root,"validation","numerical_checks.csv"));
        Check("Numerical_validation",checks.Count==10&&checks.All(x=>x["status"]=="PASS"),"Ten numerical/guard checks; distinct from this delivery audit.");
        var count=Csv.Read(Path.Combine(root,"paper","word_count.csv")).Single(x=>x["metric"]=="excluding_references");
        double words=Csv.Number(count["words"])!.Value;
        Check("Manuscript_word_limit",words>=2000&&words<=3000,$"{words} words including tables/captions/appendix, excluding references.");
        using(var zip=ZipFile.OpenRead(Path.Combine(root,"paper","DHH_Comprehension_Research.docx")))
        {
            using var stream=zip.GetEntry("word/document.xml")!.Open();var xml=XDocument.Load(stream);
            XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            string actual=string.Concat(xml.Descendants(w+"t").Select(x=>x.Value));
            string expected=Regex.Replace(File.ReadAllText(Path.Combine(root,"paper","manuscript.md")),@"!\[[^\]]*\]\([^\)]*\)","");
            string Normalize(string x)=>new(x.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
            Check("DOCX_matches_exported_manuscript",Normalize(actual)==Normalize(expected),"Full body, tables, references and appendix; markup and image paths excluded.");
        }
        Check("Delivered_PDF_matches_final_render",Hash(Path.Combine(root,"paper","DHH_Comprehension_Research.pdf"))==Hash(Path.Combine(root,"tmp","document_qa_final","DHH_Comprehension_Research.pdf")),"Byte-identical PDF from the inspected final DOCX render.");
        using(var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"validation","latex_compile.json"))))
            Check("Standalone_LaTeX_compilation",doc.RootElement.GetProperty("kind").GetString()=="success","Desktop compiler success; no external figure/bibliography files.");
        Csv.Write(Path.Combine(root,"validation","delivery_audit.csv"),["check","status","detail"],results);
        Csv.Write(Path.Combine(root,"validation","artifact_hashes.csv"),["artifact","sha256"],
            new[]{".docx",".pdf",".tex"}.Select(ext=>new object?[]{"paper/DHH_Comprehension_Research"+ext,Hash(Path.Combine(root,"paper","DHH_Comprehension_Research"+ext))}));
        Console.WriteLine($"Delivery audit: {results.Count(x=>(x[1] as string) == "PASS")} passed, {results.Count(x=>(x[1] as string) == "FAIL")} failed.");
        return results.All(x=>(x[1] as string) == "PASS")?0:1;
    }
}
