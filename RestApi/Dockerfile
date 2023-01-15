FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RestApi/RestApi.csproj", "RestApi/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["ForwardingApi/ForwardingApi.csproj", "ForwardingApi/"]
COPY ["Forwarders/TelegramForwarder/TelegramForwarder.csproj", "TelegramForwarder/"]
COPY ["DataLayer/DataLayer.csproj", "DataLayer/"]
COPY ["SmtpReceiverServer/SmtpReceiverServer.csproj", "SmtpReceiverServer/"]
RUN dotnet restore "RestApi/RestApi.csproj"
COPY . .
WORKDIR "/src/RestApi"
RUN dotnet build "RestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir "./forwarder"
ENTRYPOINT ["dotnet", "RestApi.dll"]
