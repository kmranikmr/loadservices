#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "10002"

#Akka.Remote inbound listening endpoint
EXPOSE 10002 

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ./get-dockerip.sh ./get-dockerip.sh
COPY ["DataAnalyticsPlatform.Service.Worker/DataAnalyticsPlatform.Service.Worker.csproj", "DataAnalyticsPlatform.Service.Worker/"]
COPY ["DataAnalyticsPlatform.Actors/DataAnalyticsPlatform.Actors.csproj", "DataAnalyticsPlatform.Actors/"]
COPY ["DataAnalyticsPlatform.Writers/DataAnalyticsPlatform.Writers.csproj", "DataAnalyticsPlatform.Writers/"]
COPY ["DataAnalyticsPlatform.Common/DataAnalyticsPlatform.Common.csproj", "DataAnalyticsPlatform.Common/"]
COPY ["DataAnalyticsPlatform.SharedUtils/DataAnalyticsPlatform.SharedUtils.csproj", "DataAnalyticsPlatform.SharedUtils/"]
COPY ["DataAnalyticsPlatform.Shared/DataAnalyticsPlatform.Shared.csproj", "DataAnalyticsPlatform.Shared/"]
COPY ["DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/JsonClassGeneratorLibCore.csproj", "DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/"]
COPY ["DataAnalyticsPlatform.Readers/DataAnalyticsPlatform.Readers.csproj", "DataAnalyticsPlatform.Readers/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]

COPY ["LoadServiceApi/LoadServiceApi/Scripts/PostgresFunctions.txt", "PostgresFunctions.txt"]

RUN dotnet restore "DataAnalyticsPlatform.Service.Worker/DataAnalyticsPlatform.Service.Worker.csproj"
COPY . .
WORKDIR "/src/DataAnalyticsPlatform.Service.Worker"
RUN dotnet build "DataAnalyticsPlatform.Service.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAnalyticsPlatform.Service.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /src/get-dockerip.sh ./get-dockerip.sh

COPY ["LoadServiceApi/LoadServiceApi/Scripts/PostgresFunctions.txt", "PostgresFunctions.txt"]
COPY --from=publish /app/publish .

ENTRYPOINT ["/bin/bash","get-dockerip.sh"]
CMD ["dotnet", "DataAnalyticsPlatform.Service.Worker.dll"]

