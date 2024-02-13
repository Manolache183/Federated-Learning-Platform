import okhttp3.OkHttpClient
import okhttp3.Request

object HttpClient {
    private val client = OkHttpClient()

    fun pingServer() {
        val url = "http://localhost:8080/mnist/test_kotlin"
        val request = Request.Builder().url(url).build()

        client.newCall(request).execute().use { response ->
            if (!response.isSuccessful) throw RuntimeException("Unexpected code $response")

            println(response.code)
        }
    }
}