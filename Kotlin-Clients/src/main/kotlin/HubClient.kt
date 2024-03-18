import com.microsoft.signalr.HubConnectionBuilder

object HubClient {
    private const val hubUrl = "http://localhost:8080/clientHub"
    private val hubConnection = HubConnectionBuilder.create(hubUrl).build()

    private const val receiveNotificationTag = "ReceiveNotification" // if more stack up, replace them with enum class

    init { // maybe remove init
        hubConnection.start().blockingAwait()

        hubConnection.on(
            receiveNotificationTag,
            ::receiveNotification,
            String::class.java
        )
    }

    private fun receiveNotification(message : String) {
        println("New Message: $message")
    }
}