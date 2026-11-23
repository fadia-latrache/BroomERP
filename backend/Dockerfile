FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY BroomErp.slnx ./
COPY src/BroomErp.Domain/BroomErp.Domain.csproj src/BroomErp.Domain/
COPY src/BroomErp.Application/BroomErp.Application.csproj src/BroomErp.Application/
COPY src/BroomErp.Infrastructure/BroomErp.Infrastructure.csproj src/BroomErp.Infrastructure/
COPY src/BroomErp.Api/BroomErp.Api.csproj src/BroomErp.Api/

RUN dotnet restore BroomErp.slnx

# Copy source and publish
COPY src/ ./src/
RUN dotnet publish src/BroomErp.Api/BroomErp.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "BroomErp.Api.dll"]
