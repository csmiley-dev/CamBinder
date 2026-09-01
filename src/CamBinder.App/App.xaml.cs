using System.IO;
using System.Windows;

namespace CamBinder.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var pdfPaths = e.Args
            .Where(a => Path.GetExtension(a).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            .Where(File.Exists)
            .ToArray();

        if (pdfPaths.Length == 0)
        {
            Shutdown();
            return;
        }

        var window = new MainWindow(pdfPaths);
        window.Show();
    }
}
