using Autofac;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Models.Infrastructure;
using eShopModernizedMVC.Services;
using Microsoft.Extensions.Configuration;

namespace eShopModernizedMVC.Modules
{
    public class ApplicationModule : Module
    {
        private readonly bool useMockData;
        private readonly bool useAzureStorage;

        public ApplicationModule(bool useMockData, bool useAzureStorage)
        {
            this.useMockData = useMockData;
            this.useAzureStorage = useAzureStorage;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (useMockData)
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

            if (useAzureStorage)
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
        }
    }
}
