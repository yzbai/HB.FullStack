using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            ThreadStart threadStart = new ThreadStart(() => { });
            Thread t = new Thread(threadStart);


            IHostBuilder hostBuilder = new HostBuilder()
                .ConfigureServices((hostBuilderContext, services) => { });
                

            IHost host = hostBuilder.Build();

            host.Run();

            Console.WriteLine("Hello World!");


            //应该Retry
            ConnectionFactory connectionFactory = new ConnectionFactory();

            connectionFactory.Uri = new Uri("amqp://admin:_admin@localhost:5672/");
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
            connectionFactory.AutomaticRecoveryEnabled = true;

            //单例
            IConnection connection = connectionFactory.CreateConnection();

            connection.CallbackException += (sender, eventArgs) => { /*Try reconnect*/};
            connection.ConnectionBlocked += (sender, eventArgs) => { /*Try reconnect*/ };
            connection.ConnectionRecoveryError += (sender, eventArgs) => { };
            connection.ConnectionShutdown += (sender, eventArgs) => { /*Try reconnect*/ };
            connection.ConnectionUnblocked += (sender, eventArgs) => { };
            connection.RecoverySucceeded += (sender, eventArgs) => { };

            //Transient, 可以考虑复用, 复用，需要用lock， 每一次lock，只能publish一次，
            IModel channel = connection.CreateModel();

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

            //channel.WaitForConfirms();

            //channel.ExchangeDeclare("testExchange", ExchangeType.Direct);
            //channel.QueueDeclare("hello", false, false, false, null);

            //channel.QueueBind("testQueue", "testExchange", "testRoutingKey");



            Task.Run(() => 
            {
                //Thread.Sleep(5000);

                channel.ConfirmSelect();
                

                for (int i =0; i < 100000; i++)
                {
                    byte[] data = Encoding.UTF8.GetBytes($"Publisher : hello, there. This is {i} th times.");

                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    basicProperties.ContentType = "text/plain";
                    basicProperties.DeliveryMode = 2;

                    //channel.BasicPublish("", "testRoutingKey", basicProperties, Encoding.UTF8.GetBytes($"Publisher : hello, there. This is {i} th times."));


                    //channel.BasicPublish()
                    ulong number = channel.NextPublishSeqNo;
                    channel.BasicPublish(exchange: "", routingKey: "hello", mandatory: false, basicProperties: basicProperties, body: data);

                    //当用default direct exchange "" 时，routingkey 可以直接是queue name。默认binding了。
                    //channel.BasicPublish("testExchange", "test", basicProperties, data);


                    

                    //需要检测有没有被reject with an exception，如果时，需要自己retry。 这个时候channel不会auto recover。
                    //再下来，消息到底有没有被送达，就要靠 publisher confirms了。

                    

                    Thread.Sleep(new Random().Next(1, 2) * 1000);
                }
            });

            //一个接一个处理，别一次性给多个
            channel.BasicQos(0, 1, false);


            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArgs) =>
            {
                byte[] data = eventArgs.Body;

                string message = Encoding.UTF8.GetString(data);

                Console.WriteLine($"Consumer : Received {message}");

                if (eventArgs.Redelivered)
                {
                    //log, we have found a redelivered.
                }

                channel.BasicAck(eventArgs.DeliveryTag, false);


            };

            string queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queueName, "testExchange", "test");

            string consumerTag = channel.BasicConsume(queueName, false, consumer);


            while (true)
            {
                char c = Console.ReadKey().KeyChar; 

                if(c.Equals('q'))
                {
                    break;
                }
            }

            //channel.BasicCancel(consumerTag);

            channel.Close();
            connection.Close();


            //publish confirms

        }

        static void Main2(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            // configure various connection settings

            try
            {
                IConnection conn = factory.CreateConnection();
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
            {
                Thread.Sleep(5000);
                // apply retry logic
            }
            finally
            {
                
            }
        }
    }
}
