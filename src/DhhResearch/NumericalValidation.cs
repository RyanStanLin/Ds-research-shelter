namespace DhhResearch;

internal static class NumericalValidation
{
    public static int Run(string root)
    {
        var checks = new (string Name, Action Check)[]
        {
            ("HedgesG_PublishedSixStudyReference_MatchesRMeta", PublishedExample),
            ("HedgesG_ReverseGroups_ReversesSignPreservesVariance", ReverseGroups),
            ("HedgesG_InvalidSummaries_Rejects", InvalidSummaries),
            ("RandomEffects_AnalyticHeterogeneousExample_MatchesAllStatistics", Heterogeneous),
            ("RandomEffects_IdenticalEffects_TruncatesTauAndI2ToZero", Homogeneous),
            ("RandomEffects_KackerReference_MatchesPublishedDLTau", Kacker),
            ("RandomEffects_InvalidOrSingletonInput_Rejects", InvalidMeta),
            ("FisherZ_AnalyticReference_MatchesAndRejectsInvalid", Fisher),
            ("PairedSMCC_AnalyticReference_UsesDifferenceSD", Paired),
            ("ModifiedKH_SymmetricReference_AndVarianceFloor", SmallK)
        };
        var rows = new List<object?[]>();
        int failed = 0;
        foreach (var (name, check) in checks)
        {
            try { check(); rows.Add([name, "PASS", "NA"]); }
            catch (Exception ex) { failed++; rows.Add([name, "FAIL", ex.Message]); }
        }
        Directory.CreateDirectory(Path.Combine(root, "validation"));
        Csv.Write(Path.Combine(root, "validation", "numerical_checks.csv"), ["check_name", "status", "diagnostic"], rows);
        Console.WriteLine($"Passed: {checks.Length - failed}, Failed: {failed}, Skipped: 0");
        return failed == 0 ? 0 : 1;
    }

    private static void Near(double expected, double actual, double tolerance = 1e-12)
    {
        if (!double.IsFinite(actual) || Math.Abs(expected - actual) > tolerance)
            throw new Exception($"Expected {expected:R}; actual {actual:R}; tolerance {tolerance:R}");
    }

    private static void Reject(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new Exception("Expected ArgumentException.");
    }

    private static void PublishedExample()
    {

        (int n1, double m1, double s1, int n0, double m0, double s0, double g, double v)[] cases =
        [
            (60, 94, 22, 60, 92, 20, .09452437336063831, .03337182573880991),
            (65, 98, 21, 65, 92, 22, .27735640148036095, .03107434965484927),
            (40, 98, 28, 40, 88, 26, .36654634845797818, .05088322525353587),
            (200, 94, 19, 200, 82, 17, .66438509989113559, .01055724834741877),
            (50, 98, 21, 45, 88, 22, .46180797677414176, .04339324466573324),
            (85, 96, 21, 85, 92, 22, .18516464424648887, .02363264537147130)
        ];
        foreach (var x in cases)
        {
            var result = Statistics.HedgesG(x.n1, x.m1, x.s1, x.n0, x.m0, x.s0);
            Near(x.g, result.G); Near(x.v, result.Variance);
        }
    }

    private static void ReverseGroups()
    {
        var positive = Statistics.HedgesG(30, 12, 3, 24, 9, 4);
        var negative = Statistics.HedgesG(24, 9, 4, 30, 12, 3);
        Near(-positive.G, negative.G); Near(positive.Variance, negative.Variance);
    }

    private static void InvalidSummaries()
    {
        Reject(() => Statistics.HedgesG(1, 12, 3, 24, 9, 4));
        Reject(() => Statistics.HedgesG(30, double.NaN, 3, 24, 9, 4));
        Reject(() => Statistics.HedgesG(30, 12, -3, 24, 9, 4));
        Reject(() => Statistics.HedgesG(30, 12, 0, 24, 9, 0));
    }

    private static void Heterogeneous()
    {
        var r = Statistics.RandomEffects([new(-1, .25), new(0, .25), new(1, .25)]);
        Near(0, r.Pooled); Near(8, r.Q); Near(.75, r.TauSquared); Near(75, r.I2Percent);
        Near(.5773502691896257, r.StandardError);
        Near(-1.131585734076172, r.Lower); Near(1.131585734076172, r.Upper);
    }

    private static void Homogeneous()
    {
        var r = Statistics.RandomEffects([new(.2, .1), new(.2, .2), new(.2, .3)]);
        Near(.2, r.Pooled); Near(0, r.TauSquared); Near(0, r.I2Percent);
        Near(.2335496832484569, r.StandardError);
    }

    private static void Kacker()
    {
        double[] y = [61, 61.4, 62.21, 62.3, 62.34, 62.6, 62.7, 62.84, 65.9];
        double[] v = [.2025, 1.21, .09, .2025, .3844, .5625, .0676, .0225, 1.8225];
        var r = Statistics.RandomEffects(y.Zip(v).Select(x => new Effect(x.First, x.Second)).ToArray());
        Near(.5359, Math.Sqrt(r.TauSquared), .001);
    }

    private static void InvalidMeta()
    {
        Reject(() => Statistics.RandomEffects([]));
        Reject(() => Statistics.RandomEffects([new(.2, .1)]));
        Reject(() => Statistics.RandomEffects([new(.2, 0), new(.4, .1)]));
        Reject(() => Statistics.RandomEffects([new(double.PositiveInfinity, .1), new(.4, .1)]));
    }

    private static void Fisher()
    {

        var r=Statistics.FisherZ(20,.5);
        Near(.5493061443340548,r.G);Near(.058823529411764705,r.Variance);
        Near(.5,Math.Tanh(r.G));
        Reject(()=>Statistics.FisherZ(3,.5));Reject(()=>Statistics.FisherZ(20,1));
    }

    private static void Paired()
    {

        var r=Statistics.PairedFromT(25,2);
        Near(.38736842105263157,r.G);Near(.04300108587257618,r.Variance);

        var s=Statistics.Paired(25,2,2.5,1,2.5,.5);
        Near(r.G,s.G);Near(r.Variance,s.Variance);
        Reject(()=>Statistics.Paired(25,2,2.5,1,2.5,1));
        Reject(()=>Statistics.Paired(25,2,2.5,1,2.5,1.1));
    }

    private static void SmallK()
    {
        Effect[] x=[new(-1,.25),new(0,.25),new(1,.25)];
        var ci=Statistics.SmallKInterval(x,Statistics.RandomEffects(x));

        Near(1,ci.Scale);Near(-2.484137711719546,ci.Lower,1e-9);Near(2.484137711719546,ci.Upper,1e-9);
        Effect[] same=[new(.2,.1),new(.2,.2)];
        Near(1,Statistics.SmallKInterval(same,Statistics.RandomEffects(same)).Scale);
    }
}
