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

## TODO

- [ ] 將 migration 依照 Database 種類分成多個資料夾。