#!/bin/bash

# boot gate server

parentPath=$(dirname $(pwd))
cd $parentPath/../../bin/Debug/
nohup mono spark-server.exe TestCases RPCTestServer &