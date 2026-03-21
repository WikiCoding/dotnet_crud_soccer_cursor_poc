FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["cursor_dotnet_test.csproj", "./"]
RUN dotnet restore "cursor_dotnet_test.csproj"

COPY . .
RUN dotnet publish "cursor_dotnet_test.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cursor_dotnet_test.dll"]
