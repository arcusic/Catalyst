#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/mssql/server:2022-latest AS base
USER root
ENV DEBIAN_FRONTEND noninteractive
RUN apt-get update
RUN apt-get -y upgrade
RUN apt-get clean
RUN rm -rf /var/lib/apt/lists/*
COPY ./Catalyst/Redistributables/SQL/entrypoint.sh /entrypoint.sh
RUN mkdir /mssql
RUN chown mssql:mssql /entrypoint.sh
RUN chown mssql:mssql /mssql
USER mssql
RUN chmod +x /entrypoint.sh
CMD ["/entrypoint.sh"]
