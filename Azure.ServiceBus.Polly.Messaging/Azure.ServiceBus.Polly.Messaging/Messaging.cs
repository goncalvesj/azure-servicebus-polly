using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azure.ServiceBus.Polly.Messaging
{
	public class Messaging : IMessaging
	{
		///  <summary>
		///      Sets Up primary Service Bus connection.
		///      Sets Up Secondary Service Bus connection if secondary connection string exists.
		///      Sets Up Retry and Fallback policies if secondary connection string exists.
		/// 		Logs Exception in case of failure in secondary namespace
		///  </summary>		
		/// <param name="options"></param>
		public Messaging(IOptions<MessagingOptions> options)
		{
			var _options = options.Value;

			MessageFactory.PrimarySenderClient =
				MessageFactory.SetUpFactory(_options.TopicName, _options.ServiceBusConnectionString, RetryPolicy.Default);
			
			if (string.IsNullOrEmpty(_options.ServiceBusConnectionStringSecondary)) return;

			MessageFactory.SecondarySenderClient =
				MessageFactory.SetUpFactory(_options.TopicName, _options.ServiceBusConnectionStringSecondary, RetryPolicy.Default);
						
			MessageFactory.ErrorSenderClient =
				MessageFactory.SetUpFactory(_options.ErrorQueueName, _options.ServiceBusConnectionStringSecondary);

			PollyFactory.SetUpPolicies(_options.RetryCount, _options.RetrySeconds, OnFallBackAction, OnErrorAction);
		}

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
		/// <param name="eventTopic">Topic to publish events to</param>
		//public ProceduralSystemMessaging(
		//	string topicName,
		//	string connectionStringPrimary, string connectionStringSecondary = "",
		//	int retryCount = Globals.RetryCount, int retrySeconds = Globals.RetrySeconds,
		//	string errorQueueName = Globals.ErrorQueueName, string eventTopic = Globals.EventTopicName)
		//{
		//	MessageFactory.PrimarySenderClient =
		//		MessageFactory.SetUpFactory(topicName, connectionStringPrimary, RetryPolicy.Default);

		//	MessageFactory.PrimaryEventSenderClient =
		//		MessageFactory.SetUpFactory(eventTopic, connectionStringPrimary, RetryPolicy.Default);

		//	if (string.IsNullOrEmpty(connectionStringSecondary)) return;

		//	MessageFactory.SecondarySenderClient =
		//		MessageFactory.SetUpFactory(topicName, connectionStringSecondary, RetryPolicy.Default);

		//	MessageFactory.SecondaryEventSenderClient =
		//		MessageFactory.SetUpFactory(eventTopic, connectionStringSecondary, RetryPolicy.Default);

		//	MessageFactory.ErrorSenderClient =
		//		MessageFactory.SetUpFactory(errorQueueName, connectionStringSecondary);

		//	PollyFactory.SetUpPolicies(retryCount, retrySeconds, OnFallBackAction, OnErrorAction);
		//}

		internal static Func<IBaseMessage, Task> OnFallBackAction { get; } =
			async message => await SendMessage(message, MessageFactory.SecondarySenderClient).ConfigureAwait(false);

		internal static Func<IBaseMessage, Task> OnErrorAction { get; } =
			async message => await SendMessage(message, MessageFactory.ErrorSenderClient).ConfigureAwait(false);

		public async Task SendMessageAsync<T>(T messageToSend) where T : IBaseMessage
		{
			if (PollyFactory.MessagingPolicy != null)
			{
				await PollyFactory.MessagingPolicy
								  .ExecuteAsync(
												context =>
													SendMessage(messageToSend, MessageFactory.PrimarySenderClient),
												new Dictionary<string, object> { { "messageToSend", messageToSend } }).ConfigureAwait(false);
			}
			else
				await SendMessage(messageToSend, MessageFactory.PrimarySenderClient).ConfigureAwait(false);
		}

		private static async Task SendMessage<T>(T messageToSend, ISenderClient sender) where T : IBaseMessage
		{
			var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageToSend)))
			{
				Label = messageToSend.Label
			};

			await sender.SendAsync(message).ConfigureAwait(false);
		}
	}
}