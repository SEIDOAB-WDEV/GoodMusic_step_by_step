using Microsoft.AspNetCore.Mvc.RazorPages;
using Seido.Utilities.SeedGenerator;
namespace AppRazor.Pages
{
    public record AlbumCard(string Title, string ImageUrl, string Description);
    public class AlbumCardsModel : PageModel
    {
        private SeedGenerator _seeder = new SeedGenerator();
        public int NrOfCards { get; set; }
        public List<AlbumCard> AlbumCards { get; set; } = new(); 
        readonly ILogger<AlbumCardsModel> _logger;

        public AlbumCardsModel(ILogger<AlbumCardsModel> logger)
        {
            _logger = logger;
            NrOfCards = 40; // Example value, can be set dynamically

            for (int i = 0; i < NrOfCards; i++)
            {
                AlbumCards.Add (new AlbumCard(
                    _seeder.MusicGroupName, 
                    _seeder.FromString("jupiter1.jpg, jupiter2.jpg, jupiter3.jpg, jupiter5.jpg"),
                    _seeder.LatinSentence
                ));
            }
        }
    }
}
