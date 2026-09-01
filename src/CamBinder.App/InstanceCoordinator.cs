using System.IO;
using System.IO.Pipes;

namespace CamBinder.App;

// Explorer's MultiSelectModel=Player registry setting is supposed to launch CamBinder
// once with every selected file, but in practice it's unreliable and sometimes launches
// one process per selected file instead. This coordinator makes that survivable: the
// first process to start becomes the "primary" and briefly listens for sibling processes
// launched by the same click, which hand off their file and exit quietly. The primary
// waits for a short idle gap in arrivals (or a hard cap) before finalizing the file list.
public sealed class InstanceCoordinator : IDisposable
{
    private const string MutexName = "Local\\CamBinder_SingleInstance_Mutex";
    private const string PipeName = "CamBinder_IPC";
    private static readonly TimeSpan CollectionIdleWindow = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan MaxTotalWait = TimeSpan.FromSeconds(1.5);

    private readonly Mutex _mutex;
    private readonly CancellationTokenSource _cts = new();
    private readonly object _lock = new();
    private readonly List<string> _collected = new();
    private readonly DateTime _startUtc = DateTime.UtcNow;
    private DateTime _lastArrivalUtc = DateTime.UtcNow;
    private bool _stopped;

    private InstanceCoordinator(Mutex mutex, IEnumerable<string> initialPaths)
    {
        _mutex = mutex;
        _collected.AddRange(initialPaths);
        _ = ListenLoopAsync();
    }

    public static bool TryBecomePrimary(IEnumerable<string> initialPaths, out InstanceCoordinator? coordinator)
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (createdNew)
        {
            coordinator = new InstanceCoordinator(mutex, initialPaths);
            return true;
        }

        mutex.Dispose();
        coordinator = null;
        return false;
    }

    public static void SendToPrimary(IReadOnlyList<string> pdfPaths)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000);
            using var writer = new StreamWriter(client) { AutoFlush = true };
            foreach (var path in pdfPaths)
                writer.WriteLine(path);
        }
        catch
        {
            // Primary wasn't reachable in time; nothing more a secondary instance can do.
        }
    }

    public async Task<IReadOnlyList<string>> WaitForCollectionAsync()
    {
        while (true)
        {
            TimeSpan sinceArrival, sinceStart;
            lock (_lock)
            {
                sinceArrival = DateTime.UtcNow - _lastArrivalUtc;
                sinceStart = DateTime.UtcNow - _startUtc;
            }

            if (sinceArrival >= CollectionIdleWindow || sinceStart >= MaxTotalWait)
                break;

            await Task.Delay(50).ConfigureAwait(false);
        }

        List<string> result;
        lock (_lock)
        {
            _stopped = true;
            result = _collected.ToList();
        }

        _cts.Cancel();
        return result;
    }

    // Pipe I/O deliberately avoids the WPF UI thread (ConfigureAwait(false) throughout):
    // continuations posted to a busy Dispatcher queue (window creation, animation setup)
    // were observed delaying connection acceptance by 400ms+, which was enough to drop
    // sibling instances' files past the collection window.
    private async Task ListenLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(_cts.Token).ConfigureAwait(false);

                using var reader = new StreamReader(server);
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (File.Exists(line))
                        AddPath(line);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                // Ignore a single failed connection attempt and keep listening.
            }
        }
    }

    private void AddPath(string path)
    {
        lock (_lock)
        {
            if (_stopped)
                return;

            if (!_collected.Contains(path, StringComparer.OrdinalIgnoreCase))
                _collected.Add(path);

            _lastArrivalUtc = DateTime.UtcNow;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
