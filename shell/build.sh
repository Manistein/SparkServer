#!/bin/bash

# /***
# * 编译、清理脚本
# * @author manistein
# * @since 2019-04-19
# */

parentPath=$(dirname $(pwd))

function build() {
	# 编译spark-server
	echo "====================="
	echo "start build spark-server"
	cd $parentPath/spark-server
	MONO_IOMAP=case msbuild SparkServer.sln

	cd $parentPath/spark-server/server/bin/Debug
	chmod 755 *
}

function clean() {
	# 清理spark-server
	echo "====================="
	echo "start clean spark-server"
	cd $parentPath/spark-server/server/bin
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
