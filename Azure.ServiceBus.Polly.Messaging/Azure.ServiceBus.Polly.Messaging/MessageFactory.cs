using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Azure.ServiceBus.Polly.Messaging
{
	public class MessageFactory
	{
		internal static ISenderClient PrimarySenderClient { get; set; }
		internal static ISenderClient SecondarySenderClient { get; set; }
		internal static ISenderClient ErrorSenderClient { get; set; }

		internal static MessageSender SetUpFactory(string topicName, string connectionString,
			RetryPolicy retryPolicy = null)
		{
			return new MessageSender(connectionString, topicName, retryPolicy ?? RetryPolicy.Default);
		}
	}
}