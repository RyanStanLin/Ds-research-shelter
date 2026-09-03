using System.Text;
using System.Text.RegularExpressions;
namespace DhhResearch;

internal static class PaperExport
{
    public static int Run(string root)
    {
        var p=Csv.Read(Path.Combine(root,"data","publications.csv"));
        var originals=Csv.Read(Path.Combine(root,"data","fulltext_manifest.csv")).Select(x=>x["publication_id"]).ToHashSet();
        string Authors(string text)
        {
            var a=text.Split(';',StringSplitOptions.TrimEntries);return a.Length<2?a[0]:string.Join(", ",a[..^1])+", & "+a[^1];
        }
        string Short(Dictionary<string,string> x)
        {
            var a=x["authors"].Split(';',StringSplitOptions.TrimEntries).Select(s=>s.Split(',')[0]).ToArray();
            return a.Length==1?a[0]:a.Length==2?a[0]+" and "+a[1]:a[0]+" et al.";
        }
        var refs=new StringBuilder("## References\n\n");
        foreach(var x in p.OrderBy(x=>x["authors"],StringComparer.Ordinal))
        {
            string vi=x["volume_issue"]=="NA"?"":", "+Regex.Replace(x["volume_issue"],@"^(\d+)(.*)$","*$1*$2");
            string pages=x["pages"] is "NA" or "7-page_report"?"":", "+x["pages"];
            refs.AppendLine($"{Authors(x["authors"])} ({x["year"]}). {x["title"]}. *{x["journal"]}*{vi}{pages}. {(x["doi"]=="NA"?x["source_url"]:"https://doi.org/"+x["doi"])}\n");
        }
        refs.AppendLine("Deeks, J. J., Higgins, J. P. T., Altman, D. G., McKenzie, J. E., & Veroniki, A. A. (Eds.). (2024). Chapter 10: Analysing data and undertaking meta-analyses. In *Cochrane Handbook for Systematic Reviews of Interventions*. https://training.cochrane.org/handbook/current/chapter-10\n");
        refs.AppendLine("Page, M. J., McKenzie, J. E., Bossuyt, P. M., Boutron, I., Hoffmann, T. C., Mulrow, C. D., Shamseer, L., Tetzlaff, J. M., Akl, E. A., Brennan, S. E., Chou, R., Glanville, J., Grimshaw, J. M., Hróbjartsson, A., Lalu, M. M., Li, T., Loder, E. W., Mayo-Wilson, E., McDonald, S., ... Moher, D. (2021). The PRISMA 2020 statement: An updated guideline for reporting systematic reviews. *BMJ, 372*, n71. https://doi.org/10.1136/bmj.n71\n");
        refs.AppendLine("Röver, C., Knapp, G., & Friede, T. (2015). Hartung–Knapp–Sidik–Jonkman approach and its modification for random-effects meta-analysis with few studies. *BMC Medical Research Methodology, 15*, 99. https://doi.org/10.1186/s12874-015-0091-1\n");
        refs.AppendLine("Viechtbauer, W. (2010). Conducting meta-analyses in R with the metafor package. *Journal of Statistical Software, 36*(3), 1–48. https://doi.org/10.18637/jss.v036.i03\n");
        var entries=refs.ToString().Split("\n\n",StringSplitOptions.RemoveEmptyEntries).Where(x=>!x.StartsWith("##")).OrderBy(x=>x,StringComparer.InvariantCulture).ToArray();
        refs.Clear().Append("## References\n\n").Append(string.Join("\n\n",entries)).Append("\n");
        File.WriteAllText(Path.Combine(root,"paper","references.md"),refs.ToString());
        var appendix=new StringBuilder("## Appendix A Metadata only records\n\nThese records are cited to make the evidence directory auditable. Their originals were not supplied and no numerical result was extracted. Labels describe indexed evidence only.\n\n| Citation and ID | Reason for no extraction |\n|---|---|\n");
        foreach(var x in p.Where(x=>!originals.Contains(x["publication_id"])))
        {
            string reason=x["screening_status"] switch{
                "fulltext_unavailable"=>"Original unavailable",
                "qualitative_context"=>"Qualitative context",
                "review_context"=>"Review, no independent sample",
                "candidate_near_scope"=>x["outcome_preliminary"].Contains("vocabulary")||x["outcome_preliminary"].Contains("words")?"Word-level outcome":"Indirect outcome or population",
                _=>"Population or design outside scope"};
            appendix.AppendLine($"| {Short(x)} ({x["year"]}), {x["publication_id"]} | {reason} |");
        }
        File.WriteAllText(Path.Combine(root,"paper","appendix.md"),appendix.ToString());
        string body=File.ReadAllText(Path.Combine(root,"paper","body.md"));
        var counts=Csv.Read(Path.Combine(root,"results","candidate_factor_counts.csv"));
        var t1=new StringBuilder("Table 1. Factor coverage and quantitative feasibility\n\n| Factor | Originals | Estimable study units | Decision |\n|---|---:|---:|---|\n");
        foreach(var x in counts)t1.AppendLine($"| {x["factor_name"]} | {x["supplied_publications"]} | {x["estimable_study_units"]} | {(x["factor_id"]=="reading_proficiency"?"Exploratory synthesis":x["selection_status"].StartsWith("selected")?"Separate contrasts":"Mapped")} |");
        t1.AppendLine("\nNote. Estimable units can include indirect contextual outcomes. Reading pooling uses only three of the five units. Caption speed is tagged separately from teacher speech speed.\n");
        string t2="Table 2. Primary reading association data\n\n| Cohort | n | Reading measure | Content outcome | r |\n|---|---:|---|---|---:|\n| Stinson et al. (2009), school | 48* | CTBS or Mini-Battery | Lecture multiple choice | .51 |\n| Stinson et al. (2009), college | 48* | Four-test z-reading composite | Lecture multiple choice | .63 |\n| Tyler et al. (2009), school | 20 | Woodcock reading cluster | Documentary questions, 120 wpm | .75 |\n\nNote. *Complete cohort n; separate pairwise correlation n not printed. College composite includes SAT verbal.\n";
        var effects=Csv.Read(Path.Combine(root,"results","effect_sizes.csv")).ToDictionary(x=>x["effect_id"]);
        var t3=new StringBuilder("Table 3. Separate presentation contrasts\n\n| Comparison | Metric | Estimate | 95% CI |\n|---|---|---:|---|\n");
        string F(string value)=>double.Parse(value,System.Globalization.CultureInfo.InvariantCulture).ToString("0.000",System.Globalization.CultureInfo.InvariantCulture);
        foreach(var (id,label) in new (string,string)[]{("E01","Captions plus signing vs signing"),("E18","Edited vs near-verbatim captions"),("E26","SimCom vs speech, quiet"),("E28","SimCom vs speech, noise"),("E29","Quiet vs noisy speech"),("E17","90/120 vs 180 wpm captions")})
        {var x=effects[id];t3.AppendLine($"| {label} | {(x["effect_family"]=="MD"?"Percentage points":x["effect_family"]=="Hedges_g"?"Hedges' g":x["effect_family"])} | {F(x["display_estimate"])} | [{F(x["ci_lower"])}, {F(x["ci_upper"])}] |");}
        t3.AppendLine("\nNote. Hedges_g and g_z use different SD denominators. Rows are not pooled; the two noise contrasts share participants and a passage confound.\n");
        body=body.Replace("{{TABLE1}}",t1.ToString()).Replace("{{TABLE2}}",t2).Replace("{{TABLE3}}",t3.ToString());
        string full=body+"\n\n"+refs+"\n\n"+appendix;
        File.WriteAllText(Path.Combine(root,"paper","manuscript.md"),full);

        string countText=body+"\n"+appendix;
        countText=Regex.Replace(countText,@"!\[[^\]]*\]\([^\)]*\)","");
        countText=Regex.Replace(countText,@"\[([^\]]*)\]\([^\)]*\)","$1");
        int words=Regex.Matches(countText,@"\b[\p{L}\p{N}]+(?:['’–-][\p{L}\p{N}]+)*\b").Count;
        int prose=Regex.Matches(Regex.Replace(body,@"(?m)^\|.*$",""),@"\b[\p{L}\p{N}]+(?:['’–-][\p{L}\p{N}]+)*\b").Count;
        Csv.Write(Path.Combine(root,"paper","word_count.csv"),["metric","words","definition"],
            new object?[][]{["excluding_references",words,"Includes title, tables, figure captions and Appendix A; removes image paths and Markdown syntax"],["body_prose_and_headings",prose,"Before references; removes table rows only"]});
        if(words<2000||words>3000)throw new InvalidDataException($"Manuscript word count {words} outside 2000–3000.");
        Console.WriteLine($"Manuscript: {words} words excluding references; {p.Count} evidence records cited.");return 0;
    }
}
