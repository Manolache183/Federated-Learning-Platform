from quque_manager import QueueManager
import asyncio

async def main():
    print("Hello from Aggregator!")
    queuueManager = QueueManager()
    connect = queuueManager.connect()
    if (connect == False):
        print("Could not connect to RabbitMQ. Exiting...")
        return

    loop = asyncio.get_event_loop()
    loop.run_in_executor(None, queuueManager.consume)
    
    print("Starting to listen for work...")

if __name__ == "__main__":
    asyncio.run(main())
