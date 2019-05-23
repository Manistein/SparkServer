#!/bin/bash

# boot rpc clinet

parentPath=$(dirname $(pwd))

cd $parentPath/../../bin/Debug/

mono spark-server.exe TestCases RPCTestClient
