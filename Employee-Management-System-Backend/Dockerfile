FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything into the build context (Render will build from the service rootDir)
COPY . ./

RUN dotnet restore "Employee-Management-System-Backend.csproj"
RUN dotnet publish "Employee-Management-System-Backend.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

# Render provides $PORT at runtime
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "Employee-Management-System-Backend.dll"]

