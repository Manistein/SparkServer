#!/bin/bash

# /***
# * 安装依赖环境
# * @author manistein
# * @since 2019-04-19
# */

parentPath=$(dirname $(pwd))

# install gcc
sudo apt-get install gcc

# install make
sudo apt-get install make

# install cmake
sudo apt-get install cmake

# install python3
sudo apt-get install python

# install python27
sudo apt install python2.7 python-pip

# install net-tools
sudo apt-get install net-tools

# 安装autoconf
sudo apt-get install autoconf

# 安装readline
sudo apt-get install libreadline7 libreadline-dev

# 安装zip
sudo apt-get install zip
