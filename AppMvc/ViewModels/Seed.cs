using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AppMvc.ViewModels
{
	public class Seed
    {
        
        public int NrOfGroups { get; set; }
        
        [BindProperty]
        [Required(ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItemsToSeed { get; set; } = 100;
        
        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;
    }
}

