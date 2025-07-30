FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ✅ Copy everything at once (no partial restore hack — safer for preview builds)
COPY . .

# Restore & build
RUN dotnet restore ./src/AdminService/AdminService.csproj
RUN dotnet build ./src/AdminService/AdminService.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./src/AdminService/AdminService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AdminService.dll"]
