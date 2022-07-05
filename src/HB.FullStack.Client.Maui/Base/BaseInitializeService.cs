﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.Maui.Base
{
    public class BaseInitializeService : IMauiInitializeService
    {
        private readonly IEnumerable<Migration>? _migrations;

        public BaseInitializeService(IEnumerable<Migration>? migrations = null)
        {
            _migrations = migrations;
        }

        public void Initialize(IServiceProvider services)
        {
            #region Logging & Exceptions

            GlobalSettings.Logger = services.GetRequiredService<ILogger<BaseInitializeService>>();

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 上报

                GlobalSettings.Logger.LogError(e.Exception, $"发现没有处理的UnobservedTaskException。Sender: {sender?.GetType().FullName}");

                Currents.ShowToast("抱歉，发生了错误");

                e.SetObserved();
            };

            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex =>
            {
                //TODO:上报

                GlobalSettings.Logger.LogError(ex, "使用了SafeFireAndForget的默认异常处理");


                Currents.ShowToast("抱歉，发生了错误");
            });

            #endregion

            //Database
            //TODO: 是否在这里等他是最好的选择？
            IDatabase database = services.GetRequiredService<IDatabase>();

            //Currents.AppendingTasks.Add(

            JoinableTasks.JoinableTaskFactory.Run(() =>
                database.InitializeAsync(_migrations).ContinueWith(task =>
                {
                    //TODO: 这里先跑一下数据库，因为，有时候，IdBarrier会莫名其妙的第一次读不到数据
                }, TaskScheduler.Default));

            //TODO: 使用ApiClient获取一些初始化参数，或者私密信息
        }
    }
}