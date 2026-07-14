using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class ContactModel : PageModel
    {
        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
            Message = "Your contact page.";
        }
    }
}
