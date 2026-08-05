namespace DhhResearch;

internal static class Analysis
{
    private const double Z=1.959963984540054;
    private sealed record Computed(Dictionary<string,string> Row, Effect Effect, string Family, double Display, double Lower, double Upper);
    public static int Run(string root)
    {
        var rows=Csv.Read(Path.Combine(root,"data","extracted_data.csv"));
        var papers=Csv.Read(Path.Combine(root,"data","publications.csv")).Select(x=>x["publication_id"]).ToHashSet();
        var factors=Csv.Read(Path.Combine(root,"data","candidate_factors.csv")).Select(x=>x["factor_id"]).ToHashSet();
        var manifest=Csv.Read(Path.Combine(root,"data","fulltext_manifest.csv")).Select(x=>x["publication_id"]).ToHashSet();
        var registry=Csv.Read(Path.Combine(root,"data","study_registry.csv")).ToDictionary(x=>x["study_id"]);
        var ids=new HashSet<string>();var effects=new List<Computed>();var decisions=new List<object?[]>();
        double Number(Dictionary<string,string> row,string k)=>Csv.Number(row[k])??throw new InvalidDataException($"Missing {k}: {row["effect_id"]}");
        int N(Dictionary<string,string> row,string k)
        {
            double n=Number(row,k);
            if(n<2||n>int.MaxValue||n!=Math.Truncate(n))throw new InvalidDataException("Invalid sample size.");
            return (int)n;
        }
        foreach(var row in rows)
        {
            if(!ids.Add(row["effect_id"])||row["effect_id"]=="NA")throw new InvalidDataException("Duplicate/missing extraction ID.");
            if(!papers.Contains(row["publication_id"])||!factors.Contains(row["factor_id"])||!registry.ContainsKey(row["study_id"]))
                throw new InvalidDataException("Unknown publication, factor or study.");
            if(registry[row["study_id"]]["publication_id"]!=row["publication_id"])throw new InvalidDataException("Study/publication mismatch.");
            if(!manifest.Contains(row["publication_id"])||row["source_status"]!="pdf_verified"||row["source_page_table"] is "" or "NA")
                throw new InvalidDataException("Original PDF provenance required.");
            if(row["eligible"]!="yes"){decisions.Add([row["effect_id"],"not_computed",row["extraction_note"]]);continue;}
            Effect e; string family=row["effect_family"];
            string expected=row["design"] switch{"independent_groups"=>"Hedges_g","correlation"=>"Fisher_z","paired_t" or "paired_F" or "paired"=>"g_z","reported_contrast"=>"MD",_=>"unsupported"};
            if(family!=expected)throw new InvalidDataException("Design/effect-family mismatch.");
            switch(row["design"])
            {
                case "independent_groups":
                    e=Statistics.HedgesG(N(row,"n_treatment"),Number(row,"mean_treatment"),Number(row,"sd_treatment"),N(row,"n_control"),Number(row,"mean_control"),Number(row,"sd_control"));break;
                case "correlation":e=Statistics.FisherZ(N(row,"correlation_n"),Number(row,"raw_r"));break;
                case "paired_t":
                case "paired_F":
                    int n=N(row,"paired_n");
                    if(Number(row,"raw_df")!=n-1)throw new InvalidDataException("Paired t df must equal n-1.");
                    double t=row["design"]=="paired_F"?Math.Sqrt(Number(row,"raw_F")):Number(row,"raw_t");
                    if(row["t_direction"]=="comparator_minus_intervention")t=-t;
                    else if(row["t_direction"]!="intervention_minus_comparator")throw new InvalidDataException("Explicit t direction required.");
                    e=Statistics.PairedFromT(n,t);break;
                case "paired":
                    e=Statistics.Paired(N(row,"paired_n"),Number(row,"mean_treatment"),Number(row,"sd_treatment"),Number(row,"mean_control"),Number(row,"sd_control"),Number(row,"paired_r"));break;
                case "reported_contrast":
                    double se=Number(row,"raw_se");if(se<=0)throw new InvalidDataException("Positive contrast SE required.");
                    e=new Effect(Number(row,"raw_md"),se*se);break;
                default:throw new InvalidDataException("Unsupported design selected for analysis.");
            }
            if(row["score_direction"]=="lower_better")e=e with{G=-e.G};
            else if(row["score_direction"]!="higher_better")throw new InvalidDataException("Score direction required.");
            double lo=e.G-Z*Math.Sqrt(e.Variance),hi=e.G+Z*Math.Sqrt(e.Variance),display=e.G;
            if(family=="Fisher_z"){display=Math.Tanh(e.G);lo=Math.Tanh(lo);hi=Math.Tanh(hi);}
            if(family=="MD"){lo=Number(row,"raw_CI_lower");hi=Number(row,"raw_CI_upper");}
            effects.Add(new(row,e,family,display,lo,hi));
            decisions.Add([row["effect_id"],"computed",row["pool_role"]+"; "+family]);
        }
        var pooled=new List<object?[]>();var membership=new List<object?[]>();
        void Pool(string id,IEnumerable<Computed> chosen,string note)
        {
            var m=chosen.ToArray();if(m.Length<2)throw new InvalidDataException("Synthesis requires two independent units.");
            if(m.Select(x=>x.Row["independence_cluster_id"]).Distinct().Count()!=m.Length)throw new InvalidDataException("Dependent estimates in synthesis.");
            if(m.Any(x=>x.Family!="Fisher_z"||x.Row["factor_id"]!="reading_proficiency"||x.Row["outcome_type"]!="content_comprehension"))
                throw new InvalidDataException("Only compatible reading/content associations enter this synthesis.");
            var r=Statistics.RandomEffects(m.Select(x=>x.Effect).ToArray());
            var small=Statistics.SmallKInterval(m.Select(x=>x.Effect).ToArray(),r);
            pooled.Add([id,"reading_proficiency",r.K,m.Select(x=>x.Row["publication_id"]).Distinct().Count(),m.Sum(x=>N(x.Row,"correlation_n")),r.Pooled,r.StandardError,Math.Tanh(r.Pooled),Math.Tanh(r.Lower),Math.Tanh(r.Upper),r.TauSquared,r.Q,r.I2Percent,"DL_Wald_Fisher_z_exploratory",note,Math.Tanh(small.Lower),Math.Tanh(small.Upper),small.Scale]);
            double total=m.Sum(x=>1/(x.Effect.Variance+r.TauSquared));
            foreach(var x in m)membership.Add([id,x.Row["effect_id"],x.Row["independence_cluster_id"],100/(x.Effect.Variance+r.TauSquared)/total]);
        }
        Computed Get(string id)=>effects.Single(x=>x.Row["effect_id"]==id);
        var primary=effects.Where(x=>x.Row["pool_role"]=="primary").ToArray();
        Pool("reading_primary",primary,"Three cohorts in two reports; unadjusted associations; mixed review timing in P05; pairwise n=48 inferred; few-study CI/tau uncertainty.");
        Pool("reading_rate90",[Get("R01"),Get("R02"),Get("R06")],"Substitute 90 wpm for 120 wpm, same Tyler cohort.");
        Pool("reading_rate180",[Get("R01"),Get("R02"),Get("R07")],"Substitute 180 wpm for 120 wpm, same Tyler cohort.");
        Pool("reading_interpretation",[Get("R04"),Get("R05"),Get("R03")],"Replace CPrint correlations with interpretation correlations; changed support context.");
        foreach(var omitted in primary)Pool("reading_leave_"+omitted.Row["effect_id"],primary.Where(x=>x!=omitted),"Leave one cohort out; exploratory k=2.");
        string output=Path.Combine(root,"results");Directory.CreateDirectory(output);
        Csv.Write(Path.Combine(output,"effect_sizes.csv"),["effect_id","publication_id","study_id","factor_id","effect_family","estimate","variance","display_estimate","ci_lower","ci_upper","pool_role","estimator"],effects.Select(x=>new object?[]{x.Row["effect_id"],x.Row["publication_id"],x.Row["study_id"],x.Row["factor_id"],x.Family,x.Effect.G,x.Effect.Variance,x.Display,x.Lower,x.Upper,x.Row["pool_role"],x.Family switch{"Fisher_z"=>"atanh_r_V1_over_nminus3","Hedges_g"=>"independent_J_approx_statsmodels_variance","g_z"=>"SMCC_J_approx_LS_variance","MD"=>"reported_MD_SE_and_original_CI",_=>"NA"}}));
        Csv.Write(Path.Combine(output,"meta_analysis.csv"),["synthesis_id","factor_id","k","reports","n_observations","pooled_z","se_z","pooled_r","ci_lower_r","ci_upper_r","tau_squared_z","Q","I2_percent","method","note","mKH_ci_lower_r","mKH_ci_upper_r","mKH_scale"],pooled);
        Csv.Write(Path.Combine(output,"synthesis_membership.csv"),["synthesis_id","effect_id","independence_cluster_id","weight_percent"],membership);
        Csv.Write(Path.Combine(output,"analysis_decisions.csv"),["id","decision","reason"],decisions);
        File.WriteAllText(Path.Combine(output,"analysis_status.md"),$"# Analysis status\n\n{rows.Count} extraction rows; {effects.Count} estimable rows including alternatives and contextual evidence; one primary synthesis and {pooled.Count-1} sensitivity syntheses. Sensitivity results reuse the primary participants and are not independent replications.\n\nOnly reading associations are pooled. Standalone Hedges g, paired difference-standardized g_z, and MD are distinct estimands and never pooled together. Standardized-contrast intervals use a large-sample approximation; caption-rate interval is reported. P05 pairwise n inferred from complete cohort measurement.\n");
        Console.WriteLine($"Extracted {rows.Count}; estimable {effects.Count}; primary cohorts {primary.Length}; sensitivities {pooled.Count-1}.");return 0;
    }
}
