namespace Azure.ServiceBus.Polly.Messaging
{
	public interface IBaseMessage
	{
		string Label { get; set; }
	}
}