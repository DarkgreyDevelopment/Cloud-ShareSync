FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY . /build
WORKDIR /build
RUN dotnet restore "src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj"
RUN dotnet build "src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj" -c Release -o /app/build
RUN dotnet publish "src/SimpleBackup/Cloud-ShareSync.SimpleBackup.csproj" -c Release -o /app/publish
COPY LICENSE /app/publish
COPY ./build/entrypoint.sh /app/publish


FROM base AS final
RUN apt-get update
RUN apt-get install -y p7zip p7zip-full
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["/bin/bash", "-c", "/app/entrypoint.sh"]
