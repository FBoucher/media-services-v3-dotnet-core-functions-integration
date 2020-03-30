//
// Azure Media Services REST API v3 - Functions
//
// StartBlobContainerCopyToAsset - This function starts copying blob container to the asset.
//
/*
```c#
Input:
    {
        // [Required] The name of the file at destination
        "destFilename": "test-A.mp4",

        // [Required] The name of the storage account for copy destination
        "sourceStorageAccountName": "mediaimports",

        // [Required] The key of the storage account for copy destination
        "destStorageAccountKey": "keyxxx==",

        // [Required] The Blob container name of the storage account for copy destination
        "destContainer":  "movie-trailer",

        // The URL of file as source contents
        //      all blobs in the source container will be copied if no fileNames
        "sourceUrl": "https://....streaming.media.azure.net/110c3675-2384-4f86-bc8e-e261f729bbd0/test-A_1920x1080_6000.mp4"
    }
Output:
    {
        // string return by the copy
        "status": ""
    }

```
*/
//
//

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

using Microsoft.Extensions.Logging;

using Microsoft.WindowsAzure.Storage.Blob;

using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace advanced_vod_functions
{
    public static class CopyBlobBetweenStorage
    {
        [FunctionName("CopyBlobBetweenStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"StartBlobContainerCopyToAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.destFilename == null)
                return new BadRequestObjectResult("Please pass destFilename in the input object");
            if (data.destStorageAccountName == null)
                return new BadRequestObjectResult("Please pass destStorageAccountName in the input object");
            if (data.destStorageAccountKey == null)
                return new BadRequestObjectResult("Please pass destStorageAccountKey in the input object");
            if (data.destContainer == null)
                return new BadRequestObjectResult("Please pass destContainer in the input object");
            if (data.sourceUrl == null)
                return new BadRequestObjectResult("Please pass sourceUrl in the input object");

            string destFilename = data.destFilename;
            string destStorageAccountName = data.destStorageAccountName;
            string destStorageAccountKey = data.destStorageAccountKey;
            string destContainer = data.destContainer;
            string sourceUrl = data.sourceUrl;
            string result = string.Empty;

            try
            {
                CloudStorageAccount csa = new CloudStorageAccount(new StorageCredentials(destStorageAccountName, destStorageAccountKey), true);
                CloudBlobClient blobClient = csa.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(destContainer);

                var newBlockBlob = blobContainer.GetBlockBlobReference(destFilename);
                result = await newBlockBlob.StartCopyAsync(new Uri(sourceUrl));
            }
            catch (Exception e)
            {
                log.LogError($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                status = result
            });
        }
    }
}