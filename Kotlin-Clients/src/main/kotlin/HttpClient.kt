import okhttp3.MediaType
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
import okhttp3.RequestBody.Companion.toRequestBody

object HttpClient {
    private val client = OkHttpClient()
    private var trainingStarted = false

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
        while(true) {
            if (trainingStarted) {
                if (pullCurrentModel()) {
                    println("Final model Pulled")
                    trainingStarted = false
                    break
                } else {
                    println("Aggregation not ready yet!")
                }
            }
            else if (checkIfTrainingShouldStart())
            {
                trainingStarted = true
                if (pullCurrentModel()) {
                    work()
                    println("Training Finished")

                    if (pushModel()) {
                        println("Model Uploaded")
                    } else {
                        println("Model Upload Failed")
                    }
                } else {
                    println("Pulling initial model Failed")
                }
            }

            print("Checking again in 10 seconds")
            Thread.sleep(10000)
        }

        print("Training Cycle Finished")
    }

    private fun checkIfTrainingShouldStart() : Boolean {
        val url = "$serverUrl/mnist/checkIfTrainingShouldStart"
        val request = Request.Builder()
            .url(url)
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            println("Code: " + response.code + "\nMessage: " + response.message)
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
        val url = "$serverUrl/mnist/pullModel"
        val request = Request.Builder()
            .url(url)
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            println("Code: " + response.code + "\nMessage: " + response.message)
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

    private fun pushModel() : Boolean {
        val url = "$serverUrl/mnist/pushModel"
        val requestBody = "{\"model\": \"Here is a model!\"}".toRequestBody("application/json".toMediaType())

        val request = Request.Builder()
            .url(url)
            .post(requestBody)
            .build()

        client.newCall(request).execute().use { response ->
            println("Code: " + response.code + "\nMessage: " + response.message)
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
        Thread.sleep(10000)
    }
}