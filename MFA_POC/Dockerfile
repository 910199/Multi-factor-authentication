FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MFA_POC.csproj", ""]
RUN dotnet restore "MFA_POC.csproj"
COPY . .
WORKDIR "/src/"

RUN dotnet build "MFA_POC.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MFA_POC.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN apt-get update && apt-get install -y wkhtmltopdf 
ENTRYPOINT ["dotnet", "MFA_POC.dll"]
