/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;

namespace HB.FullStack.Client.Base
{
    public interface IClientModelSettingFactory
    {
        ClientModelSetting? Get<T>() where T : IModel;

        void Register<T>(ClientModelSetting def) where T : IModel;
    }
}