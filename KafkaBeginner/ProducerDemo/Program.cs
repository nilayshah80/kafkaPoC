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
            var config = new ProducerConfig {BootstrapServers = "localhost:9092"};
            
            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            using (var p = new ProducerBuilder<string, string>(config).Build())
            {
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var dr = await p.ProduceAsync("first_topic", new Message<string, string> {Key = $"id_{i.ToString()}",Value = $"hello world {i.ToString()}"});

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
            }
        }
    }
}