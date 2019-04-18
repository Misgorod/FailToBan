#--no-cache
dotnet clean ./FailToBan.Server/FailToBan.Server.csproj --configuration Debug --framework netcoreapp2.1 --output ../Bin/Server
dotnet clean ./FailToBan.Client/FailToBan.Client.csproj --configuration Debug --framework netcoreapp2.1 --output ../Bin/Client

Remove-Item -path ./Bin/* -recurse

dotnet publish ./FailToBan.Server/FailToBan.Server.csproj --force --configuration Debug --framework netcoreapp2.1 --output ../Bin/Server
dotnet publish ./FailToBan.Client/FailToBan.Client.csproj --force --configuration Debug --framework netcoreapp2.1 --output ../Bin/Client

docker build  -t vicfail2ban -f ./Dockerfile .