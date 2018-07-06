using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace Azure.ServiceBus.Polly.Messaging
{
	public class Messaging : IMessaging
	{
		internal const int RetryCount = 3;
		internal const int RetrySeconds = 3;
		internal const string ErrorQueueName = "Error";

		///  <summary>
		///      Sets Up primary Service Bus connection.
		///      Sets Up Secondary Service Bus connection if secondary connection string exists.
		///      Sets Up Retry and Fallback policies if secondary connection string exists.
		/// 		Logs Exception in case of failure in secondary namespace
		///  </summary>
		///  <param name="topicName">Service Bus topic name</param>
		///  <param name="connectionStringPrimary">Service Bus primary connection string</param>
		///  <param name="connectionStringSecondary">Service Bus secondary connection string</param>
		///  <param name="retryCount">Number of retries applied to Retry Policy</param>
		///  <param name="retrySeconds">Number of seconds between every retry</param>
		///  <param name="errorQueueName">Name of Queue to log expections</param>
		public Messaging(
			string topicName,
			string connectionStringPrimary, string connectionStringSecondary = "",
			int retryCount = RetryCount, int retrySeconds = RetrySeconds, string errorQueueName = ErrorQueueName)
		{
			MessageFactory.PrimarySenderClient =
				MessageFactory.SetUpFactory(topicName, connectionStringPrimary, RetryPolicy.Default);

			if (string.IsNullOrEmpty(connectionStringSecondary)) return;

			MessageFactory.SecondarySenderClient =
				MessageFactory.SetUpFactory(topicName, connectionStringSecondary, RetryPolicy.Default);

			MessageFactory.ErrorSenderClient =
				MessageFactory.SetUpFactory(errorQueueName, connectionStringSecondary);

			PollyFactory.SetUpPolicies(retryCount, retrySeconds, OnFallBackAction, OnErrorAction);
		}

		internal static Func<IBaseMessage, Task> OnFallBackAction { get; } =
			async message => await SendMessage(message, MessageFactory.SecondarySenderClient);

		internal static Func<IBaseMessage, Task> OnErrorAction { get; } =
			async message => await SendMessage(message, MessageFactory.ErrorSenderClient);

		public async Task SendMessageAsync<T>(T messageToSend) where T : IBaseMessage
		{
			if (PollyFactory.ProceduralSystemMessagingPolicy != null)
				await PollyFactory.ProceduralSystemMessagingPolicy
					.ExecuteAsync(
						context => SendMessage(messageToSend, MessageFactory.PrimarySenderClient),
						new Dictionary<string, object> {{ "messageToSend", messageToSend } });
			else
				await SendMessage(messageToSend, MessageFactory.PrimarySenderClient);
		}

		private static async Task SendMessage<T>(T messageToSend, ISenderClient sender) where T : IBaseMessage
		{
			var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageToSend)))
			{
				Label = messageToSend.Label
			};

			await sender.SendAsync(message);
		}
	}
}