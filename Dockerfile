FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CalendarioBrasil.Domain/CalendarioBrasil.Domain.csproj CalendarioBrasil.Domain/
COPY CalendarioBrasil.Application/CalendarioBrasil.Application.csproj CalendarioBrasil.Application/
COPY CalendarioBrasil.Infrastructure/CalendarioBrasil.Infrastructure.csproj CalendarioBrasil.Infrastructure/
COPY CalendarioBrasil.Api/CalendarioBrasil.Api.csproj CalendarioBrasil.Api/
COPY CalendarioBrasil.Host/CalendarioBrasil.Host.csproj CalendarioBrasil.Host/

RUN dotnet restore CalendarioBrasil.Host/CalendarioBrasil.Host.csproj

COPY . .
RUN dotnet publish CalendarioBrasil.Host/CalendarioBrasil.Host.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CalendarioBrasil.Host.dll"]