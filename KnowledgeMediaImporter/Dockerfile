FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["KnowledgeMediaImporter/KnowledgeMediaImporter.csproj", "KnowledgeMediaImporter/"]
RUN dotnet restore "KnowledgeMediaImporter/KnowledgeMediaImporter.csproj"
COPY . .
WORKDIR "/src/KnowledgeMediaImporter"
RUN dotnet build "KnowledgeMediaImporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KnowledgeMediaImporter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KnowledgeMediaImporter.dll"]
