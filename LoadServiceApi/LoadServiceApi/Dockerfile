#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "10001"

#Akka.Remote inbound listening endpoint
EXPOSE 10001
EXPOSE 6003

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ./get-dockerip.sh ./get-dockerip.sh
COPY ["LoadServiceApi/LoadServiceApi/LoadServiceApi.csproj", "LoadServiceApi/LoadServiceApi/"]
COPY ["DataAnalyticsPlatform.SharedUtils/DataAnalyticsPlatform.SharedUtils.csproj", "DataAnalyticsPlatform.SharedUtils/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["DataAnalyticsPlatform.Actors/DataAnalyticsPlatform.Actors.csproj", "DataAnalyticsPlatform.Actors/"]
COPY ["DataAnalyticsPlatform.Writers/DataAnalyticsPlatform.Writers.csproj", "DataAnalyticsPlatform.Writers/"]
COPY ["DataAnalyticsPlatform.Common/DataAnalyticsPlatform.Common.csproj", "DataAnalyticsPlatform.Common/"]
COPY ["DataAnalyticsPlatform.Shared/DataAnalyticsPlatform.Shared.csproj", "DataAnalyticsPlatform.Shared/"]
COPY ["DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/JsonClassGeneratorLibCore.csproj", "DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/"]
COPY ["DataAnalyticsPlatform.Readers/DataAnalyticsPlatform.Readers.csproj", "DataAnalyticsPlatform.Readers/"]
RUN dotnet restore "LoadServiceApi/LoadServiceApi/LoadServiceApi.csproj"
COPY . .
WORKDIR "/src/LoadServiceApi/LoadServiceApi"
RUN dotnet build "LoadServiceApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoadServiceApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /src/get-dockerip.sh ./get-dockerip.sh
COPY --from=publish /app/publish .
ENTRYPOINT ["/bin/bash","get-dockerip.sh"]
CMD ["dotnet", "LoadServiceApi.dll"]
