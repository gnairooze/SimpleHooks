FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "SimpleHooks.Server/SimpleHooks.Server.csproj"
RUN dotnet build "SimpleHooks.Server/SimpleHooks.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleHooks.Server/SimpleHooks.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY SimpleHooks.Server/simple-hooks-task.sh .
RUN chmod +x simple-hooks-task.sh .

ENTRYPOINT ["./simple-hooks-task.sh"]