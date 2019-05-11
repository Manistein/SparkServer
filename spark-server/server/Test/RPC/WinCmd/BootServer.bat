@REM boot gate server

set bin_path=../../../bin/Debug/

cd %bin_path%
spark-server.exe TestCases RPCTestServer

pause