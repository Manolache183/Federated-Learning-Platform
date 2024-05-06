import pika
import time
from pika import credentials
from FirebaseStorageService import FirebaseStorageService

class QueueManager:
    def __init__(self):
        self.connection = None
        self.channel = None
        self.firebaseStorageService = FirebaseStorageService()

    def connect(self):
        for i in range(3):
            try:
                print("Connecting to RabbitMQ")
                credentials = pika.PlainCredentials('guest', 'guest')
                self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq', port=5672, credentials=credentials))
                self.channel = self.connection.channel()
                self.channel.queue_declare(queue='work_queue', durable=True)
                self.channel.queue_declare(queue='results_queue', durable=True)
                return True
            except Exception as e:
                print("An error occurred: ", str(e), "\nRetrying in 5 seconds")
                time.sleep(5);
        
        return False

    def consume(self):
        def callback(ch, method, properties, body):
            message = body.decode('utf-8')
            print(f" [x] Received {message}")
            self.simulateWork(message)
            print(f" [x] Done")
            ch.basic_ack(delivery_tag=method.delivery_tag)
            self.publish("Work done!")

        self.channel.basic_consume(queue='work_queue', on_message_callback=callback)
        self.channel.start_consuming()

    def close(self):
        self.connection.close()

    def publish(self, message):
        self.channel.basic_publish(exchange='', routing_key='results_queue', body=message)
        
    def simulateWork(self, modelName):
        print("Simulating work...")
        self.firebaseStorageService.downloadClientModels()
        time.sleep(5)
        print("Work done!")
        self.firebaseStorageService.uploadModel(modelName)
        