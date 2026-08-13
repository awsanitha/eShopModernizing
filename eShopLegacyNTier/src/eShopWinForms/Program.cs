using eShopWinForms.Controllers;
using eShopWinForms.eShopServiceReference;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace eShopWinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Build the WCF client with an explicit binding + endpoint address.
            // System.ServiceModel on .NET 5+ does not read endpoint configuration from
            // app.config's <system.serviceModel> section; the URL is read from <appSettings>.
            string serviceUrl =
                ConfigurationManager.AppSettings["ServiceUrl"]
                ?? "http://localhost:62314/CatalogService.svc";

            var binding = new System.ServiceModel.BasicHttpBinding();
            var address = new System.ServiceModel.EndpointAddress(serviceUrl);
            ICatalogService service = new CatalogServiceClient(binding, address);

            CatalogView catalogView = new CatalogView();
            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
