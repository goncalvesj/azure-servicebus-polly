namespace Azure.ServiceBus.Polly.Messaging
{
	public class MessagingOptions
	{
		public string TopicName { get; set; }

		public string ErrorQueueName { get; set; } = Globals.ErrorQueueName;

		public int RetryCount { get; set; } = Globals.RetryCount;

		public int RetrySeconds { get; set; } = Globals.RetrySeconds;

		public string ServiceBusConnectionString { get; set; }

		public string ServiceBusConnectionStringSecondary { get; set; } = string.Empty;
	}
}
