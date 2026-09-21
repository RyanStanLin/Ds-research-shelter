using System.Text.RegularExpressions;

namespace DhhResearch;

internal static class DataAudit
{
    public static int Run(string root)
    {
        var checks = new List<object?[]>();
        void Check(string name, Action verify)
        {
            try { verify(); checks.Add([name, "PASS", "NA"]); }
            catch (Exception ex) { checks.Add([name, "FAIL", ex.Message]); }
        }
        void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidDataException(message);
        }
        var papers = Csv.Read(Path.Combine(root, "data", "publications.csv"));
        var factors = Csv.Read(Path.Combine(root, "data", "candidate_factors.csv"));
        var manifest = Csv.Read(Path.Combine(root, "data", "fulltext_manifest.csv"));
        var appraisal = Csv.Read(Path.Combine(root, "data", "fulltext_appraisal.csv"));
        var studies = Csv.Read(Path.Combine(root, "data", "study_registry.csv"));
        var raw = Csv.Read(Path.Combine(root, "data", "extracted_data.csv"));
        var effects = Csv.Read(Path.Combine(root, "results", "effect_sizes.csv"));
        var meta = Csv.Read(Path.Combine(root, "results", "meta_analysis.csv"));
        var members = Csv.Read(Path.Combine(root, "results", "synthesis_membership.csv"));
        var paperIds = papers.Select(x => x["publication_id"]).ToHashSet();
        var factorIds = factors.Select(x => x["factor_id"]).ToHashSet();
        bool Unique(List<Dictionary<string, string>> rows, string key) =>
            rows.Count > 0 && rows.All(x => x[key] is not ("" or "NA")) &&
            rows.Select(x => x[key]).Distinct().Count() == rows.Count;

