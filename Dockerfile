# ============================================
# Build Stage
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution file
COPY LegacyOrderService.sln ./

# Copy all project files for dependency resolution and layer caching
COPY Common/Common.csproj ./Common/
COPY Model/Model.csproj ./Model/
COPY Domain/Domain.csproj ./Domain/
COPY Repository/Repository.csproj ./Repository/
COPY Service/Service.csproj ./Service/
COPY LegacyOrder/LegacyOrder.csproj ./LegacyOrder/
COPY LegacyOrder.Tests/LegacyOrder.Tests.csproj ./LegacyOrder.Tests/

# Restore NuGet packages for the entire solution
RUN dotnet restore LegacyOrderService.sln

# Copy the remaining source code
COPY Common/ ./Common/
COPY Model/ ./Model/
COPY Domain/ ./Domain/
COPY Repository/ ./Repository/
COPY Service/ ./Service/
COPY LegacyOrder/ ./LegacyOrder/

# Publish directly without --no-restore (let it restore if needed)
WORKDIR /src/LegacyOrder
RUN dotnet publish LegacyOrder.csproj -c Release -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Runtime Stage
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Install required dependencies for PostgreSQL and timezone data
RUN apk add --no-cache icu-libs tzdata

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    TZ=Asia/Bangkok

# Create a non-root user for security
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser

# Set working directory
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Create logs directory and set permissions
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/health || exit 1

# Set the entrypoint to run the application
ENTRYPOINT ["dotnet", "LegacyOrder.dll"]