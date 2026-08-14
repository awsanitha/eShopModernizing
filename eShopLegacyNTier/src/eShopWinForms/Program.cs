using eShopWinForms.Controllers;
using eShopWinForms.eShopServiceReference;
using System;
using System.ServiceModel;
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

            CatalogView catalogView = new CatalogView();

            // Construct WCF client with explicit binding and address
            // (config-file based endpoint construction is not supported on .NET Core)
            var serviceUrl = Environment.GetEnvironmentVariable("ESHOP_SERVICE_URL")
                ?? "http://localhost:62314/CatalogService.svc";
            ICatalogService service = new CatalogServiceClient(
                new BasicHttpBinding(),
                new EndpointAddress(serviceUrl));

            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
