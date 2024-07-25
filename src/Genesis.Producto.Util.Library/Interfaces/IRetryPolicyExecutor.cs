using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis.Producto.Util.Library.Interfaces;
public interface IRetryPolicyExecutor
{
    Task ExecuteWithRetry<TException>(Func<Task> action, int? retryCount, TimeSpan? retryDelay, Action<Exception, TimeSpan, int, Object> onRetry)
      where TException : Exception;

    Task<TResult> ExecuteWithRetry<TException, TResult>(Func<Task<TResult>> action, int? retryCount, TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, object> onRetry) where TException : Exception;
}
