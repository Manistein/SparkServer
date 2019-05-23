#!/bin/bash

# boot SendSkynetRequest Server

parentPath=$(dirname $(pwd))

cd $parentPath/../TestDependency/shell/

./test.sh start ../SendSkynetRequest/SkynetReceiver
