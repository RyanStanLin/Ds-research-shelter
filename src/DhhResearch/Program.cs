using DhhResearch;

try
{
    const string usage = "Commands: validate, analyze, summarize, audit-data, curate, search-ledger, inventory, extract-pdfs, paper-export, audit. Follow a command with the research root.";
    if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
    {
        Console.WriteLine(usage);
        return 0;
    }
    var root = Path.GetFullPath(args.Length > 1 ? args[1] : Directory.GetCurrentDirectory());
    return args[0] switch
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
        "audit-data" => DataAudit.Run(root),
        _ => throw new ArgumentException(usage)
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
