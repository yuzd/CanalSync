# CanalSync
canal 是阿里巴巴开源的一款基于数据库增量日志解析,提供增量数据订阅&消费,目前主要支持了MySQL(也支持mariaDB)。
我开发的这个CanalSync项目是基于canal-server之上的数据库同步&消费中间件，用于可快速搭建消费canal-server的项目。
目前我已实现并开源了如下：

1. 数据消费传输到redis组件
2. 数据消费传输到rabbitmq组件
3. 数据消费传输到mysql数据库组件

# 示意图

![image](https://images4.c-ctrip.com/target/zb0215000000y2ok436F1.png)

## Nuget：
### 1. 接收canal-server的消息中间件：
Install-Package Canal.Server 

### 2. 解析canal-server消息转出可执行sql的中间件：
Install-Package Canal.SqlParse


## 如何使用
如果你需要写一个数据消费传输到XXXMQ，用不到反解析成sql的话，只需要引用 Canal.Server中间件。
如果你需要写一个数据消费传输到XXXdb，得用到反解析sql中间件，需要同时引用Canal.Server 和 Canal.SqlParse 这2个中间件。

## Canal.Server 如何使用
1. 引用 Canal.Server 并appsettings.json 配置canal-server的参数.如下图：

![image](https://images4.c-ctrip.com/target/zb0715000000ycatg5F47.png)

参数说明：
![image](https://images4.c-ctrip.com/target/zb0t15000000xvnx4D8F2.png)

2. 创建一个 消费类 必须要 实现： INotificationHandler<CanalBody> 接口，例如叫TestHandler
```
 public class TestHandler:INotificationHandler<CanalBody>{
        public Task Handle(CanalBody notification)
        {
            //写消费逻辑

            return Task.CompletedTask;;
        }
    }
    
```
3. 在startUp 使用并注册 该消费类
```
      //注册了之后 canal-server有新的消息就会进入到TestHandler的Handle方法
      services.AddCanalService(produce => produce.RegisterSingleton<TestHandler>());
```

## Canal.SqlParse 如何使用
目前只实现了解析mysql的逻辑，未来会加入sqlserver的解析逻辑!!

```
      //注册使用 connectionString是mysql的数据库连接字符串
      services.AddMysqlParseService(connectionString);
      
      // 计划中还未实现
      //services.AddSqlserverParseService(connectionString);
```
在类的构造方法可注入：

![image](https://images4.c-ctrip.com/target/zb0d15000000xu5pq07B3.png)

如上图，代表将canal-server的数据直接在另外的mysql库里面执行，等于2个mysql数据进行互相同步。

## 欧洲与中国的2个mysql库 使用上述方法进行同步的测试
**结果： 同步速度在100~200qps**
![image](https://images4.c-ctrip.com/target/zb0u15000000xvmizF51C.png)
