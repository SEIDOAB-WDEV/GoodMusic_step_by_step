using Microsoft.AspNetCore.Mvc.RazorPages;
namespace AppRazor.Pages
{
    public class AlbumCarouselModel : PageModel
    {
        readonly ILogger<AlbumCarouselModel> _logger;

        public AlbumCarouselModel(ILogger<AlbumCarouselModel> logger)
        {
            _logger = logger;
        }
    }
}
