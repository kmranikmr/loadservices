#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "3551"

#Akka.Remote inbound listening endpoint
EXPOSE 3551
EXPOSE 9110
 
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ./get-dockerip.sh ./get-dockerip.sh
COPY ["DataAnalyticsPlatform.Service.MasterAPI/DataAnalyticsPlatform.Service.MasterAPI.csproj", "DataAnalyticsPlatform.Service.MasterAPI/"]
COPY ["DataAnalyticsPlatform.Actors/DataAnalyticsPlatform.Actors.csproj", "DataAnalyticsPlatform.Actors/"]
COPY ["DataAnalyticsPlatform.Writers/DataAnalyticsPlatform.Writers.csproj", "DataAnalyticsPlatform.Writers/"]
COPY ["DataAnalyticsPlatform.Common/DataAnalyticsPlatform.Common.csproj", "DataAnalyticsPlatform.Common/"]
COPY ["DataAnalyticsPlatform.SharedUtils/DataAnalyticsPlatform.SharedUtils.csproj", "DataAnalyticsPlatform.SharedUtils/"]
COPY ["DataAnalyticsPlatform.Shared/DataAnalyticsPlatform.Shared.csproj", "DataAnalyticsPlatform.Shared/"]
COPY ["DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/JsonClassGeneratorLibCore.csproj", "DataAnalyticsPlatform.Shared/JsonClassGeneratorLibCore/"]
COPY ["DataAnalyticsPlatform.Readers/DataAnalyticsPlatform.Readers.csproj", "DataAnalyticsPlatform.Readers/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
RUN dotnet restore "DataAnalyticsPlatform.Service.MasterAPI/DataAnalyticsPlatform.Service.MasterAPI.csproj"
COPY . .
WORKDIR "/src/DataAnalyticsPlatform.Service.MasterAPI"
RUN dotnet build "DataAnalyticsPlatform.Service.MasterAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAnalyticsPlatform.Service.MasterAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /src/get-dockerip.sh ./get-dockerip.sh
COPY --from=publish /app/publish .
ENTRYPOINT ["/bin/bash","get-dockerip.sh"]
CMD ["dotnet", "DataAnalyticsPlatform.Service.MasterAPI.dll"]
