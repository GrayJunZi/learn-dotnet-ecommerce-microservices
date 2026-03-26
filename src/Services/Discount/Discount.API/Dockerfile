FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Discount/Discount.API/Discount.API.csproj", "src/Services/Discount/Discount.API/"]
COPY ["src/Services/Discount/Discount.Infrastructure/Discount.Infrastructure.csproj", "src/Services/Discount/Discount.Infrastructure/"]
COPY ["src/Services/Discount/Discount.Core/Discount.Core.csproj", "src/Services/Discount/Discount.Core/"]
COPY ["src/Services/Discount/Discount.Application/Discount.Application.csproj", "src/Services/Discount/Discount.Application/"]
RUN dotnet restore "src/Services/Discount/Discount.API/Discount.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Discount/Discount.API"
RUN dotnet build "./Discount.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Discount.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discount.API.dll"]
