using Autofac;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;

namespace eShopLegacyWebForms.Modules
{
    public class ApplicationModule : Module
    {
        private readonly bool _useMockData;

        public ApplicationModule(bool useMockData)
        {
            _useMockData = useMockData;
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

            builder.RegisterType<CatalogItemHiLoGenerator>()
                .SingleInstance();

            builder.RegisterType<CatalogDBInitializer>()
                .InstancePerLifetimeScope();
        }
    }
}
