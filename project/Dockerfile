FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["project.csproj", "project/"]
RUN dotnet restore "project/project.csproj"
COPY . .
WORKDIR "/src/project"
RUN dotnet tool install --global dotnet-ef
RUN dotnet build "project.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "project.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod +x ./project/entrypoint.sh
ENTRYPOINT ["./project/entrypoint.sh"]
ENTRYPOINT ["dotnet", "project.dll"]