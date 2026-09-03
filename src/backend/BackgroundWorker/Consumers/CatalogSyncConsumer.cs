using Application.Common.Interfaces;
using Azure.Messaging.ServiceBus;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace BackgroundWorker.Consumers;

public class CatalogSyncConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CatalogSyncConsumer> _logger;

    public CatalogSyncConsumer(ServiceBusClient client, IServiceProvider serviceProvider, ILogger<CatalogSyncConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processor = client.CreateProcessor("catalog-sync-queue", new ServiceBusProcessorOptions());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var rawMessage = JsonSerializer.Deserialize<CatalogSyncPayload>(body);

        if (rawMessage != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ai = scope.ServiceProvider.GetRequiredService<IOpenAiEmbeddingService>();

            // Clean domain creation blueprint
            var product = new Product(Guid.NewGuid(), rawMessage.Name, rawMessage.Description, rawMessage.Sku, rawMessage.Price, rawMessage.Category);

            // Build visual text block layout contextual payload for vector transformations
            string textBlob = $"Product: {product.Name}. Classification: {product.Category}. Detail Context: {product.Description}";
            float[] vectorSpace = await ai.GenerateEmbeddingAsync(textBlob, args.CancellationToken);

            product.AssignEmbedding(vectorSpace);

            db.Products.Add(product);
            await db.SaveChangesAsync(args.CancellationToken);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus ingestion runtime exception inside stream loop handler.");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}

public record CatalogSyncPayload(string Name, string Description, string Sku, decimal Price, string Category);
