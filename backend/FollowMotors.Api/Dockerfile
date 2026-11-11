FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY backend/FollowMotors.Api/FollowMotors.Api.csproj backend/FollowMotors.Api/
RUN dotnet restore backend/FollowMotors.Api/FollowMotors.Api.csproj
COPY backend/FollowMotors.Api/ backend/FollowMotors.Api/
RUN dotnet publish backend/FollowMotors.Api/FollowMotors.Api.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "FollowMotors.Api.dll"]
