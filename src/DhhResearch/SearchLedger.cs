using System.Text.Json;
using System.Text.RegularExpressions;

namespace DhhResearch;

internal static class SearchLedger
{
    public static int Run(string root)
    {
        var rows = new List<object?[]>();
        var known = Csv.Read(Path.Combine(root, "data", "publications.csv"));
        string directory = Path.Combine(root, "literature", "searches");
        foreach (string path in Directory.GetFiles(directory, "*.json").Order())
        {
            using var parsed=JsonDocument.Parse(File.ReadAllText(path));
            var document = parsed.RootElement.ValueKind==JsonValueKind.String
                ? new Dictionary<string,JsonElement>{["response"]=parsed.RootElement.Clone(),["date"]=JsonSerializer.SerializeToElement("2026-10-14")}
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path))!;
            if (!document.TryGetValue("response", out var response) || response.ValueKind != JsonValueKind.String)
            {
                if (document.TryGetValue("hits", out var previousHits))
                {
                    foreach (var hit in previousHits.EnumerateArray())
                    {
                        string url = hit.GetProperty("url").GetString()!;
                        string? report = known.FirstOrDefault(p => p["source_url"].TrimEnd('/') == url.TrimEnd('/'))?["publication_id"];
                        rows.Add([hit.GetProperty("retrieval_id").GetString(), Path.GetFileName(path), hit.GetProperty("title").GetString(), url,
                            report, report is null ? "awaiting_triage" : "linked_to_curated_report"]);
                    }
                }
                continue;
            }
            var matches = Regex.Matches(response.GetString()!, @"(?m)^([^\r\n]*) \((https?://[^\r\n]*)\)\r?\n[^\r\n]*\[wordlim");
            var hits = new List<Dictionary<string, string>>();
            int index = 0;
            foreach (Match match in matches)
            {
                string title = match.Groups[1].Value.Trim();
                string url = match.Groups[2].Value;
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) continue;

                if (uri.Query.Contains("X-Amz", StringComparison.OrdinalIgnoreCase) || uri.Query.Contains("Signature", StringComparison.OrdinalIgnoreCase))
                    url = uri.GetLeftPart(UriPartial.Path);
                string id = $"{Path.GetFileNameWithoutExtension(path)}-{++index:D3}";
                hits.Add(new() { ["retrieval_id"] = id, ["title"] = title.Length == 0 ? "Untitled source" : title, ["url"] = url });
                string? report = known.FirstOrDefault(p => p["source_url"].TrimEnd('/') == url.TrimEnd('/'))?["publication_id"];
                rows.Add([id, Path.GetFileName(path), title, url, report, report is null ? "awaiting_triage" : "linked_to_curated_report"]);
            }
            document.Remove("response");
            document["hits"] = JsonSerializer.SerializeToElement(hits);
            document["capture_note"] = JsonSerializer.SerializeToElement("Search result titles and URLs only; snippets/full text not reproduced. Unmatched hits still require screening. Signed URL query credentials removed.");
            File.WriteAllText(path, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        Csv.Write(Path.Combine(root, "literature", "retrieval_ledger.csv"),
            ["retrieval_id", "search_record", "title", "url", "publication_id_exact_url_match", "triage_status"], rows);
        Console.WriteLine($"Recorded {rows.Count} search-result retrievals. Exact source-URL matches do not replace DOI/title deduplication or screening.");
        return 0;
    }
}
