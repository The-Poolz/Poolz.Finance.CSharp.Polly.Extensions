using Polly;
using System;
using Polly.Retry;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    public class DefaultRetryStrategyOptions<TResult> : RetryStrategyOptions<TResult>
    {
        public DefaultRetryStrategyOptions()
        {
            Name = "Retry";
            MaxRetryAttempts = 2;
            BackoffType = DelayBackoffType.Constant;
            UseJitter = false;
            Delay = TimeSpan.FromMilliseconds(250);
            ShouldHandle = new PredicateBuilder<TResult>().Handle<Exception>(_ => true);
        }
    }
}
