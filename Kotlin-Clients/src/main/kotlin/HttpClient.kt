import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.lang.Thread.sleep

object HttpClient {
    private val client = OkHttpClient()

    private const val serverUrl = "http://host.docker.internal:8080"
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
        val jwtToken = getJwtToken()

        while (!checkIfTrainingShouldStart(jwtToken)) {
            println("Server not ready yet, waiting 10 seconds before retrying")
            sleep(10000)
        }

        if (!pullCurrentModel(jwtToken)) {
            println("Failed to pull initial model")
            return
        }

        work()

        if (!pushModel(jwtToken)) {
            println("Failed to push model")
            return
        }

        println("Waiting for the others to push their models - this will be changed in the future")
        sleep(10000)

        while(!pullCurrentModel(jwtToken)) {
            println("Failed to pull final model, retrying in 10 seconds")
            sleep(10000)
        }

        println("Training cycle completed")
    }

    private fun checkIfTrainingShouldStart(jwtToken: String): Boolean {
        val url = "$serverUrl/mnist/checkIfTrainingShouldStart"

        val request = Request.Builder()
            .url(url)
            .header("Authorization", "Bearer $jwtToken")
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            println("checkIfTrainingShouldStart -> Code: ${response.code} Message: ${response.message}")
            when (response.code) {
                200 -> return true
                else -> return false
            }
        }
    }

    private fun pullCurrentModel(jwtToken: String) : Boolean {
        val modelDownloadUrl = getModelDownloadUrl(jwtToken)

        if (modelDownloadUrl.isEmpty()) {
            return false
        } else {
            print("Downloading model from: $modelDownloadUrl\n")
            val request = Request.Builder()
                .url(modelDownloadUrl)
                .header("Authorization", "Bearer $jwtToken")
                .get()
                .build()

            client.newCall(request).execute().use { response ->
                println("pullModel() -> Code: " + response.code + " Message: " + response.message)
                when (response.code) {
                    200 -> return true
                    else -> return false
                }
            }
        }
    }

    private fun getModelDownloadUrl(jwtToken: String) : String {
        val url = "$serverUrl/mnist/getModelDownloadUrl"
        val request = Request.Builder()
            .url(url)
            .header("Authorization", "Bearer $jwtToken")
            .get()
            .build()

        client.newCall(request).execute().use { response ->
            println("getModelDownloadUrl() -> Code: " + response.code + " Message: " + response.message)
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

    private fun pushModel(jwtToken: String) : Boolean {
        val url = "$serverUrl/mnist/pushModel"

        // This should be content from an actual file
        val fileContent = "Random File Content"

        val requestBody = "{\"content\": \"$fileContent\"}".toRequestBody("application/json".toMediaType())
        val request = Request.Builder()
            .url(url)
            .header("Authorization", "Bearer $jwtToken")
            .post(requestBody)
            .build()

        client.newCall(request).execute().use { response ->
            println("pushModel() -> Code: " + response.code + " Message: " + response.message)
            when (response.code) {
                200 -> return true
                else -> return false
            }
        }
    }

    private fun work() {
        // This should be replaced with actual training
        println("Local training started, it will take 5 seconds")
        sleep(5000)
    }

    private fun getJwtToken() : String {
        val url = "$serverUrl/Authentication/generateJWT"
        val requestBody = "{\"email\": \"client@gmail.com\", \"password\": \"clientPass\", \"role\": \"client\"}"
            .toRequestBody("application/json".toMediaType())
        val request = Request.Builder()
            .url(url)
            .post(requestBody)
            .build()

        client.newCall(request).execute().use { response ->
            println("getJwtToken() -> Code: " + response.code + " Message: " + response.message)
            when (response.code) {
                200 -> {
                    val responseBody = response.body?.string()
                    println("JWT Token: $responseBody")
                    return responseBody ?: ""
                }
                else -> return ""
            }
        }
    }

}