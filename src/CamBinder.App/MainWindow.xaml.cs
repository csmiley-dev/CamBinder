using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CamBinder.Core;

namespace CamBinder.App;

public partial class MainWindow : Window
{
    private readonly InstanceCoordinator _coordinator;

    public MainWindow(InstanceCoordinator coordinator)
    {
        InitializeComponent();
        _coordinator = coordinator;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        StartPulseAnimation();

        try
        {
            var pdfPaths = (await _coordinator.WaitForCollectionAsync())
                .OrderBy(Path.GetFileName, NaturalFileNameComparer.Instance)
                .ToList();
            var folder = Path.GetDirectoryName(pdfPaths[0])!;
            var outputPath = OutputPathResolver.GetOutputPath(folder);

            await Task.Run(() => PdfMerger.Merge(pdfPaths, outputPath, onBeforeSave: () => Dispatcher.Invoke(ShowCompletingColor)));

            await Task.Delay(600);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "CamBinder", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _coordinator.Dispose();
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
