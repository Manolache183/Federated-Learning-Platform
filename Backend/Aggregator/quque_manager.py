import pika
from pika import credentials

class QueueManager:
    def __init__(self):
        self.connection = None
        self.channel = None

    def connect(self):
        for i in range(3):
            try:
                print("Connecting to RabbitMQ")
                credentials = pika.PlainCredentials('guest', 'guest')
                self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq', port=5672, credentials=credentials))
                self.channel = self.connection.channel()
                self.channel.queue_declare(queue='working_queue', durable=True)
                self.channel.queue_declare(queue='results_queue', durable=True)
                return True
            except Exception as e:
                print("An error occurred: ", str(e), "\nRetrying in 5 seconds")
                time.sleep(5);
        
        return False

    def consume(self):
        def callback(ch, method, properties, body):
            print(f" [x] Received {body}")
            self.simulateWork()
            print(f" [x] Done")
            ch.basic_ack(delivery_tag=method.delivery_tag)

        self.channel.basic_consume(queue='working_queue', on_message_callback=callback)
        self.channel.start_consuming()

    def close(self):
        self.connection.close()

    def publish(self, message):
        self.channel.basic_publish(exchange='', routing_key='results_queue', body=message)
        
    def simulateWork(self):
        print("Simulating work...")
        time.sleep(5)
        print("Work done!")
        self.publish("Work done!")
        