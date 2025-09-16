using Polly;
using Xunit;
using FluentAssertions;

namespace Poolz.Finance.CSharp.Polly.Extensions.Tests;

public class DefaultRetryStrategyOptionsTests
{
    public class Constructor
    {
        [Fact]
        internal void Constructor_ShouldSetExpectedDefaults()
        {
            var options = new DefaultRetryStrategyOptions<int>();

            options.Name.Should().Be(DefaultRetryConstants.DefaultName);
            options.MaxRetryAttempts.Should().Be(DefaultRetryConstants.DefaultMaxRetryAttempts);
            options.BackoffType.Should().Be(DefaultRetryConstants.DefaultBackoffType);
            options.UseJitter.Should().Be(DefaultRetryConstants.DefaultUseJitter);
            options.Delay.Should().Be(DefaultRetryConstants.DefaultDelay);
            options.ShouldHandle.Should().NotBeNull();
            options.OnRetry.Should().NotBeNull();
        }
    }

    public class Execute
    {
        private const int MagicNumber = 42;

        [Fact]
        internal void WithDefaultOptions_ShouldRetryAndLogUsingDefaultFormat()
        {
            var logMessages = new List<string>();
            var options = new DefaultRetryStrategyOptions<int>(logMessages.Add);
            var pipeline = new ResiliencePipelineBuilder<int>().AddRetry(options).Build();
            var attempt = 0;

            var result = pipeline.Execute(_ =>
            {
                attempt++;
                if (attempt <= 2)
                {
                    throw new InvalidOperationException("boom");
                }
                return MagicNumber;
            });

            result.Should().Be(MagicNumber);
            attempt.Should().Be(DefaultRetryConstants.DefaultMaxRetryAttempts);
            logMessages.Should().HaveCount(2);
            logMessages.Should().ContainInOrder(
                $"[Retry] Attempt=1, Delay={DefaultRetryConstants.DefaultDelay}, Exception=InvalidOperationException: boom",
                $"[Retry] Attempt=2, Delay={DefaultRetryConstants.DefaultDelay}, Exception=InvalidOperationException: boom"
            );
        }

        [Fact]
        internal void WithCustomOptions_ShouldUseCustomLogFormatter()
        {
            var logMessages = new List<string>();
            var options = new DefaultRetryStrategyOptions<int>(logMessages.Add, args => $"Custom {args.AttemptNumber}");
            var pipeline = new ResiliencePipelineBuilder<int>().AddRetry(options).Build();
            var attempt = 0;

            var result = pipeline.Execute(_ =>
            {
                attempt++;
                if (attempt <= 2)
                {
                    throw new InvalidOperationException("boom");
                }
                return MagicNumber;
            });

            result.Should().Be(MagicNumber);
            attempt.Should().Be(DefaultRetryConstants.DefaultMaxRetryAttempts);
            logMessages.Should().HaveCount(2);
            logMessages.Should().ContainInOrder(
                "Custom 0",
                "Custom 1"
            );
        }

        [Fact]
        internal void WithCustomOptions_ShouldNotRetryOperationCanceledException()
        {
            var logMessages = new List<string>();
            var options = new DefaultRetryStrategyOptions<int>(logMessages.Add);
            var pipeline = new ResiliencePipelineBuilder<int>().AddRetry(options).Build();
            var attempt = 0;

            Action act = () => pipeline.Execute(_ =>
            {
                attempt++;
                if(attempt == 1)
                {
                    throw new OperationCanceledException("stop");
                }
                return MagicNumber;
            });

            act.Should().Throw<OperationCanceledException>();
            attempt.Should().Be(1);
            logMessages.Should().BeEmpty();
        }

        [Fact]
        internal void WithCustomOptions_ShouldRetryWhenLoggerIsNull()
        {
            var options = new DefaultRetryStrategyOptions<int>();
            var pipeline = new ResiliencePipelineBuilder<int>().AddRetry(options).Build();
            var attempt = 0;

            var result = pipeline.Execute(_ =>
            {
                attempt++;
                if (attempt < DefaultRetryConstants.DefaultMaxRetryAttempts)
                {
                    throw new InvalidOperationException("boom");
                }
                return MagicNumber;
            });

            result.Should().Be(MagicNumber);
            attempt.Should().Be(DefaultRetryConstants.DefaultMaxRetryAttempts);
        }
    }
}