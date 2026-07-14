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

            // Use explicit binding and endpoint address (app.config-based client endpoint
            // configuration is not supported in System.ServiceModel.Http on .NET Core+).
            var binding = new BasicHttpBinding();
            var endpointAddress = new EndpointAddress("http://localhost:62314/CatalogService.svc");
            ICatalogService service = new CatalogServiceClient(binding, endpointAddress);

            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
