using System;
using Polly.Retry;
using System.Threading;
using System.Threading.Tasks;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    public interface IRetryExecutor
    {
        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default);
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default);
    }
}
