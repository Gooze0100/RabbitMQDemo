using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

ConnectionFactory factory = new();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Receiver1 App";

IConnection cnn = factory.CreateConnection();

IModel channel = cnn.CreateModel();

string exchangeName = "DemoExchange";
string rountingKey = "demo-routing-key";
string queueName = "DemoQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName, false, false, false, null);
channel.QueueBind(queueName, exchangeName, rountingKey, null);
// prefetch size is saying we dont care how big the message is 
// prefetch count means how many messages you want to get at once, handle just one message at the time
// global just means if we want to apply this to all the system of just this one instance
channel.BasicQos(0, 1, false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    // simulate to take some time to do the work
    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
    var body = args.Body.ToArray();

    // do something with message save to db or something, thats how you handle it
    string message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Message Received: {message}");

    // do not acknoledge the message if you want ho handle when DB is down, just make it a failure and then rabbitMQ does not get message from queue
    // and leave it there 
    channel.BasicAck(args.DeliveryTag, false);
};

string consumerTag = channel.BasicConsume(queueName, false, consumer);

Console.ReadLine();

// we do it properly grab a tag from it and then delete content class consumer
channel.BasicCancel(consumerTag);

channel.Close();
cnn.Close();

/*
the biggest beuty of this is that the server do not need to be running, we can send messages into the queue all the time
In the real world re can be running second instance of receiver and handle messages, but with this project we will create a new console app to handle it
Mormally we would do we would get executable from our receiver put it on multiple servers or have it run multiple instances on one server
most likely multiple servers because it waits for specific resources it takes cpu ram or network otherwise it would be instantanious



*/
