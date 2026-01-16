# 1. Giai đoạn Build (Dùng SDK để compile code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy các file .csproj của các project liên quan
COPY ["Comax.Crawler/Comax.Crawler.csproj", "Comax.Crawler/"]
COPY ["Comax.Data/Comax.Data.csproj", "Comax.Data/"]
COPY ["Comax.Common/Comax.Common.csproj", "Comax.Common/"]

# Tải các thư viện (Restore)
RUN dotnet restore "Comax.Crawler/Comax.Crawler.csproj"

# Copy toàn bộ source code còn lại
COPY . .

# Build và Publish bản Release
WORKDIR "/src/Comax.Crawler"
RUN dotnet publish "Comax.Crawler.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Giai đoạn Run (Dùng Runtime để chạy - nhẹ hơn SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .


ENTRYPOINT ["dotnet", "Comax.Crawler.dll"]