using Microsoft.AspNetCore.Mvc.RazorPages;
namespace AppRazor.Pages
{
    public class AlbumCardsModel : PageModel
    {
        readonly ILogger<AlbumCardsModel> _logger;

        public AlbumCardsModel(ILogger<AlbumCardsModel> logger)
        {
            _logger = logger;
        }
    }
}
