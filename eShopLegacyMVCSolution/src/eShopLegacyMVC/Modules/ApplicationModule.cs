using Autofac;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Modules
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
        }
    }
}
