using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genesis.Producto.Util.Library.Interfaces;
using Genesis.Producto.Util.Library.Options;
using Polly;
using Polly.Retry;

namespace Genesis.Producto.Util.Library.Common;
public class RetryPolicyExecutor : IRetryPolicyExecutor
{
    private readonly RetryPolicyOptions _retryPolicyOptions;

    public RetryPolicyExecutor(RetryPolicyOptions retryPolicyOptions)
    {
        _retryPolicyOptions = retryPolicyOptions;
    }

    public async Task ExecuteWithRetry<TException>(Func<Task> action, int? retryCount, TimeSpan? retryDelay,
      Action<Exception, TimeSpan, int, object> onRetry)
      where TException : Exception
    {
        var count = retryCount ?? _retryPolicyOptions.RetryCount;
        var delay = retryDelay ?? TimeSpan.FromSeconds(_retryPolicyOptions.SleepDurationProvider);

        var policy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(count,
                retryAttempt => delay,
                onRetry);

        var result = await policy.ExecuteAndCaptureAsync(action);

        if (result.Outcome == OutcomeType.Failure)
        {
            onRetry(result.FinalException, delay, count + 1, result.Context);
        }
    }

    public async Task<TResult> ExecuteWithRetry<TException, TResult>(Func<Task<TResult>> action, int? retryCount, TimeSpan? retryDelay,
      Action<Exception, TimeSpan, int, object> onRetry) where TException : Exception
    {
        var count = retryCount ?? _retryPolicyOptions.RetryCount;
        var delay = retryDelay ?? TimeSpan.FromSeconds(_retryPolicyOptions.SleepDurationProvider);

        var policy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(count,
                retryAttempt => delay,
                onRetry);

        var result = await policy.ExecuteAndCaptureAsync(action);

        if (result.Outcome == OutcomeType.Failure)
        {
            onRetry(result.FinalException, delay, count + 1, result.Context);
        }

        return result.Result;
    }


}
