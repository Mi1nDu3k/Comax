# -----------------------------------------------------------------------------
# GIAI ĐOẠN 1: BUILD
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy các file project
COPY ["Comax.API/Comax.API.csproj", "Comax.API/"]
COPY ["Comax.Data/Comax.Data.csproj", "Comax.Data/"]
COPY ["Comax.Common/Comax.Common.csproj", "Comax.Common/"]

# Restore thư viện
RUN dotnet restore "Comax.API/Comax.API.csproj"

# Copy toàn bộ code
COPY . .

# Build
WORKDIR "/src/Comax.API"
RUN dotnet publish "Comax.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# GIAI ĐOẠN 2: RUNTIME (Lưu ý: Dùng ảnh aspnet cho Web API)
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

# Cấu hình múi giờ
ENV TZ="Asia/Ho_Chi_Minh"

ENTRYPOINT ["dotnet", "Comax.API.dll"]