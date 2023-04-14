#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/mssql/server:2022-latest AS base
USER root
RUN apt-get update \
 && apt-get -y upgrade \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
USER mssql

#FROM mcr.microsoft.com/dotnet/sdk:latest AS build
#WORKDIR /src
#COPY ["Catalyst/Catalyst.csproj", "Catalyst/"]
#RUN dotnet restore "Catalyst/Catalyst.csproj"
#COPY . .
#WORKDIR "/src/Catalyst"
#RUN dotnet build "Catalyst.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Catalyst.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Catalyst.dll"]
