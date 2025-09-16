using Polly;
using System;

namespace Poolz.Finance.CSharp.Polly.Extensions
{
    internal static class DefaultRetryConstants
    {
        public const string DefaultName = "Retry";
        public const int DefaultMaxRetryAttempts = 3;
        public const DelayBackoffType DefaultBackoffType = DelayBackoffType.Constant;
        public const bool DefaultUseJitter = false;
        public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(250);
    }
}
