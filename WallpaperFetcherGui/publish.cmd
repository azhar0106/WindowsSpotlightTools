dotnet publish ^
-c Release ^
-r win-x64 ^
--self-contained false ^
/p:PublishSingleFile=false ^
/p:PublishTrimmed=false ^
/p:PublishReadyToRun=false
