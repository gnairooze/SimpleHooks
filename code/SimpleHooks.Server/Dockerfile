FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "SimpleHooks.Server/SimpleHooks.Server.csproj"
RUN dotnet build "SimpleHooks.Server/SimpleHooks.Server.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-anonymous
WORKDIR /src
COPY . .
RUN dotnet restore "listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj"
RUN dotnet build "listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-typea
WORKDIR /src
COPY . .
RUN dotnet restore "listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj"
RUN dotnet build "listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj" -c Release -o /app/build

FROM build-anonymous AS publish-anonymous
RUN dotnet publish "listener-plugins/SimpleHooks.ListenerPlugins.Anonymous/SimpleHooks.ListenerPlugins.Anonymous.csproj" -c Release -o /app/publish

FROM build-typea AS publish-typea
RUN dotnet publish "listener-plugins/SimpleHooks.ListenerPlugins.TypeA/SimpleHooks.ListenerPlugins.TypeA.csproj" -c Release -o /app/publish

FROM build AS publish
RUN dotnet publish "SimpleHooks.Server/SimpleHooks.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY SimpleHooks.Server/simple-hooks-task.sh .
RUN chmod +x simple-hooks-task.sh .
RUN mkdir -p ./listener-plugins/anonymous
COPY --from=publish-anonymous /app/publish ./listener-plugins/anonymous
RUN mkdir -p ./listener-plugins/typea
COPY --from=publish-typea /app/publish ./listener-plugins/typea
ENTRYPOINT ["./simple-hooks-task.sh"]
