FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Part2/RaceDay.Contracts/RaceDay.Contracts.csproj Part2/RaceDay.Contracts/
COPY Part3/RaceDay.Web/RaceDay.Web.csproj Part3/RaceDay.Web/
RUN dotnet restore Part3/RaceDay.Web/RaceDay.Web.csproj
COPY Part2/RaceDay.Contracts/ Part2/RaceDay.Contracts/
COPY Part3/RaceDay.Web/ Part3/RaceDay.Web/
RUN dotnet publish Part3/RaceDay.Web/RaceDay.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]