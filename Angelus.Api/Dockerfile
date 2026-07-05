FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Angelus.Domain/Angelus.Domain.csproj Angelus.Domain/
COPY Angelus.Application/Angelus.Application.csproj Angelus.Application/
COPY Angelus.Infrastructure/Angelus.Infrastructure.csproj Angelus.Infrastructure/
COPY Angelus.Api/Angelus.Api.csproj Angelus.Api/
RUN dotnet restore Angelus.Api/Angelus.Api.csproj

COPY Angelus.Domain/ Angelus.Domain/
COPY Angelus.Application/ Angelus.Application/
COPY Angelus.Infrastructure/ Angelus.Infrastructure/
COPY Angelus.Api/ Angelus.Api/

RUN dotnet publish Angelus.Api/Angelus.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Angelus.Api.dll"]
