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

            // Use explicit binding + address to avoid relying on app.config <client> endpoint lookup,
            // which is not supported by the System.ServiceModel NuGet client packages.
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:62314/CatalogService.svc");
            ICatalogService service = new eShopServiceReference.CatalogServiceClient(binding, endpoint);

            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
