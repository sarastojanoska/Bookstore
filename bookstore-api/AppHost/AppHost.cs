var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter(
    "sql-password",
    secret: true);

var sqlServer = builder
    .AddSqlServer("sql", sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var database = sqlServer.AddDatabase("bookstore");

builder.AddProject<Projects.API>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();