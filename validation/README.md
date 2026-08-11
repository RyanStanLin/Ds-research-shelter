# Numerical verification

The dependency-free custom C# validation runner is invoked from the research root:

```sh
dotnet run --project src/DhhResearch -- validate .
```

It returns a nonzero exit code for any failed check and writes `numerical_checks.csv`. This project has no MSTest/xUnit/NUnit package; do not mistake `dotnet test` with zero discovery for numerical validation.

## Declared independent-groups estimator

Let N = n1+n0 and df=N−2. The pooled SD is sqrt(((n1−1)SD1²+(n0−1)SD0²)/df). Define J ≈ 1−3/(4N−9), g=J*(mean1−mean0)/SDpooled, and V(g)=N/(n1*n0)+g²/[2(N−3.94)]. This matches the explicit correction and variance in [statsmodels effectsize_smd](https://www.statsmodels.org/stable/_modules/statsmodels/stats/meta_analysis.html), whose tests compare these values to R `meta`. This is an approximation, not the exact gamma-function correction. Different variance estimators (for example, metafor LS/UB) need not return identical values and must not be silently mixed.

## Reference provenance and tolerance

The six worked-example rows and expected g/variance values are numerical facts in `TestEffSmdMeta.test_smd` in the [official statsmodels tests](https://github.com/statsmodels/statsmodels/blob/main/statsmodels/stats/tests/test_meta.py), viewed 2026-10-14. The source identifies the example as from *Applied Meta-Analysis with R* and records R `meta` comparison values. They are checked to absolute tolerance 1e-12. The Kacker nine-estimate reference in `TestMetaK1.test_tau_kacker` supplies DL tau ≈0.5359 (tolerance 0.001 due to rounding). These inputs are not DHH studies and are absent from `data/` and research figures.

The DL implementation also uses a separately derived symmetric example (effects −1,0,+1 with variance .25), for which Q=8, tau²=.75, pooled effect=0, SE=sqrt(1/3), I²=75%, and Wald limits ±1.131585734076172. Identical effects check truncation to zero. Reversed groups check effect direction and variance preservation. Invalid summaries, zero variance, nonfinite inputs, and fewer than two estimates are rejected.

| Requirement | Evidence |
|---|---|
| g and variance match an external worked example, including unequal n | `HedgesG_PublishedSixStudyReference_MatchesRMeta` |
| Group reversal changes sign and preserves variance | `HedgesG_ReverseGroups_ReversesSignPreservesVariance` |
| Invalid independent-group summaries are rejected | `HedgesG_InvalidSummaries_Rejects` |
| DL pooled effect, SE, CI, Q, tau² and I² match a closed-form case | `RandomEffects_AnalyticHeterogeneousExample_MatchesAllStatistics` |
| No observed heterogeneity truncates tau²/I² to zero | `RandomEffects_IdenticalEffects_TruncatesTauAndI2ToZero` |
| DL tau matches a published standard example | `RandomEffects_KackerReference_MatchesPublishedDLTau` |
| Invalid variance/nonfinite/singleton/empty meta inputs are rejected | `RandomEffects_InvalidOrSingletonInput_Rejects` |

The report records Passed: 7, Failed: 0, Skipped: 0 on the first execution. This verifies the basic numerical routines, not the future empirical extraction or suitability of particular papers. Paired estimators and full-text data validation still require further work after original PDFs arrive.

## Final paired and correlation estimators

After original-text assessment, the implementation added Fisher z = atanh(r), V=1/(n−3), as specified in [metafor escalc documentation](https://wviechtb.github.io/metafor/reference/escalc.html) and its [official source](https://raw.githubusercontent.com/wviechtb/metafor/master/R/escalc.r). Displayed estimates and confidence limits are transformed back with tanh; variances remain on the z scale.

For paired differences, g_z = J(n−1) * t/sqrt(n), J≈1−3/[4(n−1)−1]. A t can be reconstructed from the actual reported paired condition SDs and r: SDdiff²=SD1²+SD0²−2rSD1SD0, t=(M1−M0)sqrt(n)/SDdiff. The LS variance is 1/n+g_z²/(2n), following the documented SMCC convention. This variance convention and approximate J are declared; they are not claimed to equal exact gamma-correction results. E32 converts only a two-level within-person F(1,18) to a signed sqrt(F); no omnibus F is converted. E18 preserves the original negative t and reverses its comparator-minus-intervention orientation during analysis.

The analytic paired fixture n25/t2 has dz=.4 and J=92/95: g_z=.38736842105263157 and V=.04300108587257618. Equal marginal SD=2.5 and r=.5 imply SDdiff=2.5, supplying an independent raw-summary route. Invalid correlations and zero difference variance are rejected. The Fisher fixture n20/r=.5 yields z=ln(3)/2=.5493061443340548 and V=1/17.

Modified Knapp–Hartung is an interval sensitivity around the same DL point estimate/tau²: q*=max(1, Σw(y−mu)²/(k−1)); half-width=t(k−1,.975)*SE*sqrt(q*). The implementation deliberately supports only the actual k2 or k3 analyses, using closed-form t quantiles for df1 (Cauchy) and df2. It rejects other k rather than silently using a normal quantile. Reference: [Röver et al. 2015](https://doi.org/10.1186/s12874-015-0091-1); the variance-floor formula is also explicitly shown in [this primary methodological paper](https://link.springer.com/article/10.1186/s12874-022-01779-3). Quantiles agree with the [NIST t table](https://www.itl.nist.gov/div898/handbook/eda/section3/eda3672.htm) (df2 rounded 4.303). The symmetric fixture checks q*=1 and interval ±2.484137711719546 to 1e-9; this tolerance accommodates the rounded numerical inverse-t reference, not uncertainty in empirical extraction. An identical-effects fixture checks the floor q*=1.

Final execution: **Passed 10, Failed 0, Skipped 0**. The earlier seven-check report above describes the pre-PDF checkpoint and is superseded by numerical_checks.csv. Passing checks validates declared numerical routines; it does not establish unbiased primary studies or an exhaustive literature search.
