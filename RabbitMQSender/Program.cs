using RabbitMQ.Client;
using System.Text;

ConnectionFactory factory = new();
// this is connection string to rabbitmq server to send messages
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
// important to give a name for debugging
factory.ClientProvidedName = "Rabbit Sender App";

IConnection cnn = factory.CreateConnection();

// create a channel for rabbitmq
IModel channel = cnn.CreateModel();

string exchangeName = "DemoExchange";
string rountingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, false, false, false, null);
channel.QueueBind(queueName, exchangeName, rountingKey, null);

for (int i = 0; i < 60; i++)
{
    Console.WriteLine($"Sending message {i}");
    byte[] messageBodyBytes = Encoding.UTF8.GetBytes($"Message #{i}");
    channel.BasicPublish(exchangeName, rountingKey, null, messageBodyBytes);
    Thread.Sleep(1000);
}

//if we dont close the channel connection will close it anyway, but best practice is to close channels
channel.Close();
cnn.Close();
