FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj"
RUN dotnet build "SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleHooks.AuthApi/SimpleHooks.AuthApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleHooks.AuthApi.dll"] 