import okhttp3.MediaType
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
import okhttp3.RequestBody.Companion.toRequestBody
import java.lang.Thread.sleep

object HttpClient {
    private val client = OkHttpClient()

    private const val serverUrl = "http://restapi:8080"
    fun pingServer() {
        val url = "$serverUrl/general/ping"
        val request = Request.Builder()
            .url(url)
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            if (!response.isSuccessful) throw RuntimeException("Unexpected code $response")

            println(response.code)
        }
    }

    // All of this must be moved to a thread class
    fun trainingCycle() {
        while (!checkIfTrainingShouldStart()) {
            println("Server not ready yet, waiting 10 seconds before retrying")
            sleep(10000)
        }

        if (!pullCurrentModel()) {
            println("Failed to pull initial model")
            return
        }

        work()

        if (!pushModel()) {
            println("Failed to push model")
            return
        }

        sleep(1000)

        while(!pullCurrentModel()) {
            println("Failed to pull final model, retrying in 10 seconds")
            sleep(10000)
        }

        println("Training cycle completed")
    }

    private fun checkIfTrainingShouldStart() : Boolean {
        val url = "$serverUrl/mnist/checkIfTrainingShouldStart"
        val request = Request.Builder()
            .url(url)
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            when (response.code) {
                200 -> {
                    return true
                }
                else -> {
                    return false
                }
            }
        }
    }

    private fun pullCurrentModel() : Boolean {
        val modelDownloadUrl = getModelDownloadUrl()
        if (modelDownloadUrl.isEmpty()) {
            return false
        } else {
            print("Downloading model from: $modelDownloadUrl\n")
            val request = Request.Builder()
                .url(modelDownloadUrl)
                .get()
                .build()

            client.newCall(request).execute().use { response ->
                when (response.code) {
                    200 -> {
                        println("Model Pulled")
                    }
                    else -> {
                        println("Failed to pull model")
                        return false
                    }
                }
            }
        }

        return true
    }

    private fun getModelDownloadUrl() : String {
        val url = "$serverUrl/mnist/getModelDownloadUrl"
        val request = Request.Builder()
            .url(url)
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            when (response.code) {
                200 -> {
                    val responseBody = response.body?.string()
                    println("Model download url: $responseBody")
                    return responseBody ?: ""
                }
                else -> {
                    println("Failed to get model download url: " + response.code + " " + response.message)
                    return ""
                }
            }
        }
    }

    private fun pushModel() : Boolean {
        val url = "$serverUrl/mnist/pushModel"

        // This should be content from an actual file
        val fileContent = "Random File Content"

        val requestBody = "{\"content\": \"$fileContent\"}".toRequestBody("application/json".toMediaType())
        val request = Request.Builder()
            .url(url)
            .post(requestBody)
            .build()

        client.newCall(request).execute().use { response ->
            println("Code: " + response.code + " Message: " + response.message)
            when (response.code) {
                200 -> {
                    return true
                }
                else -> {
                    return false
                }
            }
        }
    }

    private fun work() {
        // This should be replaced with actual training
        println("Local training started, it will take 5 seconds")
        sleep(5000)
    }
}