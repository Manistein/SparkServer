# Overview
这是一个简单的C#服务端框架，用于接收skynet进程发出的请求，完成处理后返回结果给skynet节点。该框架功能包括：

* 基于Proactor模型的网络库
* 消息调度机制，可以自定义服务处理逻辑
* 日志服务

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

# 启动Example或战斗服
```
# 网络库服务器使用范例
./battlesvr.sh start TCPServerExample

# 网络库客户端使用范例
./battlesvr.sh start TCPClientExample

# 战斗服
./battlesvr.sh start BattleServer
```

# 关闭Example或战斗服
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
