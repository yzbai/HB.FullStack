using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQDemo
{
    class Program
    {
        static string consumerTag;

        static void Main(string[] args)
        {

            testSerilize();

            //应该Retry
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Uri = new Uri("amqp://admin:_admin@127.0.0.1:5672/");
            //connectionFactory.Uri = new Uri("amqp://admin:_admin@192.168.0.112:5672/");
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
            connectionFactory.AutomaticRecoveryEnabled = true;

            //单例
            IConnection connection = connectionFactory.CreateConnection();

            #region connection event
            connection.CallbackException += (sender, eventArgs) => 
            { 
                /*Try reconnect*/
            };
            connection.ConnectionBlocked += (sender, eventArgs) =>
            {
                /*Try reconnect*/
            };
            connection.ConnectionRecoveryError += (sender, eventArgs) =>
            {
                /*Try reconnect*/
            };
            connection.ConnectionShutdown += (sender, eventArgs) =>
            {
                /*Try reconnect*/
            };
            connection.ConnectionUnblocked += (sender, eventArgs) =>
            {
                /*Try reconnect*/
            };
            connection.RecoverySucceeded += (sender, eventArgs) =>
            {
                /*Try reconnect*/
            };
            #endregion

            //start Pub
            //Task.Run(() =>
            //{
            //    Pub(connection, "11111111111111");
            //});

            //Task.Run(() =>
            //{
            //    Pub(connection, "333333333333333");
            //});

            //Task.Run(() =>
            //{
            //    Pub(connection, "4444444444444444");
            //});

            //Task.Run(() =>
            //{
            //    Pub(connection, "22222222222222222");
            //});

            //Task.Run(() =>
            //{
            //    Sub(connection, 1);
            //});

            //Task.Run(() =>
            //{
            //    Sub(connection, 2);
            //});

            //Task.Run(() =>
            //{
            //    Publish(connection, 3);
            //});

            //Task.Run(() =>
            //{
            //    Publish(connection, 4);
            //});


            //Task.Run(() => {
            //    Sub(connection, 10);
            //});

            while (true)
            {
                char c = Console.ReadKey().KeyChar;

                if (c.Equals('q'))
                {
                    break;
                }

                if (c.Equals('c'))
                {
                    IModel channel = connection.CreateModel();

                    channel.BasicCancel(consumerTag);
                }
            }

            connection.Close();

        }

        private static void testSerilize()
        {
            EventMessage eventMessage = new EventMessage("xxx", DataConverter.GetUTF8Bytes("hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh"));

            byte[] result = DataConverter.SerializeUseMsgPack<EventMessage>(eventMessage);

            EventMessage event2 = new EventMessage("yyy", result);

            byte[] result2 = DataConverter.SerializeUseMsgPack<EventMessage>(event2);

            EventMessage e22 = DataConverter.DeSerializeUseMsgPack<EventMessage>(result2);

            EventMessage e11 = DataConverter.DeSerializeUseMsgPack<EventMessage>(e22.Body);

            string message = DataConverter.GetUTF8String(e11.Body);

            Console.WriteLine(message);
        }

        private static void Sub(IConnection connection, int name)
        {
            IModel channel = connection.CreateModel();

            #region channel event
            channel.BasicReturn += (sender, eventArgs) =>
            {
                Console.WriteLine($"broker return : {eventArgs.Exchange} : {eventArgs.RoutingKey}");
            };

            channel.BasicAcks += (sender, eventArgs) =>
            {
                Console.WriteLine($"broker ack : {eventArgs.DeliveryTag}");
            };

            channel.BasicNacks += (sender, eventArgs) =>
            {

                Console.WriteLine($"broker nack : {eventArgs.DeliveryTag}");
            };

            channel.BasicRecoverOk += (sender, eventArgs) =>
            {

            };

            channel.CallbackException += (sender, eventArgs) =>
            {
                //Re create channel
            };

            channel.FlowControl += (sender, eventArgs) =>
            {

            };

            channel.ModelShutdown += (sender, eventArgs) =>
            {

            };

            #endregion

            //channel.WaitForConfirms();

            //channel.ExchangeDeclare("testExchange", ExchangeType.Direct);
            //channel.QueueDeclare("hello", false, false, false, null);

            //channel.QueueBind("testQueue", "testExchange", "testRoutingKey");

            //一个接一个处理，别一次性给多个

            channel.ExchangeDeclare("DefaultTopic2", ExchangeType.Topic, true, false);

            channel.BasicQos(0, 1, false);


            
            

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArgs) =>
            {
                byte[] data = eventArgs.Body;

                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine($"Consumer {name} : Received {message}");

                if (eventArgs.Redelivered)
                {
                    //log, we have found a redelivered.
                    Console.WriteLine(" we have found a redelivered");
                }

                channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            string queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queueName, "DefaultTopic2", "a.b.c");

            consumerTag = channel.BasicConsume(queueName, false, consumer);

           

            //channel.BasicCancel(consumerTag);

            //channel.Close();

            Console.WriteLine("Leave");
        }

        private static void Pub(IConnection connection, string pubName)
        {
            IModel channel = connection.CreateModel();

            #region channel event
            channel.BasicReturn += (sender, eventArgs) =>
            {
                Console.WriteLine($"{pubName} broker return : {eventArgs.Exchange} : {eventArgs.RoutingKey}");
            };

            channel.BasicAcks += (sender, eventArgs) =>
            {
                Console.WriteLine($"{pubName} broker ack : {eventArgs.DeliveryTag}");
            };

            channel.BasicNacks += (sender, eventArgs) =>
            {

                Console.WriteLine($"{pubName} broker nack : {eventArgs.DeliveryTag}");
            };

            channel.BasicRecoverOk += (sender, eventArgs) =>
            {

            };

            channel.CallbackException += (sender, eventArgs) =>
            {
                //Re create channel
            };

            channel.FlowControl += (sender, eventArgs) =>
            {

            };

            channel.ModelShutdown += (sender, eventArgs) =>
            {

            };

            #endregion

            //channel.QueueDeclarePassive("xxxxxxxxxxxxxxxxxx");

            //channel.WaitForConfirms();

            //channel.ExchangeDeclare("testExchange", ExchangeType.Direct);
            //channel.QueueDeclare("hello", false, false, false, null);

            //channel.QueueBind("testQueue", "testExchange", "testRoutingKey");
            //Thread.Sleep(5000);

            //channel.ConfirmSelect();


            channel.ExchangeDeclare("DefaultTopic2", ExchangeType.Topic, true, false);
            //channel.QueueDeclare("DefaultTopicQueue2");
            //channel.QueueBind("DefaultTopicQueue2", "DefaultTopic2", "DefaultTopicRK2");

            Thread.Sleep(5 * 1000);

            for (int i = 0; i < 1000000; i++)
            {
                byte[] data = Encoding.UTF8.GetBytes($"{pubName} Publisher : hello, there. This is {i} th times.");

                IBasicProperties basicProperties = channel.CreateBasicProperties();
                basicProperties.ContentType = "text/plain";
                basicProperties.DeliveryMode = 2;

                ulong number = channel.NextPublishSeqNo;
                Console.WriteLine($"{pubName} 接下来的序号：{number}");

                channel.BasicPublish("DefaultTopic2", "a.b.c", true, basicProperties, data);


                Thread.Sleep(1000);

                //channel.BasicPublish("", "testRoutingKey", basicProperties, Encoding.UTF8.GetBytes($"Publisher : hello, there. This is {i} th times."));


                //channel.BasicPublish()
                //ulong number = channel.NextPublishSeqNo;
                //channel.BasicPublish(exchange: "", routingKey: "hello", mandatory: false, basicProperties: basicProperties, body: data);

                //当用default direct exchange "" 时，routingkey 可以直接是queue name。默认binding了。
                //channel.BasicPublish("testExchange", "test", basicProperties, data);




                //需要检测有没有被reject with an exception，如果时，需要自己retry。 这个时候channel不会auto recover。
                //再下来，消息到底有没有被送达，就要靠 publisher confirms了。



                //Thread.Sleep(new Random().Next(1, 2) * 1000);
            }
        }
    }
}
