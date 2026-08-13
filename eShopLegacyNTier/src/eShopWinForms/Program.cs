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

            // Use explicit binding + endpoint address – config-file client endpoints are not
            // supported by the System.ServiceModel.* NuGet client packages on modern .NET.
            var binding = new BasicHttpBinding();
            var endpointAddress = new EndpointAddress("http://localhost:62314/CatalogService.svc");
            ICatalogService service = new CatalogServiceClient(binding, endpointAddress);

            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
