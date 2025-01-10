# Stage 1: Build the Blazor WebAssembly project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY ["ContainerControlPanel.Web/ContainerControlPanel.Web.csproj", "ContainerControlPanel.Web/"]
RUN dotnet restore "ContainerControlPanel.Web/ContainerControlPanel.Web.csproj"

# Copy the entire project and build it
COPY . .
WORKDIR "/src/ContainerControlPanel.Web"
RUN dotnet publish "ContainerControlPanel.Web.csproj" -c Release -o /app/publish

# Stage 2: Serve the application using a lightweight web server
FROM nginx:latest AS final
WORKDIR /usr/share/nginx/html

# Clean the default Nginx files and copy the Blazor output
RUN rm -rf ./*
COPY --from=build /app/publish/wwwroot .

# Copy entrypoint.sh to the container
COPY entrypoint.sh /entrypoint.sh

# Fix entrypoint.sh line endings from CRLF to LF
RUN sed -i 's/\r//' /entrypoint.sh

# Expose port 80 to serve the Blazor app
EXPOSE 80

# Start the application
ENTRYPOINT ["bash", "/entrypoint.sh"]
