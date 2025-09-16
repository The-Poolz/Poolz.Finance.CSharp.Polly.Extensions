using Polly;
using System;
using Polly.Retry;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    public class DefaultRetryStrategyOptions<TResult> : RetryStrategyOptions<TResult>
    {
        public DefaultRetryStrategyOptions(Action<string>? log = null, Func<OnRetryArguments<TResult>, string>? format = null)
        {
            Name = "Retry";
            MaxRetryAttempts = 2;
            BackoffType = DelayBackoffType.Constant;
            UseJitter = false;
            Delay = TimeSpan.FromMilliseconds(250);
            ShouldHandle = new PredicateBuilder<TResult>().Handle<Exception>(exception => !(exception is OperationCanceledException));
            OnRetry = args =>
            {
                var ex = args.Outcome.Exception;
                if (ex == null || log == null) return default;

                var message = format?.Invoke(args) ?? $"[Retry] Attempt={args.AttemptNumber}, Delay={args.RetryDelay}, Exception={ex.GetType().Name}: {ex.Message}";
                log(message);

                return default;
            };
        }
    }
}
