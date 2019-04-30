#!/bin/bash

# /***
# * 安装jemalloc
# * @author manistein 
# * @since 2019-04-19
# */

parentPath=$(dirname $(pwd))

# 重新下载jemalloc，并完成部署
cd $parentPath/skynet/3rd
rm -rf jemalloc

# skynet-1.2 use jemalloc 5.1
wget https://github.com/jemalloc/jemalloc/archive/5.1.0.zip
unzip -d $parentPath/skynet/3rd/ 5.1.0.zip
mv -f $parentPath/skynet/3rd/jemalloc-5.1.0 $parentPath/skynet/3rd/jemalloc
rm 5.1.0.zip
