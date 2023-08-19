using Gw2Sharp.WebApi.Exceptions;
using System;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public class TaskUtil
    {
        /// <summary>
        /// Retries the given awaitable <see cref="Task{T}"/> function a given amount of times each after a set delay (default: 30s).
        /// </summary>
        /// <typeparam name="T">Some type returned by the <see cref="Task"/> function.</typeparam>
        /// <param name="func">The awaitable function to retry.</param>
        /// <param name="retries">Amount of retries before an exception is logged.</param>
        /// <param name="delayMs">A delay in milliseconds to wait before trying again.</param>
        /// <param name="logger">An optional logger that is used for exception messages.</param>
        /// <returns><see cref="Task{T}"/> if successful; otherwise <see cref="Task"/>&lt;<see langword="default"/>&gt;.</returns>
        public static async Task<T> RetryAsync<T>(Func<Task<T>> func, int retries = 2, int delayMs = 30000, Logger logger = null)
        {
            logger ??= Logger.GetLogger<TaskUtil>();

            try
            {
                return await func();
            }
            catch (Exception e)
            {
                // Do not retry if requested resource does not exist or access is denied.
                if (e is NotFoundException or BadRequestException or AuthorizationRequiredException)
                {
                    logger.Trace(e, e.Message);
                    return default;
                }

                if (retries > 0)
                {
                    logger.Warn(e, $"Failed to request data. Retrying in {delayMs / 1000} second(s) (remaining retries: {retries}).");
                    await Task.Delay(delayMs);
                    return await RetryAsync(func, retries - 1, delayMs, logger);
                }

                switch (e)
                {
                    case TooManyRequestsException:
                        logger.Warn(e, "After multiple attempts no data could be loaded due to being rate limited by the API.");
                        break;
                    case RequestException or RequestException<string>:
                        logger.Trace(e, e.Message);
                        break;
                    default:
                        logger.Error(e, e.Message);
                        break;
                }

                return default;
            }
        }

        /// <summary>s
        /// Tries the given awaitable <see cref="Task{T}"/> function, catching exceptions.
        /// </summary>
        /// <typeparam name="T">Some type returned by the <see cref="Task"/> function.</typeparam>
        /// <param name="func">The awaitable function to try.</param>
        /// <param name="logger">An optional logger that is used for exception messages.</param>
        /// <returns><see cref="Task{T}"/> if successful; otherwise <see cref="Task"/>&lt;<see langword="default"/>&gt;.</returns>
        public static async Task<T> TryAsync<T>(Func<Task<T>> func, Logger logger = null)
        {
            return await RetryAsync(func, 0, 0, logger);
        }
    }
}
