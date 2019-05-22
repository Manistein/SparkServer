#!/bin/bash

# boot gate server

parentPath=$(dirname $(pwd))

cd $parentPath/../../bin/Debug/

mono spark-server.exe TestCases GatewayClientCase