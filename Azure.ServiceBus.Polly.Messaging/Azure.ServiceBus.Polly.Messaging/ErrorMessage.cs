namespace Azure.ServiceBus.Polly.Messaging
{
	internal class ErrorMessage : IBaseMessage
	{
		public string Label { get; set; }
		public string Exception { get; set; }
		public IBaseMessage Message { get; set; }
	}
}
