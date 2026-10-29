using eShopWinForms.Controllers;
using eShopWinForms.eShopServiceReference;
using System;
using System.ServiceModel;
using System.Windows.Forms;

namespace eShopWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CatalogView catalogView = new CatalogView();

            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:62314/CatalogService.svc");
            ICatalogService service = new eShopServiceReference.CatalogServiceClient(binding, endpoint);

            CatalogController catalogController = new CatalogController(service, catalogView);

            catalogController.LoadView();
            catalogView.ShowDialog();
        }
    }
}
