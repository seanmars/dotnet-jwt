# WebApp

## Scripts

- Add the new migrations for database

```shell
dotnet ef migrations add {migration-name} -o .\Data\Migrations -c ApplicationDbContext
```

- Apply the new migrations

```shell
dotnet ef database update --context ApplicationDbContext
# or with environment
dotnet ef database update --context ApplicationDbContext -- --environment {Environment}
```