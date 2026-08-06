using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopLegacyWebForms.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(ILogger<ContactModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Contact");
        }
    }
}
