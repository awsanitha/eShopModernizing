using Autofac;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;

namespace eShopModernizedWebForms.Modules
{
    public class ApplicationModule : Module
    {
        private readonly bool useMockData;
        private readonly bool useAzureStorage;
        private readonly string webRootPath;

        public ApplicationModule(bool useMockData, bool useAzureStorage, string webRootPath)
        {
            this.useMockData = useMockData;
            this.useAzureStorage = useAzureStorage;
            this.webRootPath = webRootPath;
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
                builder.Register(c => new ImageAzureStorage(webRootPath))
                    .As<IImageService>()
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.Register(c => new ImageMockStorage(webRootPath))
                  .As<IImageService>()
                  .InstancePerLifetimeScope();
            }

            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();
        }
    }
}
