#!/bin/bash

# /***
# * 编译、清理脚本
# * @author manistein
# * @since 2019-04-19
# */

parentPath=$(dirname $(pwd))

function try_create_folder() {
	if [ ! -d $parentPath/$1 ]; then
		mkdir -p $parentPath/$1
	fi
}

function build() {
	# 编译skynet
	echo "====================="
	echo "start build skyent..."
	cd $parentPath/skynet
	make linux

	try_create_folder 3rd/clib/loggerx
	# 编译log日志服务
	echo "====================="
	echo "start build service log..."
	cd $parentPath/3rd/src/service-log
	make

	try_create_folder 3rd/clib/cryptex
	# 编译cryptex
	echo "====================="
	echo "start build cryptex..."
	cd $parentPath/3rd/src/cryptex
	make
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
