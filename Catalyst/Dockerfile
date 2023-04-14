#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/runtime:latest AS base
WORKDIR /app
RUN apt-get update \
 && apt-get -y upgrade \
 && apt-get -y install curl iputils-ping \
 && curl -s https://packagecloud.io/install/repositories/ookla/speedtest-cli/script.deb.sh | bash \
 && apt-get -y install speedtest
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
RUN mkdir /root/.config/
RUN mkdir /root/.config/ookla/
RUN echo '{"Settings": {"LicenseAccepted": "604ec27f828456331ebf441826292c49276bd3c1bee1a2f65a6452f505c4061c"}}' > /root/.config/ookla/speedtest-cli.json

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
USER root
RUN apt-get update \
 && apt-get -y upgrade \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY ["Catalyst/Catalyst.csproj", "Catalyst/"]
RUN dotnet restore "Catalyst/Catalyst.csproj"
COPY . .
WORKDIR "/src/Catalyst"
RUN dotnet build "Catalyst.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalyst.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalyst.dll"]
