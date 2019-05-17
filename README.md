# Overview
SparkServer是参考skynet设计的一个服务器框架，使用的是.net平台，可以在Linux、Windows、MacOS上运行。该框架功能包括：

* 有完整的RPC机制
* 基于Proactor模型的网络库
* 消息调度机制，可以自定义服务处理逻辑（对skynet节点的request进行解码，并将response编码后返回给skynet节点）
* 日志服务

# 架构文档
[C#服务端框架设计与实现](https://manistein.github.io/blog/post/server/csharp/csharp%E6%9C%8D%E5%8A%A1%E7%AB%AF%E6%A1%86%E6%9E%B6%E8%AE%BE%E8%AE%A1%E4%B8%8E%E5%AE%9E%E7%8E%B0/)

# 测试通过的环境
* Ubuntu18.04
* Win7
* Win10

# 需要预先安装的环境(以下安装示例是基于Ubuntu环境)
* [安装Mono开发运行环境](https://www.mono-project.com/download/stable/#download-lin)
* 安装dos2unix工具
```
# install dos2unix on ubuntu
sudo apt-get install dos2unix
```

# 获取仓库
```
# clone to ubuntu
sudo git clone https://github.com/Manistein/SparkServer.git
```

#编译skynet服务器


# Linux Shell
* 进入shell目录，shell目录在SparkServer/spark-server/server/Test/TestDependency/shell/
* 将shell文件的格式转换成linux文件格式
```
dos2unix *
```
* 添加可执行权限
```
chmod 755 *
```

# 运行初始化脚本
```
./installenv.sh
```

# 安装jemalloc
```
./installjemalloc.sh
```

# 编译和清理编译
```
# build all
./build.sh all

# build clean
./build.sh clean

* 编译SparkServer服务器
* 进入C#项目文件夹，路径SparkServer/spark-server/
* msbuild SparkServer.sln
* 以运行SparkServer/spark-server/server/Test/SendSkynetRequest为demo，demo内容是SparkServer向skynet发起完整的RPC请求
* 修改配置文件，修改成自己的ip地址
* 配置文件路径SparkServer/spark-server/server/Test/SendSkynetRequest/Resource/Config/ClusterName.json
```

# 进入shell目录下启动skynet测试节点
```
./test.sh start ../SendSkynetRequest/SkynetReceiver
```

# 关闭skynet节点
```
./test.sh stop skynet
```

# 启动服务器
* 进入sparkserver的bin目录，路径SparkServer/spark-server/server/bin/Debug
```
nohup mono spark-server.exe TestCases SendSkynetRequest &
```

# 关闭服务器
```
kill -9 `ps aux | grep "mono" | grep -v tail | grep -v grep | awk '{print $2}'`
```

# 配置文件说明
/*
{
    "ClusterConfig": "../../Test/SendSkynetRequest/Resource/Config/ClusterName.json",
    "ClusterName": "testclient",
    "ThreadNum": 4,
    "Logger" : "../../Game/Logs/Client/"
}
*/
* ClusterConfig包含所有的cluster节点的IP和port
* ClusterName是当前cluster节点的名称
* ThreadNum是工作线程的数量
* Logger是日志存放的路径

# SparkServer启动参数说明
```
* 第一个参数是启动模式，分别是有参初始化和无参初始化

* 有参初始化(第一个参数是SparkServer)，第二个参数是启动的类名，第三个参数是启动时加载的配置文件路径
nohup mono spark-server.exe SparkServer SparkServer.Game.Process.TestSender.Sender ../../Game/Startconf/LocalSvr/TestSender/BootConfig &

* 无参初始化(第一个参数是TestCases)，第二个参数是注册过的测试demo名字，在SparkServer/spark-server/server/Test/TestCases.cs中注册
* 该模式下启动类名和配置文件路径是硬编码模式，如下
/*
private void TestRecvSkynetRequest()
{
    BootServices boot = delegate ()
    {
        SparkServerUtility.NewService("SparkServer.Test.RecvSkynetRequest.SkynetMessageReceiver", "RecvSkynetSend");
    };
    Server server = new Server();
    server.Run("../../Test/RecvSkynetRequest/Resource/Config/Startup.json", boot);
}
*/

nohup mono spark-server.exe TestCases SendSkynetRequest &
```

# 注册服务函数
* public static int NewService(string serviceClass, string serviceName)
* 例SparkServerUtility.NewService("SparkServer.Test.SendSkynetRequest.SkynetMessageSender", "SendSkynetMsg");
* NewService函数的第二个参数是给第一个参数的类注册服务名

* 在serviceClass类中调用protected void RegisterServiceMethods(string methodName, Method method)
* 给当前服务注册服务函数
* 例RegisterServiceMethods("OnProcessRequest", OnProcessRequest);

# 向已注册的服务发送消息
* protected void Send(string destination, string method, byte[] param)
* 在serviceClass类中: 
/*
byte[] bytes = new byte[128];
Send("SendSkynetMsg", "OnProcessRequest", bytes);
*/

# 向请求服务返回回应包
/*
private void OnProcessRequest(int source, int session, string method, byte[] param)
{
    SkynetMessageReceiver_OnProcessRequest request = new SkynetMessageReceiver_OnProcessRequest(param);
    LoggerHelper.Info(m_serviceAddress, string.Format("skynet request_count:{0}", request.request_text));

    if (session > 0)
    {
        SkynetMessageReceiver_OnProcessRequestResponse response = new SkynetMessageReceiver_OnProcessRequestResponse();
        response.request_count = request.request_count;
        response.request_text = request.request_text;

        DoResponse(source, method, response.encode(), session);
    }
}
*/

# 远程RPC调用
* protected void RemoteCall(string remoteNode, string service, string method, byte[] param, SSContext context, RPCCallback cb);
/*
SkynetMessageSender_OnProcessRequest request = new SkynetMessageSender_OnProcessRequest();
request.request_count = 123456;
request.request_text = "hello skynet";
RemoteCall("testserver", ".test_send_skynet", "send_skynet", request.encode(), null, OnProcessResponse);
*/

# 设置定时器
/*
SSContext sSContext = new SSContext();
this.Timeout(sSContext, 10, TimeoutCallback);
*/

# 启动Gateway
* Gateway也是TestCase中的一个demo
nohup mono spark-server.exe TestCases GatewayCase &
