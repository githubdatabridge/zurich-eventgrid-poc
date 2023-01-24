using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Shared.Lib.Services;
using Shared.Lib.Services.FileStorage;
using System.Text.Json;

namespace CLA.Admin.Beckend
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly QueueClient _queueClient;
        private readonly ProcessService _processService;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, QueueServiceClient queueClient, ProcessService processService)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _queueClient = queueClient.GetQueueClient("cla-queue");
            _processService = processService;
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

                    QueueMessage message = receiveMessageResult.Value;
                    
                    await ProcessMessage(message);

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

        private async Task ProcessMessage(QueueMessage message)
        {
            BlobEvent? blobEvent = DecodeMessage(message);

            if (blobEvent != null && blobEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IFileStoreService container2 = scope.ServiceProvider.GetRequiredService<OutputStorageService>();

                    var result = await container2.Exists(blobEvent.FileName, null);
                    var tags = await container2.GetTags(blobEvent.FileName);
                    if (tags != null)
                    {
                        var versionId = tags["versionId"];

                        if (versionId != null)
                        {
                            var model = _processService.List.Single(x => x.VersionId == versionId);
                            model.Status = ProcessService.ProcessStatusType.Done;
                            _processService.Update(model);
                            _logger.LogInformation($"Process model updated successfully.");
                        }
                    }
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