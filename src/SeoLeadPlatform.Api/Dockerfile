FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview AS build
WORKDIR /src

COPY SeoLeadPlatform.slnx ./
COPY src/SeoLeadPlatform.Api/SeoLeadPlatform.Api.csproj src/SeoLeadPlatform.Api/
COPY src/SeoLeadPlatform.Application/SeoLeadPlatform.Application.csproj src/SeoLeadPlatform.Application/
COPY src/SeoLeadPlatform.Domain/SeoLeadPlatform.Domain.csproj src/SeoLeadPlatform.Domain/
COPY src/SeoLeadPlatform.Infrastructure/SeoLeadPlatform.Infrastructure.csproj src/SeoLeadPlatform.Infrastructure/
RUN dotnet restore src/SeoLeadPlatform.Api/SeoLeadPlatform.Api.csproj

COPY src/ src/
RUN dotnet publish src/SeoLeadPlatform.Api/SeoLeadPlatform.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SeoLeadPlatform.Api.dll"]
