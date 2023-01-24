#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MDM-B/MDM-B.csproj", "MDM-B/"]
COPY ["Shared.Lib/Shared.Lib.csproj", "Shared.Lib/"]
RUN dotnet restore "MDM-B/MDM-B.csproj"
COPY . .
WORKDIR "/src/MDM-B"
RUN dotnet build "MDM-B.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MDM-B.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MDM-B.dll"]