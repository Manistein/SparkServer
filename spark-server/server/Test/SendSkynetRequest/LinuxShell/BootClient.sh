#!/bin/bash

# boot SendSkynetRequest Client

parentPath=$(dirname $(pwd))

cd $parentPath/../../bin/Debug/
echo $parentPath/../../bin/Debug/

nohup mono spark-server.exe TestCases SendSkynetRequest &
