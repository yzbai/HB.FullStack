/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

namespace HB.FullStack.Client.Base
{
    public enum ExpiryTimeType
    {
        Always = 0,
        Tiny = 1,
        Short = 2,
        Medium = 3,
        Long = 4,
        NonExpiry = 5
    }
}

/*
//NOTICE: 几个过期时间
1. ClientModels 过期时间， 放在ClientModelDef里
2. StsToken 过期时间， 存储在StsToken里
3，DirectoryDescription过期时间，由客户端自己决定
*/