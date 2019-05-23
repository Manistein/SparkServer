#!/bin/bash

# boot RecvSkynetRequest Server

parentPath=$(dirname $(pwd))

cd $parentPath/../TestDependency/shell/

./test.sh start ../RecvSkynetRequest/SkynetSender
