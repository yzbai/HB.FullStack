using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.Common.Utility;
using HB.Infrastructure.Redis.Direct;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.RabbitMQ
{


    //public RedisEngineResult PopAndPushIfNotExist<T>(string redisInstanceName, string historyQueue, string queue, string hashName)
    //{
    //    IDatabase database = GetDatabase(redisInstanceName);

    //    RedisKey[] keys = new RedisKey[] { historyQueue, hashName, queue };
    //    RedisValue[] args = new RedisValue[] { DataConverter.CurrentTimestampSeconds(), };

    //    database.ScriptEvaluate("", )
    //    }


    public class HistoryTaskManager : RabbitMQAndDistributedQueueDynamicTaskManager
    {
        private const string _popAndPushIfNotExistInHashScript = "local rawEvent = redis.call('rpop', KEYS[1]) local event = cjson.decode(rawEvent) local diffTime = ARGV[1] - event[\"Timestamp\"] local eid = event[\"Id\"] if (diffTime < ARGV[2] + 0) then redis.call('rpush', KEYS[1], rawEvent) return 1 end if (redis.call('hexists', KEYS[2], eid) == 1) then redis.call('hdel', KEYS[2], eid) return 2 end redis.call('rpush', KEYS[3], rawEvent) return 3";

        public HistoryTaskManager(RabbitMQConnectionSetting connectionSetting, IRabbitMQConnectionManager connectionManager, IRedisDatabase redis, ILogger logger)
            : base(connectionSetting, connectionManager, redis, logger) { }

        protected override void TaskProcedure()
        {
            _logger.LogTrace($"ScanHistory Task Start, ThreadID:{Thread.CurrentThread.ManagedThreadId}");

            try
            {
                while (true)
                {
                    string[] redisKeys = new string[] { DistributedHistoryQueueName, DistributedConfirmIdHashName, DistributedQueueName };
                    string[] redisArgvs = new string[] { TimeUtil.CurrentTimestampSeconds().ToString(), _connectionSetting.WaitSecondsToBeAHistory.ToString() };

                    int result = _redis.ScriptEvaluate(
                        redisInstanceName: _connectionSetting.RedisInstanceName,
                        script: _popAndPushIfNotExistInHashScript,
                        keys: redisKeys,
                        argvs: redisArgvs);

                    //TODO: add logs

                    if (result == 1)
                    {
                        //时间太早，等会再检查
                        Thread.Sleep(10000);
                    }
                    else if (result == 2)
                    {
                        //成功
                    }
                    else if (result == 3)
                    {
                        //重新放入队列再发送
                    }
                    else
                    {
                        //出错
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"ScanHistory {_connectionSetting.BrokerName} 中，Thread Id : {Thread.CurrentThread.ManagedThreadId}, Exceptions: {ex.Message}");
            }
            finally
            {
                //Thread.Sleep();
                OnTaskFinished();
            }
        }

        protected override ulong PerThreadFacingCount()
        {
            return _connectionSetting.PerThreadFacingHistoryEventCount;
        }

        protected override string CurrentWorkloadQueueName()
        {
            return DistributedHistoryQueueName;
        }

        protected override int MaxWorkerThread()
        {
            return _connectionSetting.MaxHistoryWorkerThread;
        }
    }
}
