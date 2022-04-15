
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri(""); //write AMQP URL

using (var connection = factory.CreateConnection())
{
    var channel = connection.CreateModel();

    channel.ExchangeDeclare("exchange-header-logs", durable: true, type: ExchangeType.Headers);

    channel.BasicQos(0, 1, false);

    var subscriber = new EventingBasicConsumer(channel);

    string queueName = channel.QueueDeclare().QueueName;

    Dictionary<string, object> header = new Dictionary<string, object>();

    header.Add("format", "pdf");
    header.Add("shape", "a4");
    header.Add("x-match","all");

    channel.QueueBind(queueName, "exchange-header-logs",string.Empty,header);

    channel.BasicConsume(queueName, false, subscriber);

    Console.WriteLine("Listening...");

    subscriber.Received += (object? sender, BasicDeliverEventArgs e) =>
    {
        var message = Encoding.UTF8.GetString(e.Body.ToArray());

        Thread.Sleep(1000);

        Console.WriteLine($"Received Message : {message}");

        //File.AppendAllText("logs"+ queueName + ".txt", message + "\n");

        channel.BasicAck(e.DeliveryTag, false);
    };

    Console.ReadLine();
}