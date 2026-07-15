using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DhhResearch;

internal static class PdfExtraction
{
    public static int Run(string root, string executable)
    {
        var publications = Csv.Read(Path.Combine(root, "data", "publications.csv"));
        string output = Path.Combine(root, "literature", "fulltext");
        Directory.CreateDirectory(output);
        var manifest = new List<object?[]>();
        foreach (string file in Directory.GetFiles(Path.Combine(root, "source_papers"), "*.pdf").Order())
        {
            var start = new ProcessStartInfo(executable) { RedirectStandardError = true, UseShellExecute = false };
            string textPath = Path.Combine(output, Path.GetFileNameWithoutExtension(file) + ".txt");
            foreach (string argument in new[] { "-layout", "-enc", "UTF-8", file, textPath }) start.ArgumentList.Add(argument);
            using var process = Process.Start(start) ?? throw new IOException("Could not start PDF text extractor.");
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new IOException($"PDF extraction failed for {file}: {errors}");
            string content = File.ReadAllText(textPath);
            string frontMatter = string.Join("\n", content.Split('\f').Take(2));
            string normalizedFront = Normalize(frontMatter);
            var doiMatch = publications.FirstOrDefault(p => p["doi"] != "NA" &&
                Regex.Replace(frontMatter, @"\s+", "").Contains(p["doi"], StringComparison.OrdinalIgnoreCase));
            var match = doiMatch ?? publications.FirstOrDefault(p => normalizedFront.Contains(Normalize(p["title"]), StringComparison.Ordinal));
            string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
            int pages = content.Count(c => c == '\f');
            manifest.Add([match?["publication_id"], Path.GetFileName(file), hash, doiMatch is not null ? "yes" : match is not null ? "title_only" : "pending_title_match",
                "user_supplied_verify_version", pages, "2026-10-14", "text_extracted_full_read_pending"]);
            Console.WriteLine($"{Path.GetFileName(file)}: {pages} pages; {content.Length} characters; {match?["publication_id"] ?? "title_match_pending"}");
        }
        Csv.Write(Path.Combine(root, "data", "fulltext_manifest.csv"),
            ["publication_id", "filename", "sha256", "doi_verified", "version", "pages", "verified_date", "extraction_status"], manifest);
        return 0;
    }

    private static string Normalize(string text) => new(text.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
