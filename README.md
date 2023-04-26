# Overview
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;SparkServer是一个基于Actor模型的服务端框架，使用了微软的.Net Framework。SparkServer最初的目标，是服务端能够和Unity客户端共享一些逻辑代码，进而节约开发成本，同时能够无缝整合到[skynet](https://github.com/cloudwu/skynet)的集群机制中。SparkServer深度参照了skynet的设计，如果你熟悉skynet，那么同样可以很快理解SparkServer的设计机制。目前为止，SparkServer已经可以做到和skynet节点联合组网，SparkServer采用了skynet的集群机制的设计和实现，只需要遵循skynet cluster机制的使用方式，即可向SparkServer节点发起RPC请求。同时我们也可以很方便地从SparKServer节点向skynet节点发起RPC请求。SparkServer不仅可以无缝整合到skynet的cluster机制中，也可以独自组网，构建只有SparkServer节点的集群。  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;SparkServer可以在Windows上运行，同时也可以借助Mono在Linux平台上运行。如果你有兴趣进一步了解SparkServer的设计背景，和一些内部设计机制，可以参考这篇文章[C#服务端框架设计与实现](http://manistein.club/post/server/csharp/csharp%E6%9C%8D%E5%8A%A1%E7%AB%AF%E6%A1%86%E6%9E%B6%E8%AE%BE%E8%AE%A1%E4%B8%8E%E5%AE%9E%E7%8E%B0/)

# 运行的目标环境
* Linux
* Windows
* MacOS

# 编译环境安装
#### Windows平台
* Visual Studio 2017或以上版本
#### Linux平台
* 首先在对应的平台，先安装msbuild(重要，一定要先安装msbuild，否则编译会出现问题，得卸载下一个步骤里所有的安装包后，再重新安装)
* [安装Mono开发运行环境(version 5.18.1.0或以上)](https://www.mono-project.com/download/stable/#download-lin)
* 安装dos2unix工具

# 获取仓库
```
git clone https://github.com/Manistein/SparkServer.git
```

# 编译工程
#### Windows平台
用Visual Studio打开spark-server/SparkServer.sln工程，[Build] -> [Rebuild Solution]

#### Linux平台
msbuild SparkServer.sln

# 运行Test Case
测试用例全部放置在spark-server/server/Test目录下，由于skynet官方版本不支持在windows上运行，因此和skynet进行交互测试的RecvSkynetRequest和SendSkynetRequest两个测试用例需要在Linux或Mac环境下运行。其他测试用例可以在Windows上、Linux和Mac上运行。目前测试用例包括：  

* Gateway：实现了测试客户端，用来模拟客户端和服务器通信，测试数据包收发的情况
* RPC：测试SparkServer集群的RPC机制，包含RPC调用测试
* RecvSkynetRequest：接收来自skynet节点的请求，并回应的例子
* SendSkynetRequest：向skynet节点发送请求，并接收回应的例子
* TestDependency：测试用到的依赖工程

#### Windows平台
能在Windows上运行的用例，均在测试用例的目录中拥有一个WinCmd目录，打开WinCmd目录后，先运行BootServer.bat脚本启动服务器，再运行BootClient.bat脚本启动客户端。每个测试用例的目录下均有一个log目录。如果你希望在Visual Studio上运行测试用例，需要执行如下操作：  

* 用Visual Studio打开SparkServer.sln工程
* 在Solution Explorer中右键SparkServer工程，选择Properties
* 选择Debug标签，在Command line arguments中，填入启动脚本中的参数即可

#### Linux平台
要在Linux上运行所有的测试用例，需要预先安装一些工具，包括：  

* gcc
* make
* cmake
* python
* autoconf
* libreadline7 libreadline-dev
* zip

如果你使用的是Ubuntu系统，可以直接使用TestDependency/shell/目录下的installenv.sh脚本，一键安装环境和工具。在完成依赖项安装以后，需要依次执行TestDependency/shell目录下的这些脚本：  

* installjemalloc.sh
* build.sh all

先安装jemalloc，再对skynet进行编译。所有的测试用例，均在测试目录下有一个LinuxShell目录，要启动对应的测试用例，先运行BootServer.sh脚本，再执行BootClient.sh脚本。如果遇到shell脚本因为文件格式不正确无法执行的情况，可以安装dos2unix工具，在LinuxShell目录下执行
```
dos2unix *
```
指令来将文件转成unix文件格式。如果你想清理skynet的编译文件，可以执行./build.sh clean指令进行清理。如果你编译skynet仍然遇到困难，可以参照skynet官方的教程：[Build](https://github.com/cloudwu/skynet/wiki/Build)

# 进程启动配置
启动SparkServer节点，需要指定启动配置，而配置采用的是json格式。我们的SparkServer节点，主要包含如下字段：  

* Gateway字段：启动Gateway服务的配置，负责客户端的连接和数据包的收发处理，包含几个重要的字段
    * Host：包含服务器IP地址和端口信息的字符串，如"127.0.0.1:8888"
    * Class：包含要启动类的命名空间和类名(SparkServer支持用户自定义Gateway类，因此使用者可以写自定义Gateway服务，但是要继承Framework.Service里的Gateway服务)。如要被创建的Gateway类实例的命名空间是"SparkServer.Test.Gateway"，类名是"GatewayCase"，那么这里则是填"SparkServer.Test.Gateway.GatewayCase"
    * Name: 要被启动的Gateway服务的名称
    * 下面是一个Gateway配置的例子，具体可以参考Test Case里的Gateway测试：
    ```
    {
      "Gateway": {
        "Host": "127.0.0.1:8888",
        "Class": "SparkServer.Test.Gateway.GatewayCase",
        "Name": "gateway"
      },
    }
    ```
* Logger字段：日志输出的路径，一般是exe所在的目录作为工作目录
* ClusterConfig字段：用于指定集群配置的路径
* ClusterName字段：用于指定被启动的进程，在集群中的进程名称，在跨进程的RPC调用中发挥关键作用
* ThreadNum字段：指定worker线程会被创建多少条，默认为8条，一般建议worker线程的数量和cpu核心数相当。

# 创建新服务
在SparkServer中，我们将执行业务逻辑的实体，称之为服务。SparkServer提供了一个ServiceContext类作为所有服务的基类，所有用户自定义的服务都需要继承这个类，它为我们提供了消息回调的入口，RPC调用的接口以及定时器设置等基础功能。SparkServer的Game目录，提供了一个Example工程，他包含了我推荐的项目目录组织形式。Framework目录包含了SparkServer的基础框架逻辑，对于基于SparkServer的工程开发，最好的方式是，在工程下与Framework目录同级的目录，新建一个目录，将所有的代码和资源都放置在这里，方便管理，避免侵入框架本身。正如上面所说，Game目录的Example提供了一个范例。  

要新建一个服务，首先要创建一个继承ServiceContext的类，如下所示，我们定义了一个用于启动其他服务的Boot服务：  

```
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Game.Process.TestSender
{
    class Boot : ServiceContext
    {
        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < 100; i ++)
            {
                SparkServerUtility.NewService("SparkServer.Game.Process.TestSender.Sender");
            }
        }
    }
}
```

定义一个服务，有个非常重要的函数便是负责初始化的Init函数，我们的Init初始化函数分为两类，一种是有参，它传入一个byte[]类型的参数。一种是无参初始化函数，也就是说这种初始化不依赖任何外部参数。Init函数需要重载，为了保证线程安全，SparkServer的服务在创建时，会被push一个消息，这个消息在消费时会调用Init函数执行初始化操作，由于创建服务的线程和执行Init初始化逻辑的线程很可能不一样，这种方式避免了服务在未完成初始化的情况下，开始消费其他服务发送过来的消息。  

启动一个定义好的服务也非常简单，我们需要借助SparkServerUtility库的NewService函数去启动服务，比如我们要启动上面定义的Boot服务，那么启动它的逻辑，将是如下所示的代码：  

```
SparkServerUtility.NewService("SparkServer.Game.Process.TestSender.Boot");
```

我们的NewService函数有多个重载版本，他们分别是：  

```
// SparkServer.cs
// @serviceClass:指明第一个要被启动的服务，形式是Namespace.ClassName拼成的字符串
// @serviceName:服务的别名，在RPC调用，用于指明目标服务时使用
// @param:初始化参数，如果一个服务依赖外部参数进行初始化，那么需要在该服务对应的.sproto文件中定义对应的结构，序列化以后，作为param参数传入

public static int NewService(string serviceClass);
public static int NewService(string serviceClass, byte[] param);
public static int NewService(string serviceClass, string serviceName);
public static int NewService(string serviceClass, string serviceName, byte[] param);
```

实际上，进程的第一个启动服务是不需要我们自己去调用这个函数的，我们需要在启动命令中，指定要启动的服务，以及配置路径即可，比如我们要启动Game这个Example中Battle进程下的BattleTaskDispatcher服务，则需要执行如下命令：  

```
# windows
spark-server.exe SparkServer SparkServer.Game.Process.Battle.BattleTaskDispatcher ../../Game/Startconf/LocalSvr/Battle/BootConfig.json BattleTaskDispatcher 

# linux
mono spark-server.exe SparkServer SparkServer.Game.Process.Battle.BattleTaskDispatcher ../../Game/Startconf/LocalSvr/Battle/BootConfig.json BattleTaskDispatcher 
```

spark-server.exe是编译好的可执行文件，后面的SparkServer参数指明了，启动这个进程，需要指明初始化服务、启动配置和服务名称。SparkServer.Game.Process.Battle.BattleTaskDispatcher这个参数，指明了第一个被启动的服务是哪个，紧随其后的就是启动配置，最后是第一个被启动服务的名称。  

# 注册服务函数
我们创建一个服务以后，需要为服务定义回调函数，即接收其他服务发送的请求或通知，因此需要为这些请求或通知定义对应的函数。定义的方式也非常简单，所有要被请求的功能函数，需要遵守固定的函数签名：  

```
delegate void Method(int source, int session, string method, byte[] param);
```

下面是一个战斗任务消费的服务，定义了响应战斗请求的函数，这个函数在服务初始化阶段就完成注册：  

```
namespace SparkServer.Game.Process.Battle
{
    class BattleTaskConsumer : ServiceContext
    {
        protected override void Init()
        {
            base.Init();

            RegisterServiceMethods("OnBattleRequest", OnBattleRequest);
        }

        private void OnBattleRequest(int source, int session, string method, byte[] param)
        {
            BattleTaskConsumer_OnBattleRequest request = new BattleTaskConsumer_OnBattleRequest(param);

            // TODO Logic
            LoggerHelper.Info(m_serviceAddress, request.param);

            BattleTaskConsumer_OnBattleRequestResponse response = new BattleTaskConsumer_OnBattleRequestResponse();
            response.method = "OnBattleRequest";
            response.param = request.param;
            DoResponse(source, response.method, response.encode(), session);
        }
    }
}
```
注册服务函数，我们通过RegisterServiceMethods函数来进行，我们需要对函数取一个别名，方便其他服务发起RPC调用的时候，来指明希望调用的函数是哪个。由于我们所有的请求响应函数，都采用统一函数签名，而不同的函数需要传入的参数又各不相同，因此这里的处理方式是将参数序列化为byte数组param，并且在响应函数中，对param进行反序列化获得最终的结构。我们序列化和反序列化的工具是使用sproto，他要求被序列化和反序列化的结构，需要定义一个schema文件，具体的方式是，我们需要为每一个被创建的服务，创建一个sproto的schema文件，如我们的Game Example中，在Game/Resource/RPCProtoSchema目录下创建一个与服务同名的sproto文件，BattleTaskConsumer.sproto，然后在.sproto文件中，定义对应函数的结构，结构名称为.ClassName_MethodName的方式，如：  

```
.BattleTaskConsumer_OnBattleRequest {
    method 0 : string
    param  1 : string
}

.BattleTaskConsumer_OnBattleRequestResponse {
    method 0 : string
    param  1 : string
}
```
我们可以看到，定义的结构中，需要以‘.’作为起点，然后服务类名和函数名通过"_"来进行拼接，花括号内部就是该函数要使用到的参数名和类型了。在完成定义以后，我们需要通过Resource目录下的sproto2cs.bat工具，将schema文件转化成.cs文件，在将其添加到工程中以后，我们就可以使用它了。上面所示的示例代码展示了如何将参数反序列化的流程，并访问其中的域。  

这里还需要强调的一点则是，每个服务被创建后，接收的第一消息一定是要求调用Init函数的消息，前面我们也提到过，Init函数分为有参和无参两种类型，如果我们一个服务的初始化依赖外部参数，那么就需要为这个服务传入参数，相对应的，我们需要在对应服务的.sproto文件中，定义Init函数要用到的参数格式。现在以我们进行集群通信处理的ClusterClient服务为例，由于ClusterClient服务的初始化依赖外部参数，因此我们需要在ClusterClient.sproto文件中定义Init函数的param参数结构：  

```
.ClusterClient_Init {
    cluster_config 0 : string
    tcp_client_id  1 : integer
}
```

在完成定义和生成以后，我们创建服务实例之前，首先要创建一个ClusterClient_Init的结构，设置好对应的参数，序列化以后，将结果作为NewService的最后一个参数，传入，于是有：  

```
ClusterServer_Init clusterServerInit = new ClusterServer_Init();
clusterServerInit.tcp_server_id = m_clusterTCPServer.GetObjectId();
int clusterServerId = SparkServerUtility.NewService("SparkServer.Framework.Service.ClusterServer.ClusterServer", 
    "clusterServer", 
    clusterServerInit.encode());
```

最后，我们只需在ClusterClient服务的Init函数中，将param反序列化出来，再进行初始化即可：

```
...
protected override void Init(byte[] param)
{
    base.Init();

    ClusterClient_Init init = new ClusterClient_Init(param);
    SetTCPObjectId((int)init.tcp_client_id);
    ParseClusterConfig(init.cluster_config);

    RegisterSocketMethods("SocketConnected", SocketConnected);
    RegisterSocketMethods("SocketError", SocketError);
    RegisterSocketMethods("SocketData", SocketData);

    RegisterServiceMethods("Request", Request);
}
...
```

# 向其他服务发送消息
在完成了服务创建和函数注册以后，现在要阐述的内容是，一个服务如何向另一个服务发出请求，发起RPC请求的方式有两种，一种是不需要对方返回回应，一种需要对方返回回应消息。我们的ServiceContext服务提供了两种接口，他们分别是：  

```
// 向远程服务发出请求，但不需要回应
protected void Send(int destination, string method, byte[] param);
protected void Send(string destination, string method, byte[] param);

// 向远程服务发出请求，并且需要回应
protected void Call(int destination, string method, byte[] param, SSContext context, RPCCallback cb);
protected void Call(string destination, string method, byte[] param, SSContext context, RPCCallback cb);
```

不论是Send还是Call，他们都有两个版本，一个是指明被调用服务的地址，还有一个则是指定服务的名称，我们在调用SparkServerUtility.NewService时，有一个参数是传入服务名称，而这个服务名称就是服务的别名，在RPC请求的时候，如果一个服务有注册别名，那么Send和Call函数可以将这个别名作为第一个参数，将请求发给目标服务。具体的使用方式，可以参照Game/Process/Battle进程中，BattleTaskDispatcher服务如何向BattleTaskDispatcher服务转发消息的例子。

# 向请求服务返回回应包
如果请求服务通过Call函数，对自己发出请求，那么在完成处理以后，需要将结果返回给请求者，ServiceContext类给我们提供了一个DoResponse的函数，它的定义如下所示：  

```
// @ destination:请求者的地址
// @ method:本服务被调用的方法名
// @ param:经过序列化的返回参数
// @ session:请求者的session
protected void DoResponse(int destination, string method, byte[] param, int session);
```

为什么这里，请求者服务需要一个session？因为请求者可能要发出多个Call请求，而每次Call的时候又有自己独立的上下文环境，为了识别哪个返回对应哪个请求，我们需要为每个请求生成唯一的session id，在被请求方处理完请求时，需要将这个session带回，以方便请求者找回对应的上下文环境。具体的使用，可以参照Game/Process/Battle中，BattleTaskComsumer服务如何向BattleTaskDispatcher服务返回回应的例子。

# 远程RPC调用
上一节，我们说到了一个服务向另一个服务发起请求，不过是针对发起请求的服务和被要求请求的服务都在同一个进程的情况下做的，如果我们跨节点发起RPC请求，那么需要借助集群机制来实现。这里SparkServer提供了几个接口，以供我们进行远程RPC调用：  

```
protected void RemoteSend(string remoteNode, string service, string method, byte[] param);
protected void RemoteCall(string remoteNode, string service, string method, byte[] param, SSContext context, RPCCallback cb);
```

我们要使用SparkServer节点进行组网，首先需要配置每个节点需要绑定的IP地址和端口，这些配置，我们将填在BootConfig.json中ClusterName字段所指向文件中，比如我们的集群配置文件为ClusterName.json，我们以Game目录下的Example为例，其配置为：  

```
{
    "battlesvr" : "127.0.0.1:8888",
    "battlecli" : "127.0.0.1:6666"
}
```
在完成集群配置的编辑以后，我们需要在启动配置中，指定集群配置的路径和该进程的名称，这个名称用于指明进程将监听哪个IP的哪个端口。还是以Game的Example为例：  

```
{
    "ClusterConfig" : "../../Game/Startconf/LocalSvr/ClusterName.json",
    "ClusterName" : "battlesvr",
    "Logger" : "../../Game/Logs/Server/"   
}
```
这个配置指定了，要被启动的进程将监听"127.0.0.1:8888"这个地址。在了解集群配置的设定以后，我们就可以来解释RemoteSend和RemoteCall的参数：  

* remoteNode:集群配置中的节点名称，不能填自己绑定的节点
* service:目标服务名称，即对应SparkServerUtility.NewService里的serviceName参数
* method:目标服务注册好的方法名
* param:序列化好的参数
* context:发起rpc调用，并要求回应的时候，需要保存的上下文环境变量，将被存储在这个结构内
* cb:当被请求服务返回回应时，这个函数用于处理回应信息

具体例子可以参见Game目录下的Example，TestSender进程内的服务，如何向Battle进程内的服务发起远程RPC请求，并处理回应的。

# 设置定时器
SparkServer的ServiceContext基类，提供了一个设置定时器的函数Timeout函数，我们来看一下它的定义：  

```
// @context:要保存的上下文环境变量，这些变量用于在定时器触发时使用
// @timeout:多少秒后触发这个定时器
// @callback:定时器回调函数
protected void Timeout(SSContext context, long timeout, TimeoutCallback callback)
```

定时器回调函数，是当定时器触发时，要被调用的函数：  

```
delegate void TimeoutCallback(SSContext context, long currentTime);
```

我们只需要遵循这样的格式即可。具体的例子可以参考Game/Process/TestSender/Sender.cs，Sender这个服务的调用方式。

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=Manistein/SparkServer&type=Date)](https://star-history.com/#Manistein/SparkServer&Date)

