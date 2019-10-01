package com.github.nilay.kafka.tutorial1;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.common.errors.WakeupException;
import org.apache.kafka.common.serialization.StringDeserializer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.time.Duration;
import java.util.Collections;
import java.util.Properties;
import java.util.concurrent.CountDownLatch;

public class ConsumerDemoWithThread {
    public static void main(String[] args) {
        new ConsumerDemoWithThread().run();
    }
    private ConsumerDemoWithThread(){}
    private void run(){
        Logger logger = LoggerFactory.getLogger(ConsumerDemoWithThread.class.getName());
        //create Producer properties

        String bootstrapServers = "127.0.0.1:9092";
        String groupId = "my-Sixth-application";
        String topic = "first_topic";
        CountDownLatch latch = new CountDownLatch(1);

        logger.info("Creating a consumer thread");
        Runnable myConsumerRunnable = new ConsumerRunnable(latch,
                topic,
                bootstrapServers,
                groupId);

        Thread myThread = new Thread(myConsumerRunnable);
        myThread.start();

        //shutdown hook
        Runtime.getRuntime().addShutdownHook(new Thread( () -> {
            logger.info("shutdown hook");
            ((ConsumerRunnable) myConsumerRunnable).shutdown();
            try {
                latch.await();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }));

        try {
            latch.await();
        } catch (InterruptedException e) {
            logger.error("Application interrupted");
        }
        finally {
            logger.info("Application is closing");
        }
    }

    public class ConsumerRunnable implements Runnable{
        private CountDownLatch latch;
        private Logger logger = LoggerFactory.getLogger(ConsumerRunnable.class.getName());
        KafkaConsumer<String, String> consumer;
        public ConsumerRunnable(CountDownLatch latch, String topic,
                              String bootstrapServers, String groupId)
        {
            this.latch = latch;
            Properties properties = new Properties();
            properties.setProperty(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
            properties.setProperty(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());
            properties.setProperty(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class.getName());
            properties.setProperty(ConsumerConfig.GROUP_ID_CONFIG, groupId);
            properties.setProperty(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");

            //Create consumer
            consumer = new KafkaConsumer<String, String>(properties);

            //subscribe consumer
            consumer.subscribe(Collections.singleton(topic));
        }
        @Override
        public void run() {
            try {
                //poll for data
                while (true) {
                    ConsumerRecords<String, String> records = consumer.poll(Duration.ofMillis(100));

                    for (ConsumerRecord<String, String> record : records) {
                        logger.info("Key: " + record.key() + ", Value: " + record.value());
                        logger.info("Partition: " + record.partition() + ", Offset: " + record.offset());
                    }
                }
            }
            catch(WakeupException e){

                logger.info("Received shutdown signal!");
            }
            finally {
                consumer.close();
                latch.countDown();
            }
        }
        public void shutdown(){
            consumer.wakeup(); //will interrupt consumer.poll
        }
    }
}
