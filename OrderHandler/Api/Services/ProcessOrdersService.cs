namespace Api.Services
{
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using Api.Models;
    using Newtonsoft.Json;
    using Confluent.Kafka;

    public class ProcessOrdersService : BackgroundService
    {
        private readonly ConsumerConfig consumerConfig;
        private readonly ProducerConfig producerConfig;
        public ProcessOrdersService(ConsumerConfig consumerConfig, ProducerConfig producerConfig)
        {
            this.producerConfig = producerConfig;
            this.consumerConfig = consumerConfig;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("OrderProcessing Service Started");
            /*var conf = new ConsumerConfig
            { 
                GroupId = "csharp-consumer",
                BootstrapServers = "localhost:9092",
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest,
                //EnableAutoCommit = true,
                //StatisticsIntervalMs = 5000,
                //SessionTimeoutMs = 6000
            };*/
            while (!stoppingToken.IsCancellationRequested)
            {
                
                var consumerHelper = new ConsumerWrapper(consumerConfig, "orderrequests");
                string orderRequest = consumerHelper.readMessage();

                
                //Deserilaize 
                OrderRequest order = JsonConvert.DeserializeObject<OrderRequest>(orderRequest);

                //TODO:: Process Order
                Console.WriteLine($"Info: OrderHandler => Processing the order for {order.productname}");
                order.status = OrderStatus.COMPLETED;

                //Write to ReadyToShip Queue

                var producerWrapper = new ProducerWrapper(producerConfig,"readytoship");
                await producerWrapper.writeMessage(JsonConvert.SerializeObject(order));
            }
        }
    }
}