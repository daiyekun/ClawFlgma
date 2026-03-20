var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("pg-password", secret: true);

var postgresdb = builder.AddPostgres("postgres", password: password)
    .WithHostPort(5432)
    .WithDataVolume("postgres-claw-flgma")
    .WithPgWeb(pgAdmin => pgAdmin.WithHostPort(5050))
    // 挂载初始化脚本：容器启动时会自动执行该目录下的 .sql 文件创建 erp_demo
    .WithBindMount("./sql", "/docker-entrypoint-flgma-initdb.d")
    .AddDatabase("claw-flgma");

builder.AddProject<Projects.ClawFlgma_AuthService>("clawflgma-authservice");

builder.AddProject<Projects.ClawFlgma_AuthService>("authservice")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);


builder.AddProject<Projects.ClawFlgma_ApiGateway>("apigateway")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_UserService>("userservice")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);


builder.AddProject<Projects.ClawFlgma_ProjectService>("projectservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_NotificationService>("notificationservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_DesignService>("designservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_CollaborationService>("collaborationservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_AssetService>("assetservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.AddProject<Projects.ClawFlgma_AIService>("aiservice")
     .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.Build().Run();
