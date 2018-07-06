using System.Threading.Tasks;

namespace Azure.ServiceBus.Polly.Messaging
{
	public interface IMessaging
	{
		/// <summary>
		///     Sends a message of type IMessaging to a Service Bus Topic or Queue
		///		<para />
		///		Message is Encoded in UTF8 and Serialized to Json before being sent
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="messageToSend"></param>
		/// <returns></returns>
		Task SendMessageAsync<T>(T messageToSend) where T : IBaseMessage;
	}
}