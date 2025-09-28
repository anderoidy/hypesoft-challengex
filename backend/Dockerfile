FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar apenas os arquivos .csproj primeiro
COPY ["src/Hypesoft.API/Hypesoft.API.csproj", "src/Hypesoft.API/"]
COPY ["src/Hypesoft.Application/Hypesoft.Application.csproj", "src/Hypesoft.Application/"]
COPY ["src/Hypesoft.Domain/Hypesoft.Domain.csproj", "src/Hypesoft.Domain/"]
COPY ["src/Hypesoft.Infrastructure/Hypesoft.Infrastructure.csproj", "src/Hypesoft.Infrastructure/"]

# Restore com configurações mais robustas
RUN dotnet restore "src/Hypesoft.API/Hypesoft.API.csproj" \
    --disable-parallel \
    --verbosity minimal \
    --runtime linux-x64

# Copiar o resto dos arquivos
COPY . .
WORKDIR "/src/src/Hypesoft.API"

# Build sem restore (já foi feito)
RUN dotnet build "Hypesoft.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Hypesoft.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hypesoft.API.dll"]