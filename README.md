# Overview
这是一个简单的C#服务端框架，用于接收skynet进程发出的请求，完成处理后返回结果给skynet节点。该框架功能包括：

* 基于Proactor模型的网络库
* 消息调度机制，可以自定义服务处理逻辑（对skynet节点的request进行解码，并将response编码后返回给skynet节点）
* 日志服务

# 架构文档
[C#服务端框架设计与实现](https://manistein.github.io/blog/post/server/csharp/csharp%E6%9C%8D%E5%8A%A1%E7%AB%AF%E6%A1%86%E6%9E%B6%E8%AE%BE%E8%AE%A1%E4%B8%8E%E5%AE%9E%E7%8E%B0/)

# 测试通过的环境
* Ubuntu18.04
* Win7
* Win10

# 需要预先安装的环境
* [安装Mono开发运行环境](https://www.mono-project.com/download/stable/#download-lin)
* 安装dos2unix工具
```
# install dos2unix on ubuntu
sudo apt-get install dos2unix
```

# 获取仓库
```
# clone to ubuntu
sudo git clone git@github.com:Manistein/CSharpServerFramework.git
```

# Linux Shell
* 进入shell目录
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
```

# 启动服务器
```
./battlesvr.sh start BattleServer
```

# 关闭服务器
```
./battlesvr.sh stop
```

# 启动客户端
```
# 启动skynet测试客户端
./test start battle-client
```

# 关闭客户端
```
./test stop battle-client
```

# 开启网络库Example
进入battle-server\battle-server\bin\Debug\目录，执行如下命令
```
# 开启网络库服务端使用Example
mono battle-server.exe TCPServerExample

# 开启网络库客户端使用Example
mono battle-server.exe TCPClientExample
```
