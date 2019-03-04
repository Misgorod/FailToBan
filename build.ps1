dotnet clean ./FailToBan.Server/FailToBan.Server.csproj --configuration Debug --framework netcoreapp2.1 --output ./Release/Server
Remove-Item -path ./Release/ -recurse
dotnet build ./FailToBan.Server/FailToBan.Server.csproj --force --configuration Debug --framework netcoreapp2.1 --output ./Release/Server

dotnet clean ./FailToBan.Client/FailToBan.Client.csproj --configuration Debug --framework netcoreapp2.1 --output ./Release/Client
Remove-Item -path ./Release/ -recurse
dotnet build ./FailToBan.Client/FailToBan.Client.csproj --force --configuration Debug --framework netcoreapp2.1 --output ./Release/Client

docker build  -t vicfail2ban -f ./Dockerfile .