        Check("Canonical_IDs_and_DOIs", () =>
        {
            var dois = papers.Select(x => x["doi"].ToLowerInvariant()).Where(x => x != "na").ToArray();
            Require(Unique(papers, "publication_id") && Unique(factors, "factor_id") &&
                dois.Distinct().Count() == dois.Length, "Missing or duplicate publication, factor or DOI.");
        });
        Check("Source_manifest_metadata_only", () =>
        {
            Require(Unique(manifest, "publication_id") && Unique(manifest, "filename") &&
                manifest.All(x => paperIds.Contains(x["publication_id"]) &&
                    Regex.IsMatch(x["sha256"], "^[a-f0-9]{64}$") && Csv.Number(x["pages"]) > 0),
                "Invalid source manifest metadata; original PDFs are not checked by this command.");
        });
        Check("Appraisal_coverage", () => Require(Unique(appraisal, "publication_id") &&
            appraisal.Select(x => x["publication_id"]).Order().SequenceEqual(manifest.Select(x => x["publication_id"]).Order()),
            "Appraisals must cover exactly the source manifest."));
        Check("Study_and_extraction_references", () =>
        {
            Require(Unique(studies, "study_id") && Unique(raw, "effect_id"), "Duplicate or missing study/effect ID.");
            var registry = studies.ToDictionary(x => x["study_id"]);
            var sourceIds = manifest.Select(x => x["publication_id"]).ToHashSet();
            Require(studies.All(x => paperIds.Contains(x["publication_id"])) && raw.All(x =>
                registry.TryGetValue(x["study_id"], out var study) &&
                study["publication_id"] == x["publication_id"] &&
                study["independence_cluster_id"] == x["independence_cluster_id"] &&
                sourceIds.Contains(x["publication_id"]) && factorIds.Contains(x["factor_id"]) &&
                x["source_status"] == "pdf_verified" && x["source_page_table"] is not ("" or "NA")),
                "Broken study, factor, dependence or source-page reference.");
        });
        Check("Raw_numbers_and_sample_sizes", () =>
        {
            string[] numeric = "n_treatment,n_control,mean_treatment,mean_control,sd_treatment,sd_control,raw_t,raw_df,raw_F,raw_CI_lower,raw_CI_upper,paired_n,paired_r,sd_difference,cluster_count,icc,sample_size,scale_min,scale_max,raw_r,correlation_n,raw_md,raw_se".Split(',');
            foreach (var row in raw) foreach (string key in numeric) _ = Csv.Number(row[key]);
            foreach (var row in studies)
            {
                double? n = Csv.Number(row["n_dhh_unique"]);
                Require(n is null || n > 0 && n == Math.Truncate(n.Value), "Invalid reported study sample size.");
            }
        });
        Check("Eligible_effects_and_intervals", () =>
        {
            Require(Unique(effects, "effect_id") && effects.Select(x => x["effect_id"]).Order().SequenceEqual(
                raw.Where(x => x["eligible"] == "yes").Select(x => x["effect_id"]).Order()),
                "Computed effects must match the eligible extraction rows.");
            var extraction = raw.ToDictionary(x => x["effect_id"]);
            Require(effects.All(x => new[] { "publication_id", "study_id", "factor_id", "effect_family", "pool_role" }
                .All(key => x[key] == extraction[x["effect_id"]][key]) && Csv.Number(x["variance"]) > 0 &&
                Csv.Number(x["estimate"]) is not null && Csv.Number(x["ci_lower"]) <= Csv.Number(x["display_estimate"]) &&
                Csv.Number(x["display_estimate"]) <= Csv.Number(x["ci_upper"])), "Invalid effect identity, variance or interval.");
        });
        Check("Analysis_decisions_cover_extraction", () =>
        {
            var decisions = Csv.Read(Path.Combine(root, "results", "analysis_decisions.csv"));
            Require(Unique(decisions, "id") && decisions.Select(x => x["id"]).Order().SequenceEqual(raw.Select(x => x["effect_id"]).Order()),
                "Missing or duplicate analysis decision.");
            var extraction = raw.ToDictionary(x => x["effect_id"]);
            Require(decisions.All(x => x["decision"] == (extraction[x["id"]]["eligible"] == "yes" ? "computed" : "not_computed")),
                "Analysis decision disagrees with eligibility.");
        });
        Check("Synthesis_membership_and_weights", () =>
        {
            Require(Unique(meta, "synthesis_id") && members.Count > 0 &&
                members.Select(x => x["synthesis_id"]).ToHashSet().SetEquals(meta.Select(x => x["synthesis_id"])),
                "Unknown or empty synthesis membership.");
            var extraction = raw.ToDictionary(x => x["effect_id"]);
            var computedIds = effects.Select(x => x["effect_id"]).ToHashSet();
            foreach (var synthesis in meta)
            {
                var group = members.Where(x => x["synthesis_id"] == synthesis["synthesis_id"]).ToList();
                Require(Unique(group, "effect_id") && Unique(group, "independence_cluster_id") && group.All(x =>
                    computedIds.Contains(x["effect_id"]) && x["independence_cluster_id"] == extraction[x["effect_id"]]["independence_cluster_id"] &&
                    extraction[x["effect_id"]]["effect_family"] == "Fisher_z" && Csv.Number(x["weight_percent"]) > 0),
                    "Dependent, unknown or incompatible effect in a synthesis.");
                Require(group.Count == Csv.Number(synthesis["k"]) && group.Select(x => extraction[x["effect_id"]]["publication_id"]).Distinct().Count() == Csv.Number(synthesis["reports"]) &&
                    group.Sum(x => Csv.Number(extraction[x["effect_id"]]["correlation_n"])!.Value) == Csv.Number(synthesis["n_observations"]) &&
                    Math.Abs(group.Sum(x => Csv.Number(x["weight_percent"])!.Value) - 100) < 1e-8,
                    "Synthesis counts, observations or weights disagree.");
            }
        });
        Check("Selected_factors", () =>
        {
            var selected = Csv.Read(Path.Combine(root, "data", "selected_factors.csv"));
            Require(selected.Count is >= 1 and <= 3 && Unique(selected, "factor_id") && selected.All(x => factorIds.Contains(x["factor_id"])),
                "Select one to three known factors.");
        });
        Check("Numerical_validation_record", () =>
        {
            var numerical = Csv.Read(Path.Combine(root, "validation", "numerical_checks.csv"));
            Require(numerical.Count == 10 && Unique(numerical, "check_name") && numerical.All(x => x["status"] == "PASS"),
                "Run numerical validation first; all ten checks must pass.");
        });
        Directory.CreateDirectory(Path.Combine(root, "validation"));
        Csv.Write(Path.Combine(root, "validation", "data_audit.csv"), ["check", "status", "diagnostic"], checks);
        int failures = checks.Count(x => (string?)x[1] == "FAIL");
        Console.WriteLine($"Data audit: {checks.Count - failures} passed, {failures} failed. Source PDFs and manuscript excluded.");
        return failures == 0 ? 0 : 1;
    }
}
