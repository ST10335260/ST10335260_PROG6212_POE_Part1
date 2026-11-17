FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ST10335260_PROG6212_POE_Part3.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish ST10335260_PROG6212_POE_Part3.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish ./

USER $APP_UID
ENTRYPOINT ["dotnet", "ST10335260_PROG6212_POE_Part3.dll"]
