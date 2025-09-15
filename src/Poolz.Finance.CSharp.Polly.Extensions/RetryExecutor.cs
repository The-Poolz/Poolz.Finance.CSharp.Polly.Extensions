using Polly;
using System;
using Polly.Retry;
using System.Threading;
using System.Threading.Tasks;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    public class RetryExecutor : IRetryExecutor
    {
        public TResult Execute<TResult>(Func<CancellationToken, Task<TResult>> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default)
        {
            return ExecuteAsync(action, options, ct).GetAwaiter().GetResult();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default)
        {
            options ??= new DefaultRetryStrategyOptions<TResult>();

            var pipeline = new ResiliencePipelineBuilder<TResult>()
                .AddRetry(options)
                .Build();

            return await pipeline.ExecuteAsync(token => new ValueTask<TResult>(action(token)), ct);
        }
    }
}
