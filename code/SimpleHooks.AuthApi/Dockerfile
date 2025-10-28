FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "code/SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj"
RUN dotnet build "code/SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-anonymous
WORKDIR /src
COPY . .
RUN dotnet restore "code/listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj"
RUN dotnet build "code/listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-typea
WORKDIR /src
COPY . .
RUN dotnet restore "code/listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj"
RUN dotnet build "code/listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj" -c Release -o /app/build

FROM build-anonymous AS publish-anonymous
RUN dotnet publish "code/listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj" -c Release -o /app/publish

FROM build-typea AS publish-typea
RUN dotnet publish "code/listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj" -c Release -o /app/publish

FROM build AS publish
RUN dotnet publish "code/SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p ./listener-plugins/anonymous
COPY --from=publish-anonymous /app/publish ./listener-plugins/anonymous
RUN mkdir -p ./listener-plugins/typea
COPY --from=publish-typea /app/publish ./listener-plugins/typea
ENTRYPOINT ["dotnet", "SimpleHooks.AuthApi.dll"]
