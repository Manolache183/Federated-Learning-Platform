using Logger.Firebase;
using Microsoft.AspNetCore.Mvc;

namespace Logger.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileMetadataController : ControllerBase
    {
        private readonly FirestoreDatabaseService _cloudFirestoreService;

        public FileMetadataController(FirestoreDatabaseService cloudFirestoreService)
        {
            _cloudFirestoreService = cloudFirestoreService;
        }

        [HttpPost("addFileMetadata")]
        public async Task<IActionResult> AddFileMetadata([FromBody] FileMetadataReceive payload)
        {
            Console.WriteLine("Received file metadata request for file: {0}", payload.fileName);
            var r = await _cloudFirestoreService.AddFileMetadata(new FileMetadata(payload.fileName, payload.firebaseStorageID, DateTime.Now));
            if (r)
            {
                return Ok("Logged file metadata");
            }

            return StatusCode(500, "Failed to log file metadata");
        }

        [HttpGet("getFileMetadata/{filename}")]
        public async Task<IActionResult> GetFileMetadata(string filename)
        {
            Console.WriteLine("Received request to get file metadata");
            var r = await _cloudFirestoreService.GetFileMetadata(filename);
            if (r != null)
            {
                return Ok(r);
            }

            return StatusCode(500, "Failed to get file metadata");
        }

        [HttpPut("updateFileMetadata")]
        public async Task<IActionResult> UpdateFileMetadata([FromBody] FileMetadataReceive payload)
        {
            Console.WriteLine("Received request to update file metadata for file: {0}", payload.fileName);
            var r = await _cloudFirestoreService.UpdateFileMetadata(new FileMetadata(payload.fileName, payload.firebaseStorageID, DateTime.Now));
            if (r)
            {
                return Ok("Updated file metadata");
            }

            return StatusCode(500, "Failed to update file metadata");
        }

        [HttpDelete("deleteFileMetadata/{filename}")]
        public async Task<IActionResult> DeleteFileMetadata(string filename)
        {
            Console.WriteLine("Received request to delete file metadata for file: {0}", filename);
            var r = await _cloudFirestoreService.DeleteFileMetadata(filename);
            if (r)
            {
                return Ok("Deleted file metadata");
            }

            return StatusCode(500, "Failed to delete file metadata");
        }

        [HttpPut("swapModelFiles/{latestModelFirebaseStorageID}")]
        public async Task<IActionResult> SwapModelFiles(string latestModelFirebaseStorageID)
        {
            Console.WriteLine("Received request to swap model files");
            var r = await _cloudFirestoreService.SwapModelFiles(latestModelFirebaseStorageID);
            if (r)
            {
                return Ok("Swapped model files");
            }

            return StatusCode(500, "Failed to swap model files");
        }
    }
}
