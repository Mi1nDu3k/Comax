# Sử dụng Base Image giống API
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy các file csproj để restore trước
COPY ["Comax.VipWorker/Comax.VipWorker.csproj", "Comax.VipWorker/"]
COPY ["Comax.Data/Comax.Data.csproj", "Comax.Data/"]
COPY ["Comax.Common/Comax.Common.csproj", "Comax.Common/"]
RUN dotnet restore "Comax.VipWorker/Comax.VipWorker.csproj"

# Copy toàn bộ source code
COPY . .
WORKDIR "/src/Comax.VipWorker"
RUN dotnet build "Comax.VipWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Comax.VipWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Comax.VipWorker.dll"]