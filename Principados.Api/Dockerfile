FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Principados.Domain/Principados.Domain.csproj Principados.Domain/
COPY Principados.Application/Principados.Application.csproj Principados.Application/
COPY Principados.Infrastructure/Principados.Infrastructure.csproj Principados.Infrastructure/
COPY Principados.Api/Principados.Api.csproj Principados.Api/
RUN dotnet restore Principados.Api/Principados.Api.csproj

COPY Principados.Domain/ Principados.Domain/
COPY Principados.Application/ Principados.Application/
COPY Principados.Infrastructure/ Principados.Infrastructure/
COPY Principados.Api/ Principados.Api/

RUN dotnet publish Principados.Api/Principados.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Principados.Api.dll"]
