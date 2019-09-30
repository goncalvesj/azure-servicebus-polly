﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;

namespace Azure.ServiceBus.Polly.Messaging
{
	public static class PollyFactory
	{
		internal static AsyncPolicyWrap MessagingPolicy { get; set; }

		private static IAsyncPolicy SetUpRetryPolicy(int retryCount, int retrySeconds)
		{
			return Policy.Handle<Exception>()
				.WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(retrySeconds),
					(exception, timeSpan, i, context) => Console.WriteLine($"Retrying due to: {exception.Message}; Try number: {i}"));
		}

		private static IAsyncPolicy SetUpFallBackPolicy(Func<IBaseMessage, Task> onFallBackAction, Func<IBaseMessage, Task> onErrorAction)
		{
			return Policy.Handle<Exception>()
				.FallbackAsync(
					async (exception, context, token) => await onFallBackAction(context.Values.FirstOrDefault() as IBaseMessage).ConfigureAwait(false),
					async (exception, context) =>
					{
						await onErrorAction(new ErrorMessage
						{
							Label = "SendMessageError",
							Exception = exception.Message,
							Message = context.Values.FirstOrDefault() as IBaseMessage
						}).ConfigureAwait(false);
					});
		}

		internal static void SetUpPolicies(int retryCount, int retrySeconds,
			Func<IBaseMessage, Task> onFallBackAction, Func<IBaseMessage, Task> onErrorAction)
		{
			var fallBackPolicy =
				SetUpFallBackPolicy(onFallBackAction, onErrorAction);

			var retryPolicy = SetUpRetryPolicy(retryCount, retrySeconds);

			MessagingPolicy = fallBackPolicy.WrapAsync(retryPolicy);
		}
	}
}