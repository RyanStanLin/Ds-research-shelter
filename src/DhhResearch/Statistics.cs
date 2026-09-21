namespace DhhResearch;

internal sealed record Effect(double G, double Variance);
internal sealed record MetaResult(int K, double Pooled, double StandardError, double Lower, double Upper,
    double TauSquared, double Q, double I2Percent);

internal static class Statistics
{
    public static Effect FisherZ(int n, double r)
    {
        if (n <= 3 || !double.IsFinite(r) || Math.Abs(r) >= 1)
            throw new ArgumentException("Fisher z requires n > 3 and -1 < r < 1.");
        return new Effect(0.5 * Math.Log((1 + r) / (1 - r)), 1.0 / (n - 3));
    }

    // 配对结果使用差值标准差。
    public static Effect PairedFromT(int n, double t)
    {
        if (n < 3 || !double.IsFinite(t)) throw new ArgumentException("Invalid paired contrast.");
        double j = 1 - 3.0 / (4 * (n - 1) - 1);
        double g = j * t / Math.Sqrt(n);
        return new Effect(g, 1.0 / n + g * g / (2 * n));
    }

    public static Effect Paired(int n, double m1, double s1, double m0, double s0, double r)
    {
        if (n < 3 || !double.IsFinite(r) || Math.Abs(r) > 1 ||
            new[] { m1, m0, s1, s0 }.Any(x => !double.IsFinite(x)) || s1 < 0 || s0 < 0)
            throw new ArgumentException("Invalid paired summaries.");
        double vd = s1*s1 + s0*s0 - 2*r*s1*s0;
        if (vd <= 0 || !double.IsFinite(vd)) throw new ArgumentException("Nonpositive SD of differences.");
        return PairedFromT(n, (m1-m0) * Math.Sqrt(n / vd));
    }

    // 小样本区间仅支持两个或三个队列。
    public static (double Lower,double Upper,double Scale) SmallKInterval(IReadOnlyList<Effect> effects,MetaResult result)
    {
        if(effects.Count is not (2 or 3)||result.K!=effects.Count)
            throw new ArgumentException("Small-k sensitivity supports exactly 2 or 3 estimates.");
        double q=effects.Sum(e=>Math.Pow(e.G-result.Pooled,2)/(e.Variance+result.TauSquared))/(effects.Count-1);
        double scale=Math.Max(1,q);
        double critical=effects.Count==2?Math.Tan(.475*Math.PI):.95*Math.Sqrt(2/(1-.95*.95));
        double half=critical*result.StandardError*Math.Sqrt(scale);
        return(result.Pooled-half,result.Pooled+half,scale);
    }

    // 独立组使用合并标准差。
    public static Effect HedgesG(int n1, double mean1, double sd1, int n0, double mean0, double sd0)
    {
        if (n1 < 2 || n0 < 2 || !double.IsFinite(mean1) || !double.IsFinite(mean0) ||
            !double.IsFinite(sd1) || !double.IsFinite(sd0) || sd1 < 0 || sd0 < 0)
            throw new ArgumentException("Independent-group summaries must be finite, n >= 2, SD >= 0.");
        double total = (double)n1 + n0;
        double pooledVariance = ((n1 - 1.0) * sd1 * sd1 + (n0 - 1.0) * sd0 * sd0) / (total - 2);
        if (pooledVariance <= 0 || !double.IsFinite(pooledVariance))
            throw new ArgumentException("Pooled SD must be positive and finite.");
        double j = 1 - 3 / (4 * total - 9);
        double g = j * (mean1 - mean0) / Math.Sqrt(pooledVariance);
        double v = total / n1 / n0 + g * g / (2 * (total - 3.94));
        if (!double.IsFinite(g) || !double.IsFinite(v) || v <= 0)
            throw new ArgumentException("Effect or sampling variance is invalid.");
        return new Effect(g, v);
    }

    public static MetaResult RandomEffects(IReadOnlyList<Effect> effects)
    {
        if (effects.Count < 2) throw new ArgumentException("Pooling requires at least two independent estimates.");
        if (effects.Any(e => !double.IsFinite(e.G) || !double.IsFinite(e.Variance) || e.Variance <= 0))
            throw new ArgumentException("Each effect needs a finite estimate and a positive finite variance.");
        var weights = effects.Select(e => 1 / e.Variance).ToArray();
        double sumW = weights.Sum();
        double meanFixed = effects.Select((e, i) => weights[i] * e.G).Sum() / sumW;
        double q = effects.Select((e, i) => weights[i] * Math.Pow(e.G - meanFixed, 2)).Sum();
        double c = sumW - weights.Sum(w => w * w) / sumW;
        if (!double.IsFinite(c) || c <= 0) throw new ArgumentException("Numerically invalid weights.");
        double tau2 = Math.Max(0, (q - (effects.Count - 1)) / c);
        var rw = effects.Select(e => 1 / (e.Variance + tau2)).ToArray();
        double pooled = effects.Select((e, i) => rw[i] * e.G).Sum() / rw.Sum();
        double se = Math.Sqrt(1 / rw.Sum());
        double i2 = q == 0 ? 0 : Math.Max(0, (q - (effects.Count - 1)) / q) * 100;
        const double z = 1.959963984540054;
        var result = new MetaResult(effects.Count, pooled, se, pooled - z * se, pooled + z * se, tau2, q, i2);
        if (new[] { result.Pooled, result.StandardError, result.Lower, result.Upper, result.TauSquared, result.Q, result.I2Percent }.Any(x => !double.IsFinite(x)))
            throw new ArgumentException("Nonfinite meta-analysis result.");
        return result;
    }
}
