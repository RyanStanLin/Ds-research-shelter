# DHH evidence analysis

C# analysis of classroom comprehension among deaf and hard-of-hearing students: 43 publications, 30 study units and 42 extraction rows. The evidence review selects three factors; only reading associations support exploratory pooling. Evidence records retain their original October 2026 dates. Public repository imports and maintenance use their actual commit dates.

Use .NET SDK 10.0.401; no external NuGet packages are required.

```sh
bash scripts/reproduce.sh
```

This rebuilds statistics and runs ten numerical checks plus ten public data checks. Existing charts are included. To redraw final charts, install `results/figures/requirements.txt` in a Python environment and run `python results/figures/plot_final.py`.

Manuscripts, purchased PDFs and extracted full text are excluded. `extract-pdfs`, `paper-export`, `audit` and `scripts/build_documents.py` need those private inputs. `inventory` recreates an earlier checkpoint and overwrites some final summaries; use `reproduce.sh` for the final analysis. Archived delivery checks describe the original private project; the public data audit checks metadata, not original-PDF hashes.

<details>
<summary>File index</summary>

| File | Purpose |
|---|---|
| [.gitignore](.gitignore) | Exclude private inputs and build output. |
| [NuGet.Config](NuGet.Config) | Disable external NuGet package sources. |
| [README.md](README.md) | Repository overview, commands and file index. |
| [data/DICTIONARY.md](data/DICTIONARY.md) | Define fields, missing values and estimands. |
| [data/candidate_factors.csv](data/candidate_factors.csv) | Catalog all twelve candidate factors. |
| [data/extracted_data.csv](data/extracted_data.csv) | Store verified quantitative extraction rows. |
| [data/fulltext_appraisal.csv](data/fulltext_appraisal.csv) | Record study limitations and inclusion decisions. |
| [data/fulltext_manifest.csv](data/fulltext_manifest.csv) | Identify original files and archived hashes. |
| [data/publication_screening.csv](data/publication_screening.csv) | Record screening decisions and reasons. |
| [data/publications.csv](data/publications.csv) | Catalog publication metadata and source links. |
| [data/selected_factors.csv](data/selected_factors.csv) | Document the three evidence-derived factors. |
| [data/study_registry.csv](data/study_registry.csv) | Track cohorts, samples and possible dependence. |
| [global.json](global.json) | Pin the .NET SDK version. |
| [literature/pdf_requests/batch01.csv](literature/pdf_requests/batch01.csv) | Archive the original acquisition request table. |
| [literature/pdf_requests/batch01.md](literature/pdf_requests/batch01.md) | Archive acquisition priorities and instructions. |
| [literature/references.md](literature/references.md) | List references for the evidence directory. |
| [literature/retrieval_ledger.csv](literature/retrieval_ledger.csv) | Track retrieval appearances and publication matches. |
| [literature/searches/batch02.json](literature/searches/batch02.json) | Archive this batch of search results. |
| [literature/searches/batch03.json](literature/searches/batch03.json) | Archive this batch of search results. |
| [literature/searches/batch04.json](literature/searches/batch04.json) | Archive this batch of search results. |
| [literature/searches/batch05.json](literature/searches/batch05.json) | Archive this batch of search results. |
| [literature/searches/batch06.json](literature/searches/batch06.json) | Archive this batch of search results. |
| [literature/searches/batch07.json](literature/searches/batch07.json) | Archive this batch of search results. |
| [literature/searches/batch08.json](literature/searches/batch08.json) | Archive this batch of search results. |
| [literature/searches/batch09.json](literature/searches/batch09.json) | Archive this batch of search results. |
| [literature/searches/batch10.json](literature/searches/batch10.json) | Archive this batch of search results. |
| [literature/searches/batch11.json](literature/searches/batch11.json) | Archive this batch of search results. |
| [literature/searches/batch13.json](literature/searches/batch13.json) | Archive this batch of search results. |
| [literature/searches/batch14.json](literature/searches/batch14.json) | Archive this batch of search results. |
| [literature/searches/batch17.json](literature/searches/batch17.json) | Archive this batch of search results. |
| [literature/searches/batch18.json](literature/searches/batch18.json) | Archive this batch of search results. |
| [literature/searches/batch19.json](literature/searches/batch19.json) | Archive this batch of search results. |
| [literature/searches/batch21.json](literature/searches/batch21.json) | Archive this batch of search results. |
| [literature/searches/batch23.json](literature/searches/batch23.json) | Archive this batch of search results. |
| [literature/searches/batch25.json](literature/searches/batch25.json) | Archive this batch of search results. |
| [literature/searches/batch27.json](literature/searches/batch27.json) | Archive this batch of search results. |
| [literature/searches/final_gaps.json](literature/searches/final_gaps.json) | Archive searches for remaining evidence gaps. |
| [literature/searches/final_metadata_verification.json](literature/searches/final_metadata_verification.json) | Archive final bibliographic verification. |
| [literature/searches/final_targeted_primary.json](literature/searches/final_targeted_primary.json) | Archive targeted primary-study searches. |
| [literature/searches/initial_discovery_queries.json](literature/searches/initial_discovery_queries.json) | Archive initial discovery queries. |
| [literature/searches/statistics_references.json](literature/searches/statistics_references.json) | Archive statistical method reference searches. |
| [literature/searches/verification01.json](literature/searches/verification01.json) | Archive the first metadata verification search. |
| [protocol/checkpoint_2026-10-14.md](protocol/checkpoint_2026-10-14.md) | Archive the initial research checkpoint. |
| [protocol/completion_record.md](protocol/completion_record.md) | Record the original project completion scope. |
| [protocol/factor_feasibility_pending.md](protocol/factor_feasibility_pending.md) | Archive feasibility decisions before full-text assessment. |
| [protocol/final_factor_selection.md](protocol/final_factor_selection.md) | Explain final factor selection and comparability. |
| [protocol/requirements.md](protocol/requirements.md) | Retain the original project requirements. |
| [protocol/review_protocol.md](protocol/review_protocol.md) | Define screening, extraction and analysis rules. |
| [protocol/search_limits_and_reporting.md](protocol/search_limits_and_reporting.md) | Disclose search coverage and reporting limits. |
| [results/analysis_decisions.csv](results/analysis_decisions.csv) | Explain every computed or omitted extraction. |
| [results/analysis_status.md](results/analysis_status.md) | Summarize the current analysis scope. |
| [results/candidate_factor_counts.csv](results/candidate_factor_counts.csv) | Count evidence coverage by candidate factor. |
| [results/education_distribution.csv](results/education_distribution.csv) | Summarize study units by education level. |
| [results/effect_sizes.csv](results/effect_sizes.csv) | Store estimates, variances and confidence intervals. |
| [results/figures/factor_distribution.pdf](results/figures/factor_distribution.pdf) | Final candidate-factor evidence coverage chart. |
| [results/figures/factor_distribution.png](results/figures/factor_distribution.png) | Final candidate-factor evidence coverage chart. |
| [results/figures/factor_distribution.svg](results/figures/factor_distribution.svg) | Final candidate-factor evidence coverage chart. |
| [results/figures/plot_final.py](results/figures/plot_final.py) | Render final charts from C# results. |
| [results/figures/plot_inventory.py](results/figures/plot_inventory.py) | Render the historical discovery inventory chart. |
| [results/figures/preliminary_factor_distribution.pdf](results/figures/preliminary_factor_distribution.pdf) | Historical preliminary evidence coverage chart. |
| [results/figures/preliminary_factor_distribution.png](results/figures/preliminary_factor_distribution.png) | Historical preliminary evidence coverage chart. |
| [results/figures/preliminary_factor_distribution.svg](results/figures/preliminary_factor_distribution.svg) | Historical preliminary evidence coverage chart. |
| [results/figures/presentation_contrasts.pdf](results/figures/presentation_contrasts.pdf) | Separate teaching-condition effect estimates chart. |
| [results/figures/presentation_contrasts.png](results/figures/presentation_contrasts.png) | Separate teaching-condition effect estimates chart. |
| [results/figures/presentation_contrasts.svg](results/figures/presentation_contrasts.svg) | Separate teaching-condition effect estimates chart. |
| [results/figures/reading_forest.pdf](results/figures/reading_forest.pdf) | Reading-association synthesis forest plot. |
| [results/figures/reading_forest.png](results/figures/reading_forest.png) | Reading-association synthesis forest plot. |
| [results/figures/reading_forest.svg](results/figures/reading_forest.svg) | Reading-association synthesis forest plot. |
| [results/figures/requirements-lock.txt](results/figures/requirements-lock.txt) | Archive the complete plotting environment. |
| [results/figures/requirements.txt](results/figures/requirements.txt) | Pin the direct plotting dependency. |
| [results/meta_analysis.csv](results/meta_analysis.csv) | Store primary and sensitivity synthesis results. |
| [results/outcome_distribution.csv](results/outcome_distribution.csv) | Count extracted and estimable outcome types. |
| [results/screening_distribution.csv](results/screening_distribution.csv) | Summarize publication screening statuses. |
| [results/study_statistics.csv](results/study_statistics.csv) | Summarize corpus counts with qualifications. |
| [results/synthesis_membership.csv](results/synthesis_membership.csv) | Record synthesis dependence keys and weights. |
| [scripts/build_documents.py](scripts/build_documents.py) | Format privately supplied manuscript as DOCX/LaTeX. |
| [scripts/reproduce.sh](scripts/reproduce.sh) | Build, validate, analyze, summarize and audit. |
| [src/DhhResearch/Analysis.cs](src/DhhResearch/Analysis.cs) | Calculate design-specific effects and reading syntheses. |
| [src/DhhResearch/Csv.cs](src/DhhResearch/Csv.cs) | Read, validate and write invariant CSV. |
| [src/DhhResearch/DataAudit.cs](src/DhhResearch/DataAudit.cs) | Check public data integrity and synthesis membership. |
| [src/DhhResearch/DhhResearch.csproj](src/DhhResearch/DhhResearch.csproj) | Configure the dependency-free console project. |
| [src/DhhResearch/EvidenceCuration.cs](src/DhhResearch/EvidenceCuration.cs) | Recreate manually verified extraction tables. |
| [src/DhhResearch/EvidenceSummary.cs](src/DhhResearch/EvidenceSummary.cs) | Generate final evidence coverage summaries. |
| [src/DhhResearch/FinalAudit.cs](src/DhhResearch/FinalAudit.cs) | Audit private manuscript and original-PDF delivery. |
| [src/DhhResearch/Inventory.cs](src/DhhResearch/Inventory.cs) | Recreate the historical pre-extraction inventory. |
| [src/DhhResearch/NumericalValidation.cs](src/DhhResearch/NumericalValidation.cs) | Verify estimators against numerical reference cases. |
| [src/DhhResearch/PaperExport.cs](src/DhhResearch/PaperExport.cs) | Assemble tables/references around private manuscript text. |
| [src/DhhResearch/PdfExtraction.cs](src/DhhResearch/PdfExtraction.cs) | Extract local PDFs with external pdftotext. |
| [src/DhhResearch/Program.cs](src/DhhResearch/Program.cs) | Dispatch explicit commands and show help. |
| [src/DhhResearch/SearchLedger.cs](src/DhhResearch/SearchLedger.cs) | Normalize archived retrieval records. |
| [src/DhhResearch/Statistics.cs](src/DhhResearch/Statistics.cs) | Implement effect estimators and exploratory pooling. |
| [validation/README.md](validation/README.md) | Describe numerical verification and estimator provenance. |
| [validation/artifact_hashes.csv](validation/artifact_hashes.csv) | Archive hashes of the withheld manuscript formats. |
| [validation/artifact_qa.md](validation/artifact_qa.md) | Archive original document and source-table checks. |
| [validation/data_audit.csv](validation/data_audit.csv) | Report the public data integrity checks. |
| [validation/delivery_audit.csv](validation/delivery_audit.csv) | Archive the original private-delivery audit results. |
| [validation/latex_compile.json](validation/latex_compile.json) | Archive the original LaTeX compilation status. |
| [validation/numerical_checks.csv](validation/numerical_checks.csv) | Report the ten numerical reference checks. |

</details>
