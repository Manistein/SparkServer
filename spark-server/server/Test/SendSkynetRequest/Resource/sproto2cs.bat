@REM author:Manistein
@REM since:2019.04.26
@REM desc:dump sproto 2 cs

set RPCProtoCS=%~dp0RPCProtoCS\\
set RPCProtoSchema=%~dp0RPCProtoSchema\\

set WorkingPath=..\\..\\..\\Framework\\Tools\\sproto2cs\\lib\\
pushd %WorkingPath%

lua.exe ..\\sproto2cs.lua %RPCProtoSchema% %RPCProtoCS%

popd

del %RPCProtoCS%NetProtocol.cs

pause