# Final data dictionary

All CSV files are UTF-8 with invariant decimal-point numbers. `NA` means unreported, inapplicable or not defensibly derivable, never zero. Source values and valid derived values are explicitly distinguished.

## Evidence directory

- `publications.csv`: 43 canonical records, not 43 included studies. Stable Pxx IDs, authors, issue year, title, DOI/source URL, journal, initial abstract-only population/design/outcome fields, screening decision, acquisition and overlap leads, and semicolon-separated factor tags. P39 is intentionally absent because its proposed record duplicated P26's DOI. Bibliographic unknowns are omitted from formatted references rather than fabricated.
- `candidate_factors.csv`: the original eleven plus the data-driven learner factor reading proficiency. A tag denotes relevance and may be indirect; it does not assert manipulation or a causal effect.
- `publication_screening.csv`: index-level versus original-text status. `fulltext_unavailable` means no supplied original; no effect extraction from that report.
- `fulltext_manifest.csv`: filename, SHA256, PDF page count, verification date/status. `doi_verified=title_only` distinguishes a title match from a DOI found in the first two PDF pages; citations in the reference list are never used to identify the source PDF.
- `study_registry.csv`: 30 reported DHH cohorts/experiments with stage, DHH n, site, unavailable recruitment dates, source pages and dependence concerns. `n_dhh_unique` is the cohort's DHH count, not a globally deduplicated population. Across-report overlap in the RIT series is unresolved.
- `fulltext_appraisal.csv`: allocation clarity, measurement limitations, confounding/dependence, and analytic decision for each of 19 supplied reports. This is not an independently duplicated formal RoB score.
- `selected_factors.csv`: final factors and feasibility rationale. The narrative decision report explains why factors with related evidence were not pooled.

## Extraction rows

One row is a defined contrast or association, not a new study. The 42 rows include noncomputable summaries, dependent alternative correlations and indirect contextual results.

| Fields | Meaning |
|---|---|
| effect_id, publication_id, study_id | Unique extraction row; report and reported cohort identifiers |
| independence_cluster_id | Within-cohort dependence key; one estimate per key per synthesis. Does not establish cross-report independence |
| factor_id | Primary analytic factor for this contrast, different from broader publication tags |
| synthesis_id, pool_role | Standalone grouping or primary/alternative/context role; actual pooling membership is exported separately |
| design | independent_groups, correlation, paired, paired_t, paired_F, reported_contrast, or documented noncomputable/ambiguous design |
| effect_family | Hedges_g, Fisher_z, g_z, MD, or NA for unavailable effects |
| outcome_type, timepoint, intervention, comparator | Defined measurement/condition labels. Reading pooling is exploratory across related tests and mixed P05 review timings |
| n_treatment, n_control | True independent arm counts; not populated as two groups for paired analyses |
| mean_treatment, mean_control, sd_treatment, sd_control | Transcribed means and genuine SDs in original units; missing values remain NA |
| paired_n, paired_r, sd_difference | Within-person count, actual reported correlation and reported difference SD if available. No assumed paired correlation |
| raw_t, raw_df, raw_F | Original test statistics. Ward's original t=-1.973 is retained; t_direction declares its opposite orientation. E32 retains F(1,18)=11.58 and converts sqrt(F) only during analysis, not in the raw_t field |
| t_direction | intervention_minus_comparator or comparator_minus_intervention. Missing direction is rejected for computed test-statistic contrasts |
| raw_r, correlation_n, n_basis | Original Pearson correlation and its analysis n. P05 n=48 is derived from all recorded reading/outcome measures, not a separately printed pairwise n; this assumption is flagged |
| raw_md, raw_se, raw_CI_lower, raw_CI_upper | Original planned contrast and its SE/interval; the caption-rate interval is retained as reported |
| cluster_count, icc | Inapplicable/unavailable here; no clustered-data effect is computed |
| score_direction | Positive contrasts favor the named intervention; positive correlations mean greater reading-related proficiency accompanies higher comprehension |
| source_status, source_page_table | Supplied original reviewed and exact PDF/article page/table; mandatory before computation |
| eligible, reason, extraction_note | yes means calculable under its documented assumptions, NOT automatically poolable or low risk of bias |
| education_level, sample_size, factor_category, measurement, condition_details | Experiment population and definition; sample_size is total DHH cohort n and may include more arms than a row compares |
| scale_units, scale_min, scale_max | Original test units and known theoretical bounds; no silent rescaling |

## Computed outputs

`effect_sizes.csv` stores the estimate on its analytic scale, sampling variance, displayed estimate/CI, role and explicit estimator. Fisher z is back-transformed to r for display; its variance stays on the z scale. Hedges g and difference-standardized g_z have different SD denominators. Their displayed intervals are approximate Wald intervals. MD uses reported SE and original CI.

`meta_analysis.csv` has one primary and six sensitivity syntheses. Columns include cohort k, distinct reports, conditional observation count, pooled z/SE, pooled r, conventional CI, tau² on the z scale, Q, I², and modified KH interval/scale. Sensitivities reuse participants. `synthesis_membership.csv` gives selected row IDs and random-effects weights; duplicate dependence keys within a synthesis are rejected.

Factor/education descriptive totals are tagged study units and reported n sums, explicitly **not deduplicated unique persons**. Missing verified global totals are not invented. Counts of estimable rows include alternatives and context and must not be called independent study counts.
