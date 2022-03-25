FROM mcr.microsoft.com/dotnet/runtime:6.0-bullseye-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
COPY . /build
WORKDIR /build
RUN dotnet restore "src/Cloud-ShareSync.csproj"
RUN dotnet build "src/Cloud-ShareSync.csproj" -c Release -o /app/build
RUN dotnet publish "src/Cloud-ShareSync.csproj" -c Release -o /app/publish


FROM base AS final
RUN apt-get update
RUN apt-get install -y p7zip p7zip-full
WORKDIR /app
COPY --from=build /app/publish .
cmd ["sh", "-c", "/app/Cloud-ShareSync", "{Action}"]
