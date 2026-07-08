using DhhResearch;

try
{
    var root = Path.GetFullPath(args.Length > 1 ? args[1] : Directory.GetCurrentDirectory());
    return (args.Length > 0 ? args[0] : "inventory") switch
    {
        "inventory" => Inventory.Run(root),
        "validate" => NumericalValidation.Run(root),
        "analyze" => Analysis.Run(root),
        "search-ledger" => SearchLedger.Run(root),
        "extract-pdfs" => PdfExtraction.Run(root, args.Length > 2 ? args[2] : "pdftotext"),
        "curate" => EvidenceCuration.Run(root),
        "summarize" => EvidenceSummary.Run(root),
        "paper-export" => PaperExport.Run(root),
        "audit" => FinalAudit.Run(root),
        _ => throw new ArgumentException("Use inventory, validate, analyze, search-ledger, extract-pdfs, curate, or summarize followed by the research root.")
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
