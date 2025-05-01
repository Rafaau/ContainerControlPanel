<h1>
  <img src="https://i.imgur.com/rm066eX.png">
  Container Control Panel
</h1>

<p float="left">
	<img src="https://img.shields.io/badge/.NET-9.0-blue">
	<a href="https://www.nuget.org/packages/ContainerControlPanel.Extensions/">
		<img src="https://img.shields.io/badge/ContainerControlPanel.Extensions-1.0.3-blue">
	</a>
</p>

## What is CCP?
Container Control Panel is an open-source stack for monitoring and managing .NET Core applications running as Docker containers.
It is stronly inspired (and much better) by the .NET Aspire platform. In addition to the handling OpenTelemetry, it also provides
advanced features like managing Docker containers, swagger-like API documentation, capturing full details of the calls made to the
API, and much more. Unlike .NET Aspire, CCP does not require to specify the projects orchestration.

## Quick start
To get started, you can use the recommended Docker Compose file and run the application:
```
version: '3'

services:
    ccp-api:
        image: rafaau/ccp-api:latest
        ports:
            - 5121:8080
        depends_on:
            - mongodb
        networks:
            - ccp-network
        environment:
            - MongoDB__ConnectionString=mongodb://localhost:27017
            - WebApp__Port=5069
            - WebApp__Host=localhost
            - ComposeDir=C:\\Users\\user\\Desktop\composes
            - ImagesDir=C:\\Users\\user\\Desktop\images
        volumes:
            - /var/run/docker.sock:/var/run/docker.sock:rw
        privileged: true

    ccp-web:
        image: rafaau/ccp-web:latest
        ports:
            - 5069:80
        depends_on:
            - ccp-api
        networks:
            - ccp-network
        environment:
            - UserToken=Password
            - AdminToken=Password12345
            - AppName=NotAspire
            - WebAPIPort=5121
            - WebAPIHost=localhost
            - Realtime=true
            - LazyLoading=true

    mongodb:
        image: mongo:latest
        ports:
            - "27017:27017"
        networks:
            - ccp-network     

networks:
    ccp-network:
        driver: bridge

```

At this point, you can access the CCP application at http://localhost:5069. You will be redirected to Containers page, where you can
see the list of containers running on your machine. To enable the full monitoring capabilities, you need to add 
<a href="https://www.nuget.org/packages/ContainerControlPanel.Extensions/">ContainerControlPanel.Extensions</a>
and also <a href="https://www.nuget.org/packages/ApiDocs/">ApiDocs</a> NuGet packages to your project.

The sample implementation should look like this:

```
builder.AddOpenTelemetry(host: "host.docker.internal", port: 5121, resourceName: "WeatherForecastAPI");
```

And then to enable <b>ApiDocs</b> handling:

```
app.MapApiDocs();
```
<br>

Also to enable request and response logging (optional):
```
app.UseMiddleware<OpenTelemetryLoggingMiddleware>();
```
<br>

> [!NOTE]
> There is a probability that Controller will return <i>NotSupportedException</i> due to logging requests. In that case you need to specify the additional middleware in your <i>Program.cs</i> as below:

```
app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });
```

> [!IMPORTANT]
> It is also recommended to specify CORS policy for the CCP API:

```
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCCP", policy =>
    {
        policy.WithOrigins("http://host.docker.internal:5069")
              .SetIsOriginAllowed((host) => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

And then:

```
app.UseCors("AllowCCP");
```

## Configuration
You can configure your CCP stack by specifing following environment variables in your docker-compose file:

| Service | ENV                       | DataType   | Description                                                                |
| ------- | ------------------------- | ---------- | -------------------------------------------------------------------------- |
| WebAPI  | Redis__ConnectionString   | String     | Specifies the connection string for the Redis server.                      |
| WebAPI  | MongoDB__ConnectionString | String     | Specifies the connection string for the Mongo server (RECOMMENDED).        |
| WebAPI  | WebApp__Port              | Int        | Specifies the port number for the WebApp service.                          |
| WebAPI  | WebApp__Host              | String     | Specifies the host address of the WebApp service.                          |
| WebAPI  | ComposeDir                | String     | Specifies the path to Docker composes directory.                           |
| WebAPI  | ImagesDir                 | String     | Specifies the path to Docker images directory.                             |
| WebAPI  | AuthToken                 | String     | Specifies the authorization token.                                         |
| WebAPI  | ExcludeByName             | String     | Specifies the names of containers that will be hidden (separated by ;).    |
| WebAPI  | ExcludeByImage            | String     | Specifies the names of images that will be hidden (separated by ;).        |
| WebApp  | AppName                   | String     | Specifies the name of the application.                                     |
| WebApp  | AdminToken                | String     | Specifies the token for the admin user.                                    |
| WebApp  | UserToken                 | String     | Specifies the token for the regular user.                                  |
| WebApp  | WebAPIPort                | Int        | Specifies the port number for the WebAPI service.                          |
| WebApp  | WebAPIHost                | String     | Specifies the host of the WebAPI service.                                  |
| WebApp  | Realtime                  | Boolean    | Specifies whether the application should use real-time updates.            |
| WebApp  | TimeOffset                | Int        | Specifies the time offset for the application.                             |
| WebApp  | AuthToken                 | String     | Specifies the authorization token for WebAPI calls.                        |
| WebApp  | Context                   | String     | Specifies the project identificator for docker-compose (-p flag)           |
| WebApp  | LazyLoading               | Boolean    | Specifies whether the application should use lazy loading (MONGO CASE ONLY)|

## Screenshots

<img src="https://i.imgur.com/mPMtZjF.png">
<img src="https://i.imgur.com/gJkQAQX.png">
<img src="https://i.imgur.com/QwxgEzu.png">
<img src="https://i.imgur.com/11BVYyl.png">
<img src="https://i.imgur.com/StVtqdt.png">
<img src="https://i.imgur.com/WLWoxm1.png">
