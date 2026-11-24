using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.ViewModels;
using Services;

namespace AppMvc.Controllers;

public class HomeController : Controller
{
    readonly ILogger<HomeController> _logger;
    readonly IAdminService _service = null;

    public HomeController(ILogger<HomeController> logger, IAdminService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public IActionResult AlbumCards()
    {
        return View();
    }

    public IActionResult AlbumCarousel()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Error { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

