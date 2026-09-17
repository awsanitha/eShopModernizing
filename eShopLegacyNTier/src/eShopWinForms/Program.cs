using eShopWinForms.Controllers;
using eShopWinForms.eShopServiceReference;
using System;
using System.ServiceModel;
using System.Windows.Forms;

namespace eShopWinForms
{
    static class Program
    {
        // Service endpoint URL - update this to match the running eShopWCFService address
        private const string ServiceEndpointUrl = "http://localhost:5000/CatalogService.svc";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Use explicit binding + endpoint address (config-based endpoints not supported on .NET 10)
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(ServiceEndpointUrl);

            CatalogView catalogView = new CatalogView();
            ICatalogService service = new CatalogServiceClient(binding, endpoint);
            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
