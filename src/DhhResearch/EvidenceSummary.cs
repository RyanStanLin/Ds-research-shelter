namespace DhhResearch;

internal static class EvidenceSummary
{
    public static int Run(string root)
    {
        var p=Csv.Read(Path.Combine(root,"data","publications.csv"));
        var s=Csv.Read(Path.Combine(root,"data","study_registry.csv"));
        var f=Csv.Read(Path.Combine(root,"data","candidate_factors.csv"));
        var d=Csv.Read(Path.Combine(root,"data","extracted_data.csv"));
        var e=Csv.Read(Path.Combine(root,"results","effect_sizes.csv"));
        var m=Csv.Read(Path.Combine(root,"data","fulltext_manifest.csv")).Select(x=>x["publication_id"]).ToHashSet();
        var rows=new List<object?[]>();var selected=new List<object?[]>();
        string Decision(string id)=>id switch
        {
            "reading_proficiency"=>"selected_exploratory_meta",
            "captioning"=>"selected_standalone_contrasts",
            "sign_language_support"=>"selected_standalone_contrasts",
            _=>"mapped_not_core"
        };
        foreach(var factor in f)
        {
            string id=factor["factor_id"];var pp=p.Where(x=>x["factor_tags"].Split(';').Contains(id)).ToArray();
            var st=s.Where(x=>pp.Any(y=>y["publication_id"]==x["publication_id"])).ToArray();
            var ee=e.Where(x=>x["factor_id"]==id).ToArray();
            int primary=ee.Where(x=>x["pool_role"]=="primary").Select(x=>x["study_id"]).Distinct().Count();
            var row=new object?[]{id,factor["factor_name"],pp.Length,pp.Count(x=>m.Contains(x["publication_id"])),st.Length,
                st.Sum(x=>Csv.Number(x["n_dhh_unique"])??0),ee.Length,ee.Select(x=>x["study_id"]).Distinct().Count(),primary,Decision(id)};
            rows.Add(row);
            if(Decision(id).StartsWith("selected"))
                selected.Add([id,factor["factor_name"],st.Length,ee.Select(x=>x["study_id"]).Distinct().Count(),primary,
                id=="reading_proficiency"?"3 cohorts / 2 reports; comparable raw reading-content association; small k and mixed test timings":
                id=="captioning"?"Addition, editing, rate and expansion are distinct; missing covariances/counts prevent broad pooling":
                "Paired contrasts estimable in two CI college samples; quiet/noisy contexts and fixed stories not exchangeable",
                "not_formal_RoB_tool; see fulltext appraisal"]);
        }
        Csv.Write(Path.Combine(root,"results","candidate_factor_counts.csv"),
            ["factor_id","factor_name","catalog_publications","supplied_publications","tagged_DHH_study_units","reported_n_sum_not_deduplicated","estimable_rows","estimable_study_units","primary_meta_cohorts","selection_status"],rows);
        Csv.Write(Path.Combine(root,"data","selected_factors.csv"),
            ["factor_id","factor_name","tagged_study_units","estimable_study_units","primary_meta_cohorts","selection_reason","quality_status"],selected);
        Csv.Write(Path.Combine(root,"results","education_distribution.csv"),["education_level","study_units","reported_n_sum_not_deduplicated"],
            s.GroupBy(x=>x["education_level"]).OrderBy(x=>x.Key).Select(g=>new object?[]{g.Key,g.Count(),g.Sum(x=>Csv.Number(x["n_dhh_unique"])??0)}));
        Csv.Write(Path.Combine(root,"results","outcome_distribution.csv"),["outcome_type","extraction_rows","estimable_rows"],
            d.GroupBy(x=>x["outcome_type"]).OrderBy(x=>x.Key).Select(g=>new object?[]{g.Key,g.Count(),g.Count(x=>e.Any(y=>y["effect_id"]==x["effect_id"]))}));
        Csv.Write(Path.Combine(root,"results","screening_distribution.csv"),["screening_status","publications"],
            p.GroupBy(x=>x["screening_status"]).OrderBy(x=>x.Key).Select(g=>new object?[]{g.Key,g.Count()}));
        Csv.Write(Path.Combine(root,"results","study_statistics.csv"),["metric","value","qualification"],
            new object?[][]{
                ["catalog_publications",p.Count,"Selected canonical evidence directory, not all search hits"],
                ["supplied_originals",m.Count,"Purchased/user-provided PDFs"],
                ["DHH_study_units",s.Count,"Separate reported cohorts/experiments; cross-report independence not universally verified"],
                ["reported_N_sum",s.Sum(x=>Csv.Number(x["n_dhh_unique"])??0),"DO NOT call unique participants; RIT series overlap unknown"],
                ["extraction_rows",d.Count,"Includes unavailable statistics and alternatives"],
                ["estimable_rows",e.Count,"Includes repeated alternatives; DO NOT treat as independent studies"],
                ["primary_cohorts",3,"Two explicitly separate school/college cohorts in P05 plus Sydney cohort P08"],
                ["primary_reports",2,"No five-study synthesis found among supplied originals"],
                ["primary_N",116,"Conditional on P05 pairwise n being complete n=48"]});
        Console.WriteLine($"Summarized {p.Count} publications, {s.Count} DHH study units; selected {selected.Count} factors.");return 0;
    }
}
