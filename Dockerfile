FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Compliance360.sln ./
COPY src/Compliance360.Shared/Compliance360.Shared.csproj src/Compliance360.Shared/
COPY src/Compliance360.Domain/Compliance360.Domain.csproj src/Compliance360.Domain/
COPY src/Compliance360.Application/Compliance360.Application.csproj src/Compliance360.Application/
COPY src/Compliance360.Infrastructure/Compliance360.Infrastructure.csproj src/Compliance360.Infrastructure/
COPY src/Compliance360.Web/Compliance360.Web.csproj src/Compliance360.Web/
COPY tests/Compliance360.Tests/Compliance360.Tests.csproj tests/Compliance360.Tests/

RUN dotnet restore "Compliance360.sln"

COPY . .
RUN dotnet build "Compliance360.sln" --configuration Release --no-restore -warnaserror
RUN dotnet publish "src/Compliance360.Web/Compliance360.Web.csproj" --configuration Release --no-build --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Compliance360.Web.dll"]
