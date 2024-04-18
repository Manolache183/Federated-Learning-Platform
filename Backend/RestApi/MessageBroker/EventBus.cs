using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RestApi.MessageBroker
{
    public class EventBus
    {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private IBasicProperties _properties;
        private const string _hostName = "rabbitmq";
        private const string _userName = "guest";
        private const string _password = "guest";

        private enum Queues
        {
            work_queue,
            results_queue
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EventBus()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            var connected = connectToRabbitMQ();
            if (!connected)
            {
                Environment.Exit(1);
            }

            createQueues();
            Task.Run(() => listenForResults());
        }

        private bool connectToRabbitMQ()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _factory = new ConnectionFactory()
                    {
                        HostName = _hostName,
                        Port = 5672,
                        UserName = _userName,
                        Password = _password
                    };
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _properties = _channel.CreateBasicProperties();
                    
                    return true;
                } catch
                {
                    Console.WriteLine("Failed to connect to RabbitMQ. Retrying in 5 seconds.");
                    Thread.Sleep(5000);
                }
            }

            Console.WriteLine("Failed to connect to RabbitMQ. Exiting.");
            return false;
        }

        private void createQueues()
        {
            var queues = Enum.GetValues(typeof(Queues));
            foreach (var queue in queues)
            {
                Console.WriteLine($"Creating queue: {queue}");
                _channel.QueueDeclare(queue: queue.ToString(), durable: true, exclusive: false, autoDelete: false, arguments: null);
            }
        }

        private void listenForResults()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");
            };
            _channel.BasicConsume(queue: Queues.results_queue.ToString(), autoAck: true, consumer: consumer);
        }

        public void PublishAgregateMessage()
        {
            var message = "Message sent from rest";
            var body = Encoding.UTF8.GetBytes(message);
            
            _channel.BasicPublish(exchange: string.Empty,
                     routingKey: Queues.work_queue.ToString(),
                     basicProperties: _properties,
                     body: body);

            Console.WriteLine($" [x] Sent {message}");
        }
    }
}
