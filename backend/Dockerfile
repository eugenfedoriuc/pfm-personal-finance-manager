FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props Pfm.slnx ./
COPY src/Pfm.Api/Pfm.Api.csproj src/Pfm.Api/
COPY src/Pfm.Application/Pfm.Application.csproj src/Pfm.Application/
COPY src/Pfm.Domain/Pfm.Domain.csproj src/Pfm.Domain/
COPY src/Pfm.Infrastructure/Pfm.Infrastructure.csproj src/Pfm.Infrastructure/
RUN dotnet restore src/Pfm.Api/Pfm.Api.csproj

COPY src ./src
RUN dotnet publish src/Pfm.Api/Pfm.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
    && apt-get install --no-install-recommends --yes curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Pfm.Api.dll"]
