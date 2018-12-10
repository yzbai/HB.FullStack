using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using RabbitMQ.Client;

namespace HB.Infrastructure.RabbitMQ
{
    public class PublishTask
    {
        public Task Task { get; set; }

        public IModel Channel { get; set; }
    }

    /// <summary>
    /// 每一个PublishWokerManager，负责一个broker的工作，自己决定工作量的大小
    /// </summary>
    public class PublishWorkerManager
    {
        private RabbitMQConnectionSetting _connectionSetting;
        private IRabbitMQConnectionManager _connectionManager;
        private IDistributedQueue _queue;

        private LinkedList<PublishTask> _tasks;

        public PublishWorkerManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IDistributedQueue queue)
        {
            _connectionManager = connectionManager;
            _queue = queue;
            _connectionSetting = connectionSetting;

            _tasks = new LinkedList<PublishTask>();

            //task完成后，就从_task里删除
        }


        //Task
        private void PublishToRabbitMQ()
        {
            string threadName = Thread.CurrentThread.Name;
            // 得到当前线程所拥有的channel
            IModel channel = _connectionManager.CreateChannel();

            //设置事件等其他设置
 
            //    setting the channel
            //    ConfirmSelect()
 

            while (true)
            {
                

                //得到Redis当前的消息
                EventMessageEntity entity = _kvStore.Dequeue();

                //如果没有？
            
                //等待一些时间，然后还没有，则推出
            //task完成，就

                //使用channel发布

                //发布完成，从队列中删除

            }

            channel.Close();
        }

        public void NotifyPublishComming()
        {
            //管理
        }
    }
}
