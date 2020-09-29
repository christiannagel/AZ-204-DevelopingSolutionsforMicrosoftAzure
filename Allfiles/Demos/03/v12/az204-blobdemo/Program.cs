using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace az204_blobdemo
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Azure Blob Storage Demo\n");
            using var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddUserSecrets("e70aa3fa-11ce-4f6c-b5cd-e5ad67d85f4e");
                })
                .ConfigureServices(services =>
                {
                   
                }).Build();

            var config = host.Services.GetRequiredService<IConfiguration>();
            // Run the examples asynchronously, wait for the results before proceeding

            // add the connection string to user secrets
            string storageConnectionString = config["StorageConnectionString"];

            var serviceClient = new BlobServiceClient(storageConnectionString);
            var containerClient = await CreateContainerAsync(serviceClient);
            (var sourceFile, var itemName) = await UploadBlobAsync(containerClient);
            await ListContainerAsync(containerClient);
            string destinationFile = await DownloadBlobAsync(itemName, sourceFile, containerClient);
            await DeleteContainerAsync(containerClient, sourceFile, destinationFile);

            Console.WriteLine("Press enter to exit the sample application.");
            Console.ReadLine();
        }

        static async Task<BlobContainerClient> CreateContainerAsync(BlobServiceClient serviceClient)
        {
            // Create a container called 'quickstartblobs' and
            // append a GUID value to it to make the name unique.
            var response = await serviceClient.CreateBlobContainerAsync("demolabs" + Guid.NewGuid().ToString(),
                PublicAccessType.Blob);
            
            Console.WriteLine("A container has been created, note the " +
             "'Public access level' in the portal.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
            return response.Value;
        }

        static async Task<(string, string)> UploadBlobAsync(BlobContainerClient containerClient)
        {
            // Create a file in your local MyDocuments folder to upload to a blob.
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string localFileName = "BlobDemo_" + Guid.NewGuid().ToString() + ".txt";
            string sourceFile = Path.Combine(localPath, localFileName);

            // Write text to the file.
            File.WriteAllText(sourceFile, "Hello, World!");
            Console.WriteLine($"\r\nTemp file = {sourceFile}");
            Console.WriteLine($"Uploading to Blob storage as blob '{localFileName}'");

            // Get a reference to the blob address, then upload the file to the blob.
            // Use the value of localFileName for the blob name.
            using var stream = File.OpenRead(sourceFile);
            var response = await containerClient.UploadBlobAsync(localFileName, stream);
            
            Console.WriteLine("\r\nVerify the creation of the blob and upload in the portal.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
            return (sourceFile, localFileName);
        }

        static async Task ListContainerAsync(BlobContainerClient containerClient)
        {
            // List the blobs in the container.
            Console.WriteLine("List blobs in container.");
            var items = containerClient.GetBlobsAsync(BlobTraits.Metadata);
            await foreach (var item in items)
            {
                Console.WriteLine(item.Name);
            }

            Console.WriteLine("\r\nCompare the list in the console to the portal.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
        }

        static async Task<string> DownloadBlobAsync(string itemName, string sourceFile, BlobContainerClient blobContainerClient)
        {
            // Download the blob to a local file, using the reference created earlier.
            // Append the string "_DOWNLOADED" before the .txt extension so that you
            // can see both files in MyDocuments.
            string destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
            Console.WriteLine($"Downloading blob to {destinationFile}");
            using var stream = File.OpenWrite(destinationFile);
            var blobClient = blobContainerClient.GetBlobClient(itemName);
            await blobClient.DownloadToAsync(stream);
            Console.WriteLine("\r\nLocate the local file to verify it was downloaded.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
            return destinationFile;
        }

        static async Task DeleteContainerAsync(BlobContainerClient blobContainerClient, string sourceFile, string destinationFile)
        {
            // Clean up the resources created by the app
            Console.WriteLine("Press the 'Enter' key to delete the example files " +
                "and example container.");
            Console.ReadLine();
            // Clean up resources. This includes the container and the two temp files.
            Console.WriteLine("Deleting the container");
        
            if (blobContainerClient != null)
            {
                await blobContainerClient.DeleteIfExistsAsync();
            }
            Console.WriteLine("Deleting the source, and downloaded files\r\n");
            File.Delete(sourceFile);
            File.Delete(destinationFile);
        }
    }
}
