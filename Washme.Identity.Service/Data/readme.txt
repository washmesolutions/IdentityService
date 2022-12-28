//Migration scripts(Run on packge manager console)

dotnet ef migrations add InitialMigrationV3 -c ApplicationDbContext

dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Migrations/IdentityServer/PersistedGrantDb

dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Migrations/IdentityServer/ConfigurationDb