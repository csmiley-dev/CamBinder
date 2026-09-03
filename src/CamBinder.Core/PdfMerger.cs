using CoreLib = PdfSharpCore.Pdf;
using CoreIO = PdfSharpCore.Pdf.IO;
using SharpLib = PdfSharp.Pdf;
using SharpIO = PdfSharp.Pdf.IO;

namespace CamBinder.Core;

// Real-world PDFs from CAD/drawing export pipelines routinely trip up one PDF parser
// or the other with unrelated bugs (PdfSharpCore vs. PDFsharp each fail on different
// files that the other opens fine — confirmed against actual customer files). Rather
// than betting on a single library, try PdfSharpCore first and fall back to PDFsharp
// for the whole batch if any file in it fails.
public static class PdfMerger
{
    public static void Merge(IReadOnlyList<string> inputPaths, string outputPath, Action? onBeforeSave = null)
    {
        if (inputPaths.Count == 0)
            throw new ArgumentException("At least one input PDF is required.", nameof(inputPaths));

        try
        {
            MergeWithPdfSharpCore(inputPaths, outputPath, onBeforeSave);
        }
        catch (Exception primaryEx)
        {
            try
            {
                MergeWithPdfSharp(inputPaths, outputPath, onBeforeSave);
            }
            catch (Exception fallbackEx)
            {
                throw new InvalidOperationException(
                    $"{primaryEx.Message}\n\n(Fallback PDF engine also failed: {fallbackEx.Message})", fallbackEx);
            }
        }
    }

    private static void MergeWithPdfSharpCore(IReadOnlyList<string> inputPaths, string outputPath, Action? onBeforeSave)
    {
        using var output = new CoreLib.PdfDocument();

        foreach (var inputPath in inputPaths)
        {
            try
            {
                using var input = CoreIO.PdfReader.Open(inputPath, CoreIO.PdfDocumentOpenMode.Import);
                foreach (var page in input.Pages)
                    output.AddPage(page);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not read \"{Path.GetFileName(inputPath)}\": {ex.Message}", ex);
            }
        }

        onBeforeSave?.Invoke();
        output.Save(outputPath);
    }

    private static void MergeWithPdfSharp(IReadOnlyList<string> inputPaths, string outputPath, Action? onBeforeSave)
    {
        using var output = new SharpLib.PdfDocument();

        foreach (var inputPath in inputPaths)
        {
            try
            {
                using var input = SharpIO.PdfReader.Open(inputPath, SharpIO.PdfDocumentOpenMode.Import);
                foreach (var page in input.Pages)
                    output.AddPage(page);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not read \"{Path.GetFileName(inputPath)}\": {ex.Message}", ex);
            }
        }

        onBeforeSave?.Invoke();
        output.Save(outputPath);
    }
}
