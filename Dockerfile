#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ForbiddenKnowledge/ForbiddenKnowledge.csproj", "ForbiddenKnowledge/"]
RUN dotnet restore "./ForbiddenKnowledge/ForbiddenKnowledge.csproj"
COPY . .
WORKDIR "/src/ForbiddenKnowledge"
RUN dotnet build "./ForbiddenKnowledge.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ForbiddenKnowledge.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false \
	&& ls -la /app/publish/wwwroot

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "ForbiddenKnowledge.dll"]