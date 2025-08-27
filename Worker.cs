namespace SQLAuditWatcherJsonService;

using System.Diagnostics;
using Microsoft.SqlServer.XEvent.XELite;
using System.Text;
using System.Text.Json;
using System.IO;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly EventLog _eventLog;
    private readonly Dictionary<string, long> _fileSizes = new();
    private readonly TimeSpan _pollInterval;
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly string _logFilePath;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        _inputPath = configuration.GetValue<string>("Watcher:InputPath", @"C:\\SQL Audit Logs");
        _outputPath = configuration.GetValue<string>("Watcher:OutputPath", @"C:\\SQL Audit Logs");
        _logFilePath = configuration.GetValue<string>("Watcher:LogFile", Path.Combine(_outputPath, "SQLAuditWatcherJson.log"));
        _pollInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Watcher:PollIntervalSeconds", 5));

        const string eventSource = "SQLAuditWatcherJson";
        const string logName = "Application";
        _eventLog = new EventLog(logName, ".", eventSource);

        if (!EventLog.SourceExists(eventSource))
        {
            try
            {
                EventLog.CreateEventSource(eventSource, logName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create event source {Source}. Run 'New-EventLog -LogName Application -Source {Source}' once with administrative privileges.", eventSource, eventSource);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var file in Directory.GetFiles(_inputPath, "*.sqlaudit"))
        {
            try
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _fileSizes[file] = fs.Length;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to read length of {File}", file);
                _fileSizes[file] = 0;
            }
            _ = ProcessAuditFileAsync(file);
        }

        _eventLog.WriteEntry($"Monitoring {_inputPath} using filestream polling");
        _logger.LogInformation("Monitoring {InputPath} using filestream polling", _inputPath);
        LogToFile($"Monitoring {_inputPath} using filestream polling");

        return base.StartAsync(cancellationToken);
    }


    private async Task ProcessAuditFileAsync(string path)
    {
        if (!File.Exists(path) || Path.GetExtension(path) != ".sqlaudit")
            return;

        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);

        var txtPath = Path.Combine(_outputPath, Path.GetFileNameWithoutExtension(path) + ".txt");

        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var rows = AuditDecoder.Decode(fs, path);
        var columns = rows.SelectMany(r => r.Keys).ToHashSet();

        var ordered = new List<string> { "event_name", "timestamp" };
        ordered.AddRange(columns.Where(c => c != "event_name" && c != "timestamp"));

        await using var writer = new StreamWriter(txtPath, false, Encoding.UTF8);
        foreach (var row in rows)
        {
            var obj = new Dictionary<string, string>();
            foreach (var col in ordered)
                obj[col] = row.TryGetValue(col, out var v) ? v : string.Empty;
            await writer.WriteLineAsync(JsonSerializer.Serialize(obj));
        }

        _logger.LogInformation("Exported JSON lines file {Txt}", txtPath);
        _eventLog.WriteEntry($"Exported JSON lines file {txtPath}");
        LogToFile($"Exported JSON lines file {txtPath}");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _eventLog.WriteEntry("Watcher stopped");
        _logger.LogInformation("Watcher stopped");
        LogToFile("Watcher stopped");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var file in Directory.GetFiles(_inputPath, "*.sqlaudit"))
            {
                long length;
                try
                {
                    using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    length = fs.Length;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to read length of {File}", file);
                    continue;
                }
                if (!_fileSizes.TryGetValue(file, out var known) || length > known)
                {
                    _fileSizes[file] = length;
                    try
                    {
                        await ProcessAuditFileAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process audit file {File}", file);
                        _eventLog.WriteEntry($"Failed to process {file}: {ex.Message}", EventLogEntryType.Error);
                        LogToFile($"Failed to process {file}: {ex.Message}");
                    }
                }
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
    }

    private void LogToFile(string message)
    {
        try
        {
            var line = $"{DateTime.Now:O} {message}{Environment.NewLine}";
            var dir = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllText(_logFilePath, line);
        }
        catch
        {
            // Ignore logging failures
        }
    }
}