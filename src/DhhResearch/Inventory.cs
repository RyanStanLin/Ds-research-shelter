using System.Net;
using System.Text;

namespace DhhResearch;

internal static class Inventory
{
    public static int Run(string root)
    {
        var papers = Csv.Read(Path.Combine(root, "data", "publications.csv"));
        var factors = Csv.Read(Path.Combine(root, "data", "candidate_factors.csv"));
        if (papers.Select(p => p["publication_id"]).Distinct().Count() != papers.Count)
            throw new InvalidDataException("Duplicate publication ID.");
        var dois = papers.Select(p => p["doi"].ToLowerInvariant()).Where(d => d != "na").ToArray();
        if (dois.Distinct().Count() != dois.Length) throw new InvalidDataException("Duplicate DOI; deduplicate reports first.");
        var factorIds = factors.Select(f => f["factor_id"]).ToHashSet();
        foreach (var p in papers)
        {
            if (p["publication_id"].Length == 0 || p["title"].Length == 0 || !Uri.IsWellFormedUriString(p["source_url"], UriKind.Absolute))
                throw new InvalidDataException("Missing ID/title or invalid source URL.");
            double? n = Csv.Number(p["n_dhh_abstract"]);
            if (n is < 0 || n.HasValue && n.Value != Math.Truncate(n.Value)) throw new InvalidDataException("Invalid sample size.");
            if (p["factor_tags"].Split(';').Any(id => !factorIds.Contains(id))) throw new InvalidDataException("Unknown factor tag.");
        }
        var retained = papers.Where(p => !p["screening_status"].StartsWith("excluded_", StringComparison.Ordinal)).ToArray();
        var counts = factors.Select(f =>
        {
            var linked = retained.Where(p => p["factor_tags"].Split(';').Contains(f["factor_id"])).ToArray();
            return new object?[] { f["factor_id"], f["factor_name"], linked.Length,
                linked.Count(p => p["screening_status"] == "candidate_core"),
                linked.Count(p => p["screening_status"] is "candidate_near_scope" or "awaiting_abstract_or_fulltext"),
                linked.Count(p => p["screening_status"] is "qualitative_context" or "review_context"),
                null, null, "fulltext_pending" };
        }).ToArray();
        string output = Path.Combine(root, "results");
        Directory.CreateDirectory(output);
        Csv.Write(Path.Combine(output, "candidate_factor_counts.csv"),
            ["factor_id", "factor_name", "linked_publications", "core_candidate_reports", "near_scope_or_title_only_reports",
             "context_reports", "confirmed_independent_studies", "extractable_comparable_studies", "selection_status"], counts);
        Csv.Write(Path.Combine(output, "education_distribution.csv"), ["education_level", "publications"],
            retained.GroupBy(p => p["education_level"]).OrderBy(g => g.Key).Select(g => new object?[] { g.Key, g.Count() }));
        Csv.Write(Path.Combine(output, "outcome_distribution.csv"), ["outcome_preliminary", "publications"],
            retained.GroupBy(p => p["outcome_preliminary"]).OrderBy(g => g.Key).Select(g => new object?[] { g.Key, g.Count() }));
        Csv.Write(Path.Combine(output, "screening_distribution.csv"), ["screening_status", "publications"],
            papers.GroupBy(p => p["screening_status"]).OrderBy(g => g.Key).Select(g => new object?[] { g.Key, g.Count() }));
        string requestDir = Path.Combine(root, "literature", "pdf_requests");
        Directory.CreateDirectory(requestDir);
        var batch = papers.Where(p => p["acquisition_priority"] is "A" or "B").ToArray();
        string[] requestHeaders = ["publication_id", "acquisition_priority", "authors", "year", "title", "doi", "source_url", "access_lead", "screening_note"];
        Csv.Write(Path.Combine(requestDir, "batch01.csv"), requestHeaders, batch.Select(p => requestHeaders.Select(h => (object?)p[h])));
        var markdown = new StringBuilder("# 第一批 PDF 获取清单（2026-10-14）\n\n");
        markdown.Append("本批包含 **24 篇原始研究：17 篇 A 优先和 7 篇 B 扩展核验**。请尽量一次发送；不需要为了提交齐全而支付不合理费用，暂时买不到的编号可以直接告知。A 最接近教学或课堂内容理解；B 用于核实字幕速度、视觉布局及视频理解证据能否进入研究范围。B 尚未确定纳入，采购不保证可合并。\n\n");
        markdown.Append("`PMC_free_article`、`open_access_*`、`ERIC_free_full_text` 表示索引或出版社有公开全文线索；请先从链接核查并获取 PDF，通常不必付费。我没有下载或打开任何原论文 PDF。`publisher_access_check` 只是需要核查获取渠道，不代表必须购买。建议文件命名为 `P01_Yoon_2011.pdf`。\n\n");
        markdown.Append("| 编号 | 优先级 | 论文与 DOI | 获取线索 | 需要核实的内容 |\n|---|---|---|---|---|\n");
        foreach (var p in batch)
        {
            string doi = p["doi"] == "NA" ? "无已核实 DOI" : $"[DOI](https://doi.org/{p["doi"]})";
            markdown.Append($"| {p["publication_id"]} | {p["acquisition_priority"]} | {p["authors"]} ({p["year"]}). *{p["title"]}*. {doi} | [{p["access_lead"]}]({p["source_url"]}) | {p["screening_note"]} |\n");
        }
        markdown.Append("\n三篇综述可一起发送，供补查引用和研究背景使用：P33 Pavani & Leonetti (2026), P34 Gernsbacher (2015), P35 Mealings (2022)。其中 P33/P34 有公开全文线索，不要优先付费购买综述。其余 C/D 编号暂不要求购买。\n\n");
        markdown.Append("## 上传时请保留\n\n完整论文（包括表格、附录和参考文献）；如有统计补充材料，请一起发送。仅摘要、付款截图或出版社预览不能替代全文。来源文件与后续数据会用 publication_id 关联，确认实验及样本独立性后才分配 study_id。\n\n");
        markdown.Append("## 当前暂停点\n\n需要原文核实各实验样本量、组均值/标准差、配对相关系数或差值标准差、量表方向、条件定义、重复样本及偏倚。收到 PDF 后继续，最终因素和题目仍未确定。\n");
        File.WriteAllText(Path.Combine(requestDir, "batch01.md"), markdown.ToString());
        var bibliography = new StringBuilder("# 检索目录参考文献（临时书目信息）\n\n书目信息以索引/出版社页面核对，部分字段为 NA，原文核验后修订。此目录不是已纳入论文列表。\n\n");
        foreach (var p in papers)
            bibliography.Append($"- **{p["publication_id"]}** {p["authors"]} ({p["year"]}). {p["title"]}. *{p["journal"]}*, {p["volume_issue"]}, {p["pages"]}. {(p["doi"] == "NA" ? "DOI: NA." : $"https://doi.org/{p["doi"]}")} [来源]({p["source_url"]}). 状态：{p["screening_status"]}.\n\n");
        File.WriteAllText(Path.Combine(root, "literature", "references.md"), bibliography.ToString());
        Console.WriteLine($"Validated {papers.Count} publication records and {factors.Count} factor definitions; {batch.Length} PDFs requested. Independent-study and effect counts remain unknown.");
        return 0;
    }
}
