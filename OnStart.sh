#!/bin/bash

_term() { 
  dotnet /_Data/CLI/FailToBan.Client.dll StopContainer
  wait "$child"
}

trap _term SIGTERM

dotnet /_Data/CLI/Server/FailToBan.Server.dll "$*" & child=$!
wait "$child"
