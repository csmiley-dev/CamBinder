using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace CamBinder.Core;

public static class PdfMerger
{
    public static void Merge(IReadOnlyList<string> inputPaths, string outputPath, Action? onBeforeSave = null)
    {
        if (inputPaths.Count == 0)
            throw new ArgumentException("At least one input PDF is required.", nameof(inputPaths));

        using var output = new PdfDocument();

        foreach (var inputPath in inputPaths)
        {
            try
            {
                using var input = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
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
