fun main() {
    HttpClient.pingServer()
    HubClient.toString() // junk: to run the init block it has to be referenced once
}

