FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ECommerce.slnx .
COPY src/ECommerce.Domain/ECommerce.Domain.csproj src/ECommerce.Domain/
COPY src/ECommerce.Application/ECommerce.Application.csproj src/ECommerce.Application/
COPY src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/ECommerce.Infrastructure/
COPY src/ECommerce.Shared/ECommerce.Shared.csproj src/ECommerce.Shared/
COPY src/ECommerce.Web/ECommerce.Web.csproj src/ECommerce.Web/
COPY tests/ECommerce.Tests/ECommerce.Tests.csproj tests/ECommerce.Tests/

RUN dotnet restore ECommerce.slnx

# Copy everything and build
COPY . .
RUN dotnet build src/ECommerce.Web/ECommerce.Web.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish src/ECommerce.Web/ECommerce.Web.csproj -c Release -o /app/publish --no-build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Web.dll"]
