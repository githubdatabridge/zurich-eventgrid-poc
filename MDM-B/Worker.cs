using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Shared.Lib.Services;
using Shared.Lib.Services.FileStorage;
using System.Text.Json;

namespace MDM.B
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly QueueClient _queueClient;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, QueueServiceClient queueClient)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _queueClient = queueClient.GetQueueClient("mdm-b-queue");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _queueClient.CreateIfNotExistsAsync();

                    var receiveMessageResult = await _queueClient.ReceiveMessageAsync();

                    if (receiveMessageResult == null || receiveMessageResult.Value == null)
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var message = receiveMessageResult.Value;

                    await ProcessMessage(message, stoppingToken);

                    // Delete the message
                    _queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
                }
                catch (Exception)
                {
                    throw;
                }

               
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessMessage(QueueMessage message, CancellationToken stoppingToken)
        {
            BlobEvent? blobEvent = DecodeMessage(message);

            if (blobEvent != null && blobEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IFileStoreService inputStorage = scope.ServiceProvider.GetRequiredService<InputStorageService>();
                    IFileStoreService outputStorage = scope.ServiceProvider.GetRequiredService<OutputStorageService>();

                    var result = await inputStorage.Exists(blobEvent.FileName, null);
                    var verId = await inputStorage.GetVersionId(blobEvent.FileName);
                    _logger.LogInformation($"File with name {blobEvent.FileName} exists: [{result}] VersionId: {verId}");

                    var progress = new Progress<long>(bytes =>
                    {
                        _logger.LogInformation($"Downloading bytes: {bytes}");
                    });

                    using var stream = await inputStorage.GetFileAsync(blobEvent.FileName, progress, stoppingToken);

                    if (stream == null)
                    {
                        _logger.LogError($"File Get Failed");
                        return;
                    }

                    _logger.LogInformation($"Processing {blobEvent.FileName} ...");
                    await Task.Delay(5000);

                    var tags = new Dictionary<string, string>() {
                                { "name", blobEvent.FileName },
                                { "versionId", verId },
                                { "status", "DONE" }
                            };

                    progress = new Progress<long>(bytes =>
                    {
                        _logger.LogInformation($"Uploading bytes: {bytes}");
                    });

                    var putResult = await outputStorage.PutFileAsync(stream, blobEvent.FileName, tags, progress, cancellationToken: stoppingToken);
                    if (!putResult)
                    {
                        _logger.LogError($"File Put Failed");
                        return;
                    }

                    _logger.LogInformation($"File with name {blobEvent.FileName} processed successfully.");

                }
            }
        }

        private static BlobEvent? DecodeMessage(QueueMessage message)
        {
            var blobEventJson = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(message.Body.ToString()));
            var blobEvent = JsonSerializer.Deserialize<BlobEvent>(
                blobEventJson,
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return blobEvent;
        }
    }
}