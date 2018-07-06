# azure-servicebus-polly
.NET Standard dll that send messages to a service bus namespace with Polly policies for retry and fallback

Sets Up primary Service Bus connection.
Sets Up Secondary Service Bus connection if secondary connection string exists.
Sets Up Retry and Fallback policies if secondary connection string exists.
Logs Exception in case of failure in secondary namespace
