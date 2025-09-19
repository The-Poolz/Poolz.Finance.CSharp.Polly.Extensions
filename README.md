# Poolz.Finance.CSharp.Polly.Extensions

Poolz.Finance.CSharp.Polly.Extensions is a lightweight helper library that adds ready-to-use retry primitives on top of [Polly](https://www.thepollyproject.org/).
It targets **.NET Standard 2.1**, so it can be consumed from modern .NET and .NET Core applications, background services, and libraries.

## Features

- Sensible defaults for `RetryStrategyOptions<T>` via `DefaultRetryStrategyOptions<T>`.
- Built-in logging hook with optional custom formatter for retry attempts.
- Ignores `OperationCanceledException` by default so cooperative cancellation is preserved.
- `IRetryExecutor`/`RetryExecutor` wrapper to execute synchronous and asynchronous delegates without manually composing a `ResiliencePipeline<T>`.
- Works seamlessly with existing Polly pipelines and can be registered in dependency injection containers.

### Default retry configuration

`DefaultRetryStrategyOptions<T>` ships with the following values out of the box:

| Setting | Default value |
| --- | --- |
| Name | `"Retry"` |
| Max retry attempts | `3` |
| Backoff type | `DelayBackoffType.Constant` |
| Initial delay | `TimeSpan.FromMilliseconds(250)` |
| Use jitter | `false` |
| Retryable exceptions | Any exception except `OperationCanceledException` |

A logging callback can be supplied, as well as a custom formatter that receives Polly's `OnRetryArguments<T>` and returns the message to emit. When no formatter is provided the library uses the built-in message: `"[Retry] Attempt={n}, Delay={delay}, Exception={type}: {message}"`.

## Installation

```bash
dotnet add package Poolz.Finance.CSharp.Polly.Extensions
```

The library depends on `Polly` (currently 8.6.x). If you build from source, run `dotnet restore` followed by `dotnet build` or `dotnet test`.

## Getting started

### Creating a retry pipeline with the defaults

```csharp
using Polly;
using Poolz.Finance.CSharp.Polly.Extensions;

var options = new DefaultRetryStrategyOptions<HttpResponseMessage>(Console.WriteLine);
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(options)
    .Build();

var response = await pipeline.ExecuteAsync(async token =>
{
    // Replace with your real operation.
    return await httpClient.SendAsync(request, token);
}, CancellationToken.None);
```

The `log` delegate (`Console.WriteLine` in the sample) will be invoked for every retry attempt. When no logger is provided the retries still occur silently.

### Executing work via `RetryExecutor`

```csharp
using Poolz.Finance.CSharp.Polly.Extensions;

IRetryExecutor retryExecutor = new RetryExecutor();

var user = await retryExecutor.ExecuteAsync(async token =>
{
    return await httpClient.GetFromJsonAsync<UserDto>("/users/42", cancellationToken: token);
});

var invoice = retryExecutor.Execute(_ =>
{
    // Synchronous operations are supported too.
    return invoiceService.GenerateInvoice();
});
```

`RetryExecutor` automatically constructs a `ResiliencePipeline<T>` using the provided options (or the defaults when none are passed) and forwards the `CancellationToken` that you supply.

### Customising retry behaviour

```csharp
using Poolz.Finance.CSharp.Polly.Extensions;

var options = new DefaultRetryStrategyOptions<int>(
    log: message => logger.LogWarning(message),
    format: args =>
        $"Attempt {args.AttemptNumber + 1} failed after {args.RetryDelay.TotalMilliseconds} ms"
)
{
    MaxRetryAttempts = 5,
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true
};
```

You can further tweak the retry strategy by updating the properties inherited from Polly's `RetryStrategyOptions<T>`. The supplied `format` delegate is optional—omit it to use the default message.

### Dependency injection registration

```csharp
services.AddSingleton<IRetryExecutor, RetryExecutor>();
```

Registering the executor in your DI container makes it trivial to consume resilient execution from any service that requires it.

## Development

Clone the repository and execute the tests to validate your changes:

```bash
dotnet test
```

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/The-Poolz/Poolz.Finance.CSharp.Polly.Extensions/blob/master/LICENSE) file for more information.