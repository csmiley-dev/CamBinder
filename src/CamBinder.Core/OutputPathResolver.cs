namespace CamBinder.Core;

public static class OutputPathResolver
{
    private const string BaseName = "M-Complete";
    private const string Extension = ".pdf";

    public static string GetOutputPath(string folder)
    {
        var candidate = Path.Combine(folder, BaseName + Extension);
        if (!File.Exists(candidate))
            return candidate;

        var counter = 1;
        do
        {
            candidate = Path.Combine(folder, $"{BaseName} ({counter}){Extension}");
            counter++;
        } while (File.Exists(candidate));

        return candidate;
    }
}
