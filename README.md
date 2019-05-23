# Overview
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;SparkServer是一个基于Actor模型的服务端框架，使用了微软的.Net Framework。SparkServer最初的目标，是服务端能够和Unity客户端共享一些逻辑代码，进而节约开发成本，同时能够无缝整合到[skynet](https://github.com/cloudwu/skynet)的集群机制中。SparkServer深度参照了skynet的设计，如果你熟悉skynet，那么同样可以很快理解SparkServer的设计机制。目前为止，SparkServer已经可以做到和skynet节点联合组网，SparkServer采用了skynet的集群机制的设计和实现，只需要遵循skynet cluster机制的使用方式，即可向SparkServer节点发起RPC请求。同时我们也可以很方便地从SparKServer节点向skynet节点发起RPC请求。SparkServer不仅可以无缝整合到skynet的cluster机制中，也可以独自组网，构建只有SparkServer节点的集群。  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;SparkServer可以在Windows上运行，同时也可以借助Mono在Linux平台上运行。如果你有兴趣进一步了解SparkServer的设计背景，和一些内部设计机制，可以参考这篇文章[C#服务端框架设计与实现](https://manistein.github.io/blog/post/server/csharp/csharp%E6%9C%8D%E5%8A%A1%E7%AB%AF%E6%A1%86%E6%9E%B6%E8%AE%BE%E8%AE%A1%E4%B8%8E%E5%AE%9E%E7%8E%B0/)

# 运行的目标环境
* Linux
* Windows
* MacOS

# 编译环境安装
#### Windows平台
* Visual Studio 2017或以上版本
#### Linux平台
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
指令来将文件转成unix文件格式。如果你想清理skynet的编译文件，可以执行./build.sh clean指令进行清理。

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