using System;
using System.Globalization;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace ProducerDemo
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            //await ProducerDemoBasic();
            ProducerDemoWithCallback();

        }

        public static async Task ProducerDemoBasic()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                Acks = Acks.All,
                EnableIdempotence = true,
                CompressionType = CompressionType.Snappy
            };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            //https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/src/Confluent.Kafka/Serializers.cs
            using (var producer = new ProducerBuilder<string, string>(config)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .SetErrorHandler((_, e) =>
                Console.WriteLine($"Error: {e.Reason}")
            )
            .SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
            .Build())
            {
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var dr = await producer.ProduceAsync("first_topic", new Message<string, string> { Key = $"id_{i.ToString()}", Value = $"hello world {i.ToString()}" });

                        Console.WriteLine($"Delivered '{dr.Key}' to '{dr.TopicPartitionOffset}'");
                        Console.WriteLine("Received new metadata.");
                        Console.WriteLine($"Topic: {dr.Topic}");
                        Console.WriteLine($"Partition: {dr.Partition.Value.ToString()}");
                        Console.WriteLine($"Offset: {dr.Offset.Value.ToString()}");
                        Console.WriteLine(
                            $"Timestamp: {dr.Timestamp.UtcDateTime.Date.ToString(DateTimeFormatInfo.InvariantInfo)}");
                    }
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
                finally
                {
                    producer.Flush(new TimeSpan(0, 0, 10));
                    producer.Dispose();
                }
            }
        }

        public static void ProducerDemoWithCallback()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                Acks = Acks.All,
                EnableIdempotence = true,
                CompressionType = CompressionType.Snappy
            };

            Action<DeliveryReport<string, string>> handler = dr =>
            {
                //Console.WriteLine(!r.Error.IsError
                //    ? $"Delivered message to {r.TopicPartitionOffset}"
                //    : $"Delivery Error: {r.Error.Reason}");
                if(!dr.Error.IsError)
                {
                    Console.WriteLine($"Delivered '{dr.Key}' to '{dr.TopicPartitionOffset}'");
                    Console.WriteLine("Received new metadata.");
                    Console.WriteLine($"Topic: {dr.Topic}");
                    Console.WriteLine($"Partition: {dr.Partition.Value.ToString()}");
                    Console.WriteLine($"Offset: {dr.Offset.Value.ToString()}");
                    Console.WriteLine(
                        $"Timestamp: {dr.Timestamp.UtcDateTime.Date.ToString(DateTimeFormatInfo.InvariantInfo)}");
                }
                else
                {
                    Console.WriteLine($"Delivery Error: {dr.Error.Reason}");
                }
            };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            //https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/src/Confluent.Kafka/Serializers.cs
            using (var producer = new ProducerBuilder<string, string>(config)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .SetErrorHandler((_, e) =>
                Console.WriteLine($"Error: {e.Reason}")
            )
            .SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
            .Build())
            {
                for (int i = 0; i < 10; i++)
                {
                    producer.Produce("first_topic",
                        new Message<string, string> { Key = "key_" + i.ToString(), Value = "value_" + i.ToString() },
                        handler
                    );
                }
                producer.Flush(TimeSpan.FromSeconds(10));
            }
        }
    }
}