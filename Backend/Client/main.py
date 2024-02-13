import routes
from requests import Session
from signalr import Connection

if __name__ == "__main__":
    print("Client started", flush=True)

    # connection = Connection("http://localhost:8080/clientHub", session=Session())
    # hub = connection.register_hub("clientHub")
    # connection.start()

    # def print_message(message):
    #     print("Message received: ", message)
        
    # hub.client.on("ReceiveNotification", print_message)
    print("AAAAAAAAAAAAAAAAA", flush=True)

    #hub.server.invoke("JoinGroup", "clients")

    print("BBBBBBBBBBBBBBBBB", flush=True)
    app = routes.getApp()
    app.run(host="0.0.0.0", port=5000, debug=True)
