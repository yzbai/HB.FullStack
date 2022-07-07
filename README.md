# HB.FullStack

Not Finished yet.

This project is a complete framework for developer that using .net technologies both in mobile and server.

It can help you maximising the gainings of using many same interface, like Model, Database ORM, etc.

**NuGets**

|Name|Nuget|Info|
| ------------------- | :------------------: |--------------------|
|HB.FullStack.Common| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Common?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Common/)|Des|
|HB.FullStack.Database| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Database?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Database/)|Des|
|HB.FullStack.KVStore| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.KVStore?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.KVStore/)|Des|
|HB.FullStack.Cache| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Cache?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Cache/)|Des|
|HB.FullStack.Lock| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Lock?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Lock/)|Des|
|HB.FullStack.EventBus| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.EventBus?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.EventBus/)|Des|
|HB.FullStack.Repository| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Repository?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Repository/)|Des|
|HB.FullStack.Identity| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Identity?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Identity/)|Des|
|HB.FullStack.Server| [![Nuget](https://img.shields.io/nuget/v/HB.FullStack.Server?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.FullStack.Server/)|Des|
|HB.Infrastructure.MySQL| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.MySQL?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.MySQL/)|Des|
|HB.Infrastructure.SQLite| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.IdGen?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.SQLite/)|Des|
|HB.Infrastructure.Redis.KVStore| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.Redis.KVStore?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.Redis.KVStore/)|Des|
|HB.Infrastructure.Redis.Cache| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.Redis.Cache?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.Redis.Cache/)|Des|
|HB.Infrastructure.Redis.DistributedLock| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.Redis.DistributedLock?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.Redis.DistributedLock/)|Des|
|HB.Infrastructure.Redis.EventBus| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.Redis.EventBus?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.Redis.EventBus/)|Des|
|HB.Infrastructure.IdGen| [![Nuget](https://img.shields.io/nuget/v/HB.Infrastructure.IdGen?style=flat-square&logo=nuget)](https://www.nuget.org/packages/HB.Infrastructure.IdGen/)|Des|



## Model & ApiResource
## ApiClient
## Database ORM  
## KVStore  
## Caching
## Distributed Lock
## EventBus
## Repository Pattern
## Xamarin.Forms Toolkit
## Many others

Model - Repo ：一对一关系，Repo中处理Cache问题，事件问题，对外提供好像内存操作的对象，隐藏数据存储设施的细节。
Service : 使用多个Repo, 完成Model 到 复杂Model和Resource 的转换

Api设计：
1，url设计 https://[endpoint]/[version]/[resource]/[condition]?RandomStr=[randomStr]&Timestamp=[timestamp]&DeviceId=[deviceId]
2, 所有的参数，都放在body中，以json方式。包括Get