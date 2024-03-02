#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/mssql/server:2022-latest AS base
USER root
ENV DEBIAN_FRONTEND noninteractive
RUN apt-get update \
 && apt-get -y upgrade
RUN apt-get clean \
 && rm -rf /var/lib/apt/lists/*
COPY ./Catalyst/Redistributables/SQL/secrets/entrypoint.sh /entrypoint.sh
RUN mkdir -p /mssql/backup
RUN mkdir -p /mssql/data
RUN mkdir -p /mssql/log
RUN mkdir -p /mssql/dump
RUN chown -R mssql:mssql /mssql/backup
RUN chown -R mssql:mssql /mssql/data
RUN chown -R mssql:mssql /mssql/log
RUN chown -R mssql:mssql /mssql/dump
RUN chown mssql:mssql /entrypoint.sh
USER mssql
RUN chmod +x /entrypoint.sh
CMD ["/entrypoint.sh"]
