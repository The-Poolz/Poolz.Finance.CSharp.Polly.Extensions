using Xunit;
using FluentAssertions;

namespace Poolz.Finance.CSharp.Polly.Extensions.Tests;

public class RetryExecutorTests
{
    private const int MagicNumber = 42;

    public class Execute
    {
        protected readonly RetryExecutor _executor = new();

        [Fact]
        internal void ShouldReturnResult_WhenActionSucceeds()
        {
            var expected = Guid.NewGuid();

            var result = _executor.Execute(_ => expected);

            result.Should().Be(expected);
        }

        [Fact]
        internal void ShouldPassCancellationTokenToAction()
        {
            using var cts = new CancellationTokenSource();
            var observedToken = CancellationToken.None;

            _executor.Execute(token =>
            {
                observedToken = token;
                return MagicNumber;
            }, ct: cts.Token);

            observedToken.Should().Be(cts.Token);
        }

        [Fact]
        internal void ShouldUseProvidedOptions()
        {
            var options = new DefaultRetryStrategyOptions<int>
            {
                MaxRetryAttempts = 1
            };

            var attempts = 0;

            var act = () =>
            {
                _executor.Execute(_ =>
                {
                    attempts++;
                    throw new InvalidOperationException("failure");
                }, options);
            };

            act.Should().Throw<InvalidOperationException>();
            attempts.Should().Be(2);
        }
    }

    public class ExecuteAsync
    {
        protected readonly RetryExecutor _executor = new();

        [Fact]
        public async Task ShouldReturnResult_WhenActionSucceeds()
        {
            var result = await _executor.ExecuteAsync(_ => Task.FromResult(MagicNumber));

            result.Should().Be(MagicNumber);
        }

        [Fact]
        public async Task ShouldPassCancellationTokenToAction()
        {
            using var cts = new CancellationTokenSource();
            var observedToken = CancellationToken.None;

            await _executor.ExecuteAsync(token =>
            {
                observedToken = token;
                return Task.FromResult(string.Empty);
            }, ct: cts.Token);

            observedToken.Should().Be(cts.Token);
        }

        [Fact]
        public async Task ShouldRetryUntilSuccess()
        {
            var options = new DefaultRetryStrategyOptions<int>
            {
                MaxRetryAttempts = 3
            };
            var attempts = 0;

            var result = await _executor.ExecuteAsync(async _ =>
            {
                attempts++;
                if (attempts <= 3)
                {
                    throw new InvalidOperationException("fail");
                }

                await Task.Delay(10, _);
                return MagicNumber;
            }, options);

            attempts.Should().Be(4);
            result.Should().Be(MagicNumber);
        }
    }
}