// using Artemis.Core.DryIoc;
// using Artemis.Storage;
// using Artemis.Storage.Legacy;
// using DryIoc;
// using Microsoft.EntityFrameworkCore;
// using Serilog;
//
// using Container container = new(rules => rules
//     .WithMicrosoftDependencyInjectionRules()
//     .WithConcreteTypeDynamicRegistrations()
//     .WithoutThrowOnRegisteringDisposableTransient());
//
// container.RegisterCore();
//
// ILogger logger = container.Resolve<ILogger>();
// ArtemisDbContext dbContext = container.Resolve<ArtemisDbContext>();
//
// logger.Information("Applying pending migrations...");
// dbContext.Database.Migrate();
// logger.Information("Pending migrations applied");
//
// MigrationService.MigrateToSqlite(logger, dbContext);