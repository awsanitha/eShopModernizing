using Autofac;
using eShopModernizedMVC;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Models.Infrastructure;
using eShopModernizedMVC.Services;
using Microsoft.EntityFrameworkCore;
// ISqlConnectionFactory, ManagedIdentitySqlConnectionFactory, AppSettingsSqlConnectionFactory
// are declared in the root eShopModernizedMVC namespace (App_Start/SqlAccessTokenProvider.cs)

namespace eShopModernizedMVC.Modules
{
    public class ApplicationModule : Module
    {
        private bool useMockData;
        private bool useAzureStorage;
        private bool useManagedIdentity;

        public ApplicationModule(bool useMockData, bool useAzureStorage, bool useManagedIdentity)
        {
            this.useMockData = useMockData;
            this.useAzureStorage = useAzureStorage;
            this.useManagedIdentity = useManagedIdentity;
        }
        protected override void Load(ContainerBuilder builder)
        {
            if (this.useMockData)
            {
                builder.RegisterType<CatalogServiceMock>()
                    .As<ICatalogService>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<CatalogService>()
                    .As<ICatalogService>()
                    .InstancePerLifetimeScope();
            }

            if (this.useAzureStorage)
            {
                builder.RegisterType<ImageAzureStorage>()
                    .As<IImageService>()
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<ImageMockStorage>()
                  .As<IImageService>()
                  .InstancePerLifetimeScope();
            }

            if (!this.useMockData)
            {
                builder.Register(c =>
                {
                    var connectionFactory = c.Resolve<ISqlConnectionFactory>();
                    var optionsBuilder = new DbContextOptionsBuilder<CatalogDBContext>();
                    optionsBuilder.UseSqlServer(connectionFactory.GetConnectionString());
                    return new CatalogDBContext(optionsBuilder.Options);
                }).InstancePerLifetimeScope();

                builder.RegisterType<CatalogDBInitializer>()
                    .InstancePerLifetimeScope();
            }

            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();

            if (this.useManagedIdentity)
            {
                builder.RegisterType<ManagedIdentitySqlConnectionFactory>()
                    .As<ISqlConnectionFactory>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<AppSettingsSqlConnectionFactory>()
                    .As<ISqlConnectionFactory>()
                    .SingleInstance();
            }
        }
    }
}
