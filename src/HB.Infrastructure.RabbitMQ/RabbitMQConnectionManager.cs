using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQConnectionManager : IRabbitMQConnectionManager
    {
        private RabbitMQEngineOptions _options;
        private ILogger _logger;

        //brokerName:Connection Dictionary
        private IDictionary<string, IConnection> _pubConnectionDict = new Dictionary<string, IConnection>();
        private IDictionary<string, IConnection> _subConnectionDict = new Dictionary<string, IConnection>();
        
        public RabbitMQConnectionManager(IOptions<RabbitMQEngineOptions> options, ILogger<RabbitMQConnectionManager> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public IConnection GetPublishConnection(string brokerName)
        {
            return GetConnection(brokerName, true);
        }

        public IConnection GetSubscribeConnection(string brokerName)
        {
            return GetConnection(brokerName, false);
        }

        private IConnection GetConnection(string brokerName, bool isPublishConnection)
        {
            if (string.IsNullOrEmpty(brokerName))
            {
                throw new ArgumentNullException(nameof(brokerName));
            }

            IConnection connection = null;

            if (isPublishConnection && _pubConnectionDict.ContainsKey(brokerName))
            {
                connection = _pubConnectionDict[brokerName];
            }

            if (!isPublishConnection && _subConnectionDict.ContainsKey(brokerName))
            {
                connection = _subConnectionDict[brokerName];
            }

            if (connection != null && IsConnected(connection))
            {
                return connection;
            }

            if (connection != null)
            {
                connection.Dispose();
            }

            //Now we have to make new connection
            connection = CreateNewConnection(brokerName);

            if (isPublishConnection)
            {
                _pubConnectionDict[brokerName] = connection;
            }
            else
            {
                _subConnectionDict[brokerName] = connection;
            }

            return connection;
        }

        /// <summary>
        /// no cached
        /// </summary>
        /// <param name="brokerName"></param>
        /// <param name="isPublish"></param>
        /// <returns></returns>
        public IModel CreateChannel(string brokerName, bool isPublish)
        {
            IConnection connection = GetConnection(brokerName, isPublish);

            return connection.CreateModel();
        }

        private bool IsConnected(IConnection connection)
        {
            if (connection == null)
            {
                _logger.LogError("发现一个为null的RabbitMQ Connection");
            }

            if (connection.CloseReason != null)
            {
                _logger.LogError($"发现一个关闭了的RabbitMQ Connection, 原因：{connection.CloseReason}");

                return false;
            }

            return true;
        }

        #region Connection Creat

        private IConnection CreateNewConnection(string brokerName)
        {
            IConnection connection;
            RabbitMQConnectionSetting connectionSetting = _options.GetConnectionSetting(brokerName);

            if (connectionSetting == null)
            {
                Exception ex = new Exception($"无法找到RabbitMQ ，没有对应broker名字的配置，brokername：{brokerName}");
                _logger.LogCritical(ex, $"RabbitMQ 配置不对，找不到brokername为{brokerName}的配置");

                throw ex;
            }

            //TODO: add polly here, make sure the connection is established, or just keep trying
            //TODO: add log here

            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.Uri = new Uri(connectionSetting.ConnectionString);
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(_options.NetworkRecoveryIntervalSeconds);
            connectionFactory.AutomaticRecoveryEnabled = true;

            connection = connectionFactory.CreateConnection();

            connection.CallbackException += Connection_CallbackException;
            connection.ConnectionBlocked += Connection_ConnectionBlocked;
            connection.ConnectionUnblocked += Connection_ConnectionUnblocked;
            connection.RecoverySucceeded += Connection_RecoverySucceeded;
            connection.ConnectionRecoveryError += Connection_ConnectionRecoveryError;
            connection.ConnectionShutdown += Connection_ConnectionShutdown;

            return connection;
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning(e.ToString());
        }

        private void Connection_ConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            _logger.LogCritical(e.Exception, $"RabbitMQ Connection Recovery Error, Exception:{e.Exception.Message}");
        }

        private void Connection_RecoverySucceeded(object sender, EventArgs e)
        {
            _logger.LogWarning("RabbitMQ Recovery suncceeded.");
        }

        private void Connection_ConnectionUnblocked(object sender, EventArgs e)
        {
            _logger.LogWarning("RabbitMQ Connection Unblocked.");
        }

        private void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning($"RabbitMQ Connection Blocked. Reason:{e.Reason}");
        }

        private void Connection_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            string detailMessage = "RabbitMQ Connection Exception:";

            if (e != null & e.Detail != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(detailMessage);

                foreach (KeyValuePair<string, object> pair in e.Detail)
                {
                    stringBuilder.AppendLine($"{pair.Key} : {pair.Value.ToString()}");
                }

                detailMessage = stringBuilder.ToString();
            }

            _logger.LogError(detailMessage);
        }

        #endregion

        #region IDisposable Support

        private bool _disposed = false; // To detect redundant calls
        private readonly object _disposeLocker = new object();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    lock (_disposeLocker)
                    {
                        if (_subConnectionDict != null)
                        {
                            foreach (KeyValuePair<string, IConnection> item in _subConnectionDict)
                            {
                                item.Value?.Close();
                            }
                        }

                        if (_pubConnectionDict != null)
                        {
                            foreach(KeyValuePair<string, IConnection> item in _pubConnectionDict)
                            {
                                item.Value?.Close();
                            }
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~RabbitMQConnectionManager()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        
        #endregion
    }
}
