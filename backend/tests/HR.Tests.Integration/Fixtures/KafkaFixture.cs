namespace HR.Tests.Integration.Fixtures;

/// <summary>
/// Kafka container fixture for integration tests.
/// Implements IAsyncLifetime for xUnit collection setup/teardown.
/// </summary>
public class KafkaFixture : IAsyncLifetime
{
    private KafkaContainer? _container;
    
    public string? BootstrapServers { get; private set; }

    public async Task InitializeAsync()
    {
        _container = new KafkaBuilder()
            .Build();

        await _container.StartAsync();
        BootstrapServers = _container.GetBootstrapAddress();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    public IProducer<string, string> CreateProducer()
    {
        if (BootstrapServers is null)
            throw new InvalidOperationException("Kafka container not initialized");

        var config = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            Acks = Acks.All,
            RetryBackoffMs = 50,
            MessageTimeoutMs = 5000
        };

        return new ProducerBuilder<string, string>(config)
            .SetValueSerializer(Serializers.Utf8)
            .Build();
    }

    public IConsumer<string, string> CreateConsumer(string groupId, string topic)
    {
        if (BootstrapServers is null)
            throw new InvalidOperationException("Kafka container not initialized");

        var config = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SessionTimeoutMs = 6000
        };

        var consumer = new ConsumerBuilder<string, string>(config)
            .SetValueDeserializer(Deserializers.Utf8)
            .Build();

        consumer.Subscribe(topic);
        return consumer;
    }
}

/// <summary>
/// Collection definition for Kafka tests.
/// </summary>
[CollectionDefinition("Kafka Collection")]
public class KafkaCollection : ICollectionFixture<KafkaFixture>
{
    // This class has no code, and is never created. Its purpose is only
    // to define the collection that this test class belongs to.
}
