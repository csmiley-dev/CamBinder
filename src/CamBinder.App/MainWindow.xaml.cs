using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CamBinder.Core;

namespace CamBinder.App;

public partial class MainWindow : Window
{
    private readonly IReadOnlyList<string> _pdfPaths;

    public MainWindow(IReadOnlyList<string> pdfPaths)
    {
        InitializeComponent();
        _pdfPaths = pdfPaths;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        StartPulseAnimation();

        try
        {
            var folder = Path.GetDirectoryName(_pdfPaths[0])!;
            var outputPath = OutputPathResolver.GetOutputPath(folder);

            await Task.Run(() => PdfMerger.Merge(_pdfPaths, outputPath, onBeforeSave: () => Dispatcher.Invoke(ShowCompletingColor)));

            await Task.Delay(600);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "CamBinder", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Close();
        }
    }

    private void StartPulseAnimation()
    {
        var pulse = new DoubleAnimation
        {
            From = 0.85,
            To = 1.1,
            Duration = TimeSpan.FromSeconds(0.6),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        IndicatorScale.BeginAnimation(ScaleTransform.ScaleXProperty, pulse);
        IndicatorScale.BeginAnimation(ScaleTransform.ScaleYProperty, pulse);
    }

    // Placeholder completion cue: swaps the indicator color shortly before the merged
    // PDF is written to disk. Replace with the real animation/graphic later.
    private void ShowCompletingColor()
    {
        Indicator.Fill = (Brush)FindResource("CompletingBrush");
    }
}
