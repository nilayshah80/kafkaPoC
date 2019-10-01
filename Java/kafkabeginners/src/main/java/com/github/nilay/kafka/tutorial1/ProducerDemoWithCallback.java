package com.github.nilay.kafka.tutorial1;

import org.apache.kafka.clients.producer.*;
import org.apache.kafka.common.serialization.StringSerializer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Properties;

//https://stackoverflow.com/questions/52788743/intellij-error-java-release-version-10-not-supported/54963753
//code compiles only after changing project byte code 8 and remove line from target byte code
public class ProducerDemoWithCallback {
    public static void main(String[] args) {
        final Logger logger = LoggerFactory.getLogger(ProducerDemoWithCallback.class);
        //create Producer properties
        Properties properties = new Properties();
        String bootstrapServers = "127.0.0.1:9092";
        properties.setProperty(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        properties.setProperty(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        properties.setProperty(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG,StringSerializer.class.getName());

        //create the producer
        final KafkaProducer<String, String> producer = new KafkaProducer<String, String>(properties);
        for(int i=0;i<10;i++){
            // create producer record
            ProducerRecord<String, String> record = new ProducerRecord<String, String>("first_topic", "hello world" + Integer.toString(i));
            //send data - asynchronous -- so must wait
            producer.send(record, new Callback() {
                public void onCompletion(RecordMetadata recordMetadata, Exception e) {
                    //executes every time a record is successfully sent or and exception is thrown
                    if(e == null)
                    {
                        logger.info("Received new metadata \n" +
                                "Topic:" + recordMetadata.topic() + "\n" +
                                "Partition:" + recordMetadata.partition() + "\n" +
                                "Offset:" + recordMetadata.offset()  + "\n" +
                                "Timestamp: " + recordMetadata.timestamp());

                    }
                    else{
                        logger.error("Error while producing ", e);
                    }
                }
            });
        }

        producer.flush();
        producer.close();
    }
}
