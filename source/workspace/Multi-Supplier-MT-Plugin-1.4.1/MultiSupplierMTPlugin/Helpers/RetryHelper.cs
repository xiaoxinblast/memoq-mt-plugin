using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LLH = MultiSupplierMTPlugin.Localized.LocalizedHelper;
using LLK = MultiSupplierMTPlugin.Localized.LocalizedKeyCommon;

namespace MultiSupplierMTPlugin.Helpers
{
    class RetryHelper
    {
        private readonly int _failedTimeoutMs;
        private readonly int _retryWaitingMs;
        private readonly int _numberOfRetries;

        public RetryHelper(int failedTimeoutMs, int retryWaitingMs, int numberOfRetries)
        {
            this._failedTimeoutMs = Math.Max(failedTimeoutMs, 0);
            this._retryWaitingMs = Math.Max(retryWaitingMs, 0);
            this._numberOfRetries = Math.Max(numberOfRetries, 0);
        }

        public async Task<T> ExecWithRetryAsync<T>(Func<CancellationToken, Task<T>> action)
        {
            var exceptions = new List<Exception>();

            for (int attempt = 0; attempt <= _numberOfRetries; attempt++)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    LoggingHelper.Verbose($"Retry attempt {attempt + 1}/{_numberOfRetries + 1} started. TimeoutMs={_failedTimeoutMs}, RetryWaitingMs={_retryWaitingMs}");

                    if (_failedTimeoutMs <= 0)
                    {
                        var resultWithoutTimeout = await action(cts.Token);
                        LoggingHelper.Verbose($"Retry attempt {attempt + 1}/{_numberOfRetries + 1} succeeded.");
                        return resultWithoutTimeout;
                    }

                    var mainTask = action(cts.Token);
                    var timeoutTask = Task.Delay(_failedTimeoutMs, cts.Token);

                    var completedTask = await Task.WhenAny(mainTask, timeoutTask);

                    if (completedTask == mainTask)
                    {
                        var result = await mainTask; // 正常完成
                        LoggingHelper.Verbose($"Retry attempt {attempt + 1}/{_numberOfRetries + 1} succeeded.");
                        return result;
                    }

                    // 超时处理：取消任务并等待其响应
                    cts.Cancel();
                    try { await mainTask; } catch { /* 忽略取消或异常 */ }

                    throw new TimeoutException(LLH.G(LLK.RetryHelper_Exception_TimeoutMsg, _failedTimeoutMs));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    LoggingHelper.Verbose($"Retry attempt {attempt + 1}/{_numberOfRetries + 1} failed. {ex.GetType().Name}: {ex.Message}");

                    if (attempt < _numberOfRetries)
                    {
                        LoggingHelper.Verbose($"Waiting {_retryWaitingMs} ms before next retry attempt.");
                        await Task.Delay(_retryWaitingMs);
                    }
                }
                finally
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }

            LoggingHelper.Verbose($"All retry attempts failed. Attempts={_numberOfRetries + 1}");

            throw new AggregateException(
                LLH.G(LLK.RetryHelper_Exception_AllAttemptFailMsg, _numberOfRetries + 1),
                exceptions
            );
        }

        public Task ExecWithRetryAsync(Func<CancellationToken, Task> action)
        {
            return ExecWithRetryAsync(async (ct) => { await action(ct); return true; });
        }

        public T ExecWithRetry<T>(Func<T> action)
        {
            return ExecWithRetryAsync(ct => Task.FromResult(action())).GetAwaiter().GetResult();
        }

        public void ExecWithRetry(Action action)
        {
            ExecWithRetry(() => { action(); return true; });
        }
    }
}
