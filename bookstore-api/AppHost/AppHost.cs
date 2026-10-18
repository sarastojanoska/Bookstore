var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder
    .AddSqlServer("sql");
//.WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("bookstore");

var authentication = builder.AddProject<Projects.Authentication>("auth")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.API>("api")
    .WithReference(database)
    .WithReference(authentication)
    .WaitFor(database)
    .WaitFor(authentication);

builder.Build().Run();