using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestApi.Common;
using RestApi.HttpClients;
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

        private readonly IClientPlatformService _clientPlatformService;

        private const string clientAddr = "http://host.docker.internal:4000";
        public bool aggregationInProgress = false;

        private TrainingInfo _trainingInfo;

        private enum QueueName
        {
            work_queue,
            results_queue
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EventBus(IClientPlatformService clientPlatformService)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _clientPlatformService = clientPlatformService;

            var connected = connectToRabbitMQ();
            if (!connected)
            {
                Environment.Exit(1);
            }

            createQueues();
            Task.Run(listenForResults);
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
                    
                    Console.WriteLine("Connected to RabbitMQ.");
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

        public void PublishAgregateMessage(string message)
        {
            var timeStamp = DateTime.UtcNow.ToString("o");
            _trainingInfo = new TrainingInfo();
            _trainingInfo.startedAt = timeStamp;

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: string.Empty,
                     routingKey: QueueName.work_queue.ToString(),
                     basicProperties: _properties,
                     body: body);

            Console.WriteLine($" [x] Sent {message}");
        }

        private void createQueues()
        {
            var queues = Enum.GetValues(typeof(QueueName));
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
                
                Console.WriteLine($" [x] Received {message}"); // message = clientID-accuracy

                var clientID = message.Split(';')[0];
                var accuracy = message.Split(';')[1];

                _trainingInfo.finishedAt = DateTime.UtcNow.ToString("o");
                _trainingInfo.accuracy = double.Parse(accuracy);

                Console.WriteLine("Salut client: " + clientID);

                _clientPlatformService.NotifyClient(clientID, _trainingInfo).Wait();
                
                aggregationInProgress = false;
            };
            _channel.BasicConsume(queue: QueueName.results_queue.ToString(), autoAck: true, consumer: consumer);
        }
    }
}
