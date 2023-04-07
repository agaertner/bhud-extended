using Flurl.Http;
using Gw2Sharp.WebApi.Exceptions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.Extended
{
    public class TaskUtil
    {
        private static Logger Logger = Logger.GetLogger<TaskUtil>();
        public static void CallActionWithTimeout(Action action, Action error, int timeout)
        {
            var cancelToken = new CancellationTokenSource();
            var token = cancelToken.Token;
            var task = Task.Run(delegate
            {
                try
                {
                    var thread = Thread.CurrentThread;
                    using (token.Register(thread.Abort))
                    {
                        action();
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }, token);
            var index = Task.WaitAny(task, Task.Delay(timeout));
            if (index != 0)
            {
                cancelToken.Cancel();
                error();
            }
            else if (task.Result != null)
            {
                Logger.Error(task.Result.Message, task.Result);
            }
        }

        public static bool TryParseJson<T>(string json, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (_, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(json, settings);
            return success;
        }

        public static async Task<(bool, T)> GetJsonResponse<T>(string request, int timeOutSeconds = 10)
        {
            try
            {
                var rawJson = await request.AllowHttpStatus(HttpStatusCode.NotFound).AllowHttpStatus("200").WithTimeout(timeOutSeconds).GetStringAsync();
                return (TryParseJson<T>(rawJson, out var result), result);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                Logger.Warn(ex, $"Request '{request}' timed out.");
            }
            catch (FlurlHttpException ex)
            {
                Logger.Warn(ex, $"Request '{request}' was not successful.");
            }
            catch (JsonReaderException ex)
            {
                Logger.Warn(ex, $"Failed to deserialize requested content from \"{request}\"\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unexpected error while requesting '{request}'.");
            }

            return (false, default);
        }

        /// <summary>
        /// Retries the given awaitable <see cref="Task{T}"/> function a given amount of times each after a set delay (default: 30s).
        /// </summary>
        /// <typeparam name="T">Some type returned by the <see cref="Task"/> function.</typeparam>
        /// <param name="func">The awaitable function to retry.</param>
        /// <param name="retries">Amount of retries before an exception is logged.</param>
        /// <param name="delayMs">A delay in milliseconds to wait before trying again.</param>
        /// <returns><see cref="Task{T}"/> if successful; otherwise <see cref="Task"/>&lt;<see langword="default"/>&gt;.</returns>
        public static async Task<T> RetryAsync<T>(Func<Task<T>> func, int retries = 2, int delayMs = 30000)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                // Do not retry if requested resource does not exist or access is denied.
                if (e is NotFoundException or BadRequestException or AuthorizationRequiredException)
                {
                    Logger.Trace(e, e.Message);
                    return default;
                }

                if (retries > 0)
                {
                    Logger.Warn(e, $"Failed to pull data from the GW2 API. Retrying in {delayMs / 1000} second(s) (remaining retries: {retries}).");
                    await Task.Delay(delayMs);
                    return await RetryAsync(func, retries - 1, delayMs);
                }

                switch (e)
                {
                    case TooManyRequestsException:
                        Logger.Warn(e, "After multiple attempts no data could be loaded due to being rate limited by the API.");
                        break;
                    case RequestException or RequestException<string>:
                        Logger.Trace(e, e.Message);
                        break;
                    default:
                        Logger.Error(e, e.Message);
                        break;
                }

                return default;
            }
        }

        /// <summary>
        /// Tries the given awaitable <see cref="Task{T}"/> function, catching exceptions.
        /// </summary>
        /// <typeparam name="T">Some type returned by the <see cref="Task"/> function.</typeparam>
        /// <param name="func">The awaitable function to try.</param>
        /// <returns><see cref="Task{T}"/> if successful; otherwise <see cref="Task"/>&lt;<see langword="default"/>&gt;.</returns>
        public static async Task<T> TryAsync<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case NotFoundException or BadRequestException or AuthorizationRequiredException: // Resource does not exist or access is denied.
                        Logger.Trace(e, e.Message);
                        break;
                    case TooManyRequestsException:
                        Logger.Warn(e, "No data could be loaded due to being rate limited by the API.");
                        break;
                    case RequestException or RequestException<string>:
                        Logger.Trace(e, e.Message);
                        break;
                    default:
                        Logger.Error(e, e.Message);
                        break;
                }
                return default;
            }
        }
    }
}
