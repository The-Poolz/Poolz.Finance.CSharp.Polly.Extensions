using Polly;
using System;
using Polly.Retry;
using System.Threading;
using System.Threading.Tasks;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    public class RetryExecutor : IRetryExecutor
    {
        public TResult Execute<TResult>(Func<CancellationToken, TResult> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default)
        {
            var pipeline = CreatePipeline(options);
            return pipeline.Execute(action, ct);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, RetryStrategyOptions<TResult>? options = null, CancellationToken ct = default)
        {
            var pipeline = CreatePipeline(options);
            return await pipeline.ExecuteAsync(token => new ValueTask<TResult>(action(token)), ct).ConfigureAwait(false);
        }

        private static ResiliencePipeline<TResult> CreatePipeline<TResult>(RetryStrategyOptions<TResult>? options)
        {
            options ??= new DefaultRetryStrategyOptions<TResult>();
            return new ResiliencePipelineBuilder<TResult>()
                .AddRetry(options)
                .Build();
        }
    }
}