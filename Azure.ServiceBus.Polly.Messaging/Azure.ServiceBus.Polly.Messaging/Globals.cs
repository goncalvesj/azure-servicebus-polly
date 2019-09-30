namespace Azure.ServiceBus.Polly.Messaging
{
	public static class Globals
	{
		internal const int RetryCount = 3;
		internal const int RetrySeconds = 3;
		internal const string ErrorQueueName = "Error";
		internal const string EventTopicName = "events";
	}
}
