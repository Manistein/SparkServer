#!/bin/bash

# /***
# * 编译、清理脚本
# * @author manistein 
# * @since 2019-04-19
# */

parentPath=$(dirname $(pwd))

function build() {
	# 编译skynet
	echo "====================="
	echo "start build skyent..."
	cd $parentPath/skynet
	make linux

	# 编译log日志服务
	echo "====================="
	echo "start build service log..."
	cd $parentPath/3rd/src/service-log
	make

	# 编译cryptex
	echo "====================="
	echo "start build cryptex..."
	cd $parentPath/3rd/src/cryptex
	make

	# 编译battle-server
	echo "====================="
	echo "start build battle-server"
	cd $parentPath/game/battle-server
	MONO_IOMAP=case msbuild battle-server.sln
}

function clean() {
	# 清理skynet
	echo "====================="
	echo "start clean skyent..."
	cd $parentPath/skynet
	make clean

	# 清理log日志服务
	echo "====================="
	echo "start clean service log..."
	cd $parentPath/3rd/src/service-log
	make clean

	# 清理cryptex
	echo "====================="
	echo "start clean cryptex..."
	cd $parentPath/3rd/src/cryptex
	make clean

	# 清理battle-server
	echo "====================="
	echo "start clean battle-server"
	cd $parentPath/game/battle-server/battle-server/bin
	rm -rf * 
}

if [[ "$1" == "all" ]]; then
	build
elif [[ "$1" == "clean" ]]; then
	clean
elif [[ "$1" == "rebuild" ]]; then
	clean
	build
else
	echo "不存在$1指令"
fi
