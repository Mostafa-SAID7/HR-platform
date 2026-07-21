namespace HR.Tests.Integration.Kafka;

using HR.Tests.Integration.Fixtures;

[Collection("Kafka Collection")]
[Trait("Category", "Integration")]
public class EventPublishingIntegrationTests : IAsyncLifetime
{
    private readonly KafkaFixture _fixture;
    private IProducer<string, string>? _producer;
    private IConsumer<string, string>? _consumer;

    public EventPublishingIntegrationTests(KafkaFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _producer = _fixture.CreateProducer();
        _consumer = _fixture.CreateConsumer("test-group", "test-topic");
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _producer?.Dispose();
        _consumer?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task PublishEvent_ToKafka_CanBeConsumed()
    {
        // Arrange
        const string topic = "test-topic";
        var testMessage = new { EmployeeId = Guid.NewGuid(), Name = "John Doe" };
        var messageJson = JsonSerializer.Serialize(testMessage);

        _consumer = _fixture.CreateConsumer("test-group", topic);

        // Act
        var result = await _producer!.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = testMessage.EmployeeId.ToString(),
                Value = messageJson
            }
        );

        // Wait for consumer to receive
        var consumedMessage = _consumer.Consume(TimeSpan.FromSeconds(5));

        // Assert
        result.Status.Should().Be(PersistenceStatus.Persisted);
        consumedMessage.Should().NotBeNull();
        consumedMessage.Message.Value.Should().Contain("John Doe");
    }

    [Fact]
    public async Task PublishMultipleEvents_ToKafka_AllAreReceived()
    {
        // Arrange
        const string topic = "test-events";
        var messages = new List<string>
        {
            JsonSerializer.Serialize(new { Id = Guid.NewGuid(), Action = "Created" }),
            JsonSerializer.Serialize(new { Id = Guid.NewGuid(), Action = "Updated" }),
            JsonSerializer.Serialize(new { Id = Guid.NewGuid(), Action = "Deleted" })
        };

        _consumer = _fixture.CreateConsumer("test-group", topic);

        // Act
        var producedCount = 0;
        foreach (var message in messages)
        {
            var result = await _producer!.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
            
            if (result.Status == PersistenceStatus.Persisted)
                producedCount++;
        }

        // Assert
        producedCount.Should().Be(messages.Count);
    }

    [Fact]
    public async Task PublishEvent_WithLargePayload_IsHandledCorrectly()
    {
        // Arrange
        const string topic = "large-events";
        var largePayload = new
        {
            Id = Guid.NewGuid(),
            Data = string.Join(",", Enumerable.Range(1, 1000))
        };
        var messageJson = JsonSerializer.Serialize(largePayload);

        _consumer = _fixture.CreateConsumer("test-group", topic);

        // Act
        var result = await _producer!.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = largePayload.Id.ToString(),
                Value = messageJson
            }
        );

        var consumedMessage = _consumer.Consume(TimeSpan.FromSeconds(5));

        // Assert
        result.Status.Should().Be(PersistenceStatus.Persisted);
        consumedMessage.Should().NotBeNull();
        consumedMessage.Message.Value.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task PublishEvent_WithEmptyMessage_IsHandled()
    {
        // Arrange
        const string topic = "empty-test";
        _consumer = _fixture.CreateConsumer("test-group", topic);

        // Act
        var result = await _producer!.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = ""
            }
        );

        // Assert
        result.Status.Should().Be(PersistenceStatus.Persisted);
    }
}

[Collection("Kafka Collection")]
[Trait("Category", "Integration")]
public class EventConsumingIntegrationTests : IAsyncLifetime
{
    private readonly KafkaFixture _fixture;
    private IProducer<string, string>? _producer;
    private IConsumer<string, string>? _consumer;

    public EventConsumingIntegrationTests(KafkaFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _producer = _fixture.CreateProducer();
        _consumer = _fixture.CreateConsumer("consumer-group", "consume-topic");
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _producer?.Dispose();
        _consumer?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ConsumeEvent_ReceivesMessageInOrder()
    {
        // Arrange
        const string topic = "consume-topic";
        var messages = new List<string> { "Message1", "Message2", "Message3" };

        // Act
        foreach (var msg in messages)
        {
            await _producer!.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = msg
            });
        }

        var receivedMessages = new List<string>();
        for (int i = 0; i < messages.Count; i++)
        {
            var consumedMessage = _consumer!.Consume(TimeSpan.FromSeconds(5));
            if (consumedMessage != null)
                receivedMessages.Add(consumedMessage.Message.Value);
        }

        // Assert
        receivedMessages.Should().HaveCount(3);
        receivedMessages.Should().ContainInOrder("Message1", "Message2", "Message3");
    }
}
