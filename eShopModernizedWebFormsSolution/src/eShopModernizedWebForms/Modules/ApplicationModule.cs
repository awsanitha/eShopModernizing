using Autofac;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Models.Infrastructure;
using eShopModernizedWebForms.Services;

namespace eShopModernizedWebForms.Modules
{
    public class ApplicationModule : Module
    {
        private readonly bool _useMockData;
        private readonly bool _useAzureStorage;
        private readonly bool _useManagedIdentity;

        public ApplicationModule(bool useMockData, bool useAzureStorage, bool useManagedIdentity)
        {
            _useMockData = useMockData;
            _useAzureStorage = useAzureStorage;
            _useManagedIdentity = useManagedIdentity;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_useMockData)
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

            if (_useAzureStorage)
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

            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();

            if (_useManagedIdentity)
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
