# syntax=docker/dockerfile:1
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY BrightChain.API/bin/Release/net6.0/BrightChain.API.dll /app
WORKDIR /app
ENTRYPOINT ["dotnet", "BrightChain.API.dll"]
