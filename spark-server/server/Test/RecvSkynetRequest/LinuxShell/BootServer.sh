#!/bin/bash

# boot RecvSkynetRequest Client

parentPath=$(dirname $(pwd))

cd $parentPath/../../bin/Debug/

nohup mono spark-server.exe TestCases RecvSkynetRequest &
