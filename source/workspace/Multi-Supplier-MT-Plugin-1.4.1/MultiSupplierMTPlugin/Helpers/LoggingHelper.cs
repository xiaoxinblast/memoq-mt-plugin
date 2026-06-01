using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSupplierMTPlugin.Helpers
{
    static class LoggingHelper
    {
        private static readonly Logger _logger = new Logger();

        // 请求上下文：每次翻译请求分配一个递增 ID，关联所有相关日志行
        private static readonly AsyncLocal<RequestContext> _currentRequest = new AsyncLocal<RequestContext>();
        private static int _requestCounter;

        public static void Init(string logDir, string prefix, bool enable, LogLevel logLevel, int retentionDays,
            bool enableVerboseRuntimeLog = false, bool enableApiRequestResponseLog = false)
        {
            _logger.Init(logDir, prefix, enable, logLevel, retentionDays, enableVerboseRuntimeLog, enableApiRequestResponseLog);
        }

        /// <summary>开始一次翻译请求上下文，返回 IDisposable 用于自动清理</summary>
        public static IDisposable BeginRequest()
        {
            var id = Interlocked.Increment(ref _requestCounter);
            var ctx = new RequestContext(id, _currentRequest.Value);
            _currentRequest.Value = ctx;
            return ctx;
        }

        /// <summary>获取当前请求 ID 字符串，如 "#042"</summary>
        internal static string CurrentRequestId()
        {
            var ctx = _currentRequest.Value;
            return ctx != null ? $"#{ctx.Id:D3}" : string.Empty;
        }

        /// <summary>记录多行内容，每行缩进对齐</summary>
        public static void Multiline(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            var prefix = BuildIndentPrefix();
            foreach (var line in message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                _logger.LogRaw($"{prefix}{line}");
        }

        /// <summary>视觉分隔线，标记请求边界</summary>
        public static void Separator()
        {
            _logger.LogRaw(new string('-', 80));
        }

        private static string BuildIndentPrefix()
        {
            var reqId = CurrentRequestId();
            if (!string.IsNullOrEmpty(reqId))
                return new string(' ', 28 + reqId.Length) + "| ";
            return new string(' ', 28) + "| ";
        }

        public static void Debug(string message)  => _logger.Log(message, LogLevel.Debug);
        public static void Info(string message)   => _logger.Log(message, LogLevel.Info);
        public static void Warn(string message)   => _logger.Log(message, LogLevel.Warn);
        public static void Error(string message)  => _logger.Log(message, LogLevel.Error);
        public static void Verbose(string message) => _logger.LogSpecial(message, "VRB", _logger.EnableVerboseRuntimeLog);
        public static void Api(string message)     => _logger.LogSpecial(message, "API", _logger.EnableApiRequestResponseLog);

        public static bool TryGetLogFilePath(out string logFilePath) => _logger.TryGetLogFilePath(out logFilePath);

        public static bool Enable { get => _logger.Enable; set => _logger.Enable = value; }
        public static LogLevel MinLogLevel { get => _logger.MinLogLevel; set => _logger.MinLogLevel = value; }
        public static bool EnableVerboseRuntimeLog { get => _logger.EnableVerboseRuntimeLog; set => _logger.EnableVerboseRuntimeLog = value; }
        public static bool EnableApiRequestResponseLog { get => _logger.EnableApiRequestResponseLog; set => _logger.EnableApiRequestResponseLog = value; }
        public static void Dispose() => _logger.Dispose();

        sealed class RequestContext : IDisposable
        {
            public int Id { get; }
            private readonly RequestContext _previous;
            private bool _disposed;

            public RequestContext(int id, RequestContext previous)
            {
                Id = id;
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _currentRequest.Value = _previous;
            }
        }
    }

    sealed class Logger : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private BlockingCollection<string> _messageQueue;
        private Task _writerTask;

        private string _logDirectory;
        private string _filePrefix;
        private DateTime _currentDate;
        private int _retentionDays;
        private string _currentLogFile;
        private bool _isInitialized;

        // 级别 → 3 字符标签
        private static readonly string[] LevelTags = { "DBG", "INF", "WRN", "ERR" };

        public void Init(string logDir, string prefix, bool enable, LogLevel logLevel, int retentionDays,
            bool enableVerboseRuntimeLog, bool enableApiRequestResponseLog)
        {
            if (_isInitialized) return;

            try
            {
                Directory.CreateDirectory(logDir);

                _logDirectory = logDir;
                _filePrefix = prefix;
                _currentDate = DateTime.Today;
                _currentLogFile = GetLogFilePath(_currentDate);

                _cancellationTokenSource = new CancellationTokenSource();
                _messageQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());
                _writerTask = Task.Run(() => ProcessQueueAsync(_cancellationTokenSource.Token));

                _isInitialized = true;

                Enable = enable;
                MinLogLevel = logLevel;
                EnableVerboseRuntimeLog = enableVerboseRuntimeLog;
                EnableApiRequestResponseLog = enableApiRequestResponseLog;
                _retentionDays = retentionDays;

                Task.Run(() => CleanupOldLogsAsync());
            }
            catch
            {
                _isInitialized = false;
            }
        }

        public void Log(string message, LogLevel logLevel)
        {
            if (!_isInitialized || !Enable || logLevel < MinLogLevel) return;
            Enqueue(FormatLine(LevelTags[(int)logLevel], null, message));
        }

        public void LogSpecial(string message, string tag, bool enabled)
        {
            if (!enabled || !_isInitialized || !Enable) return;
            Enqueue(FormatLine(tag, null, message));
        }

        /// <summary>直接写入原始行（用于缩进行、分隔线等）</summary>
        public void LogRaw(string line)
        {
            if (!_isInitialized || !Enable) return;
            Enqueue(line);
        }

        private string FormatLine(string tag, string subTag, string message)
        {
            var sb = new StringBuilder();

            // 时间戳: HH:mm:ss.fff
            sb.Append(DateTime.Now.ToString("HH:mm:ss.fff"));

            // 级别标签: [INF] [WRN] 等
            sb.Append(" [").Append(tag).Append(']');

            // 请求关联 ID: [#042]
            var reqId = LoggingHelper.CurrentRequestId();
            if (!string.IsNullOrEmpty(reqId))
                sb.Append(' ').Append(reqId);

            // 消息
            sb.Append(' ').Append(message);

            return sb.ToString();
        }

        private void Enqueue(string line)
        {
            try
            {
                if (!_messageQueue.IsAddingCompleted)
                    _messageQueue.Add(line);
            }
            catch { }
        }

        public bool TryGetLogFilePath(out string logFilePath)
        {
            logFilePath = _isInitialized ? _currentLogFile : null;
            return _isInitialized;
        }

        public bool Enable { get; set; } = true;
        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;
        public bool EnableVerboseRuntimeLog { get; set; }
        public bool EnableApiRequestResponseLog { get; set; }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(_currentLogFile, true, Encoding.UTF8);

                foreach (var message in _messageQueue.GetConsumingEnumerable(token))
                {
                    var today = DateTime.Today;
                    if (today != _currentDate)
                    {
                        await writer.FlushAsync();
                        writer.Dispose();

                        _currentDate = today;
                        _currentLogFile = GetLogFilePath(_currentDate);
                        writer = new StreamWriter(_currentLogFile, true, Encoding.UTF8);
                    }

                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }
            }
            catch { }
            finally
            {
                writer?.Dispose();
            }
        }

        private string GetLogFilePath(DateTime date)
        {
            return Path.Combine(_logDirectory, $"{_filePrefix}.{date:yyyy-MM-dd}.txt");
        }

        private async Task CleanupOldLogsAsync()
        {
            if (_retentionDays < 0) return;

            try
            {
                DateTime threshold = DateTime.Today.AddDays(-_retentionDays);
                var files = Directory.GetFiles(_logDirectory, $"{_filePrefix}.*.txt");

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var datePart = Path.GetFileNameWithoutExtension(fileName).Replace(_filePrefix + ".", "");

                    if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var fileDate))
                    {
                        if (fileDate < threshold)
                        {
                            try { RecycleBinHelper.MoveToRecycleBin(file); }
                            catch { Log($"Log cleanup: failed to recycle '{file}'", LogLevel.Warn); }
                        }
                    }
                }
            }
            catch { }
        }

        public void Dispose()
        {
            if (!_isInitialized) return;

            try
            {
                _cancellationTokenSource?.Cancel();
                _messageQueue?.CompleteAdding();
                _writerTask?.Wait();

                _messageQueue?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            catch { }
            finally
            {
                _isInitialized = false;
            }
        }
    }

    enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
    }
}
