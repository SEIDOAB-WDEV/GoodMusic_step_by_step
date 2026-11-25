# Exercises: Number of music groups per genre in DataSourceInfo

The page **Data overview** (`DataSourceInfo`) shows how many music groups, albums and artists the database contains. In these exercises you add a fourth block that shows **how many (seeded) music groups there are per genre**.

You do it first in **AppRazor** (Exercise 1), and then in **AppMvc** (Exercise 2).

## Prerequisites
- You are on branch `6ex-data-overview`. The solution builds (`dotnet build GoodMusic.slnx` gives 0 errors).
- The database is running and filled with the initial data (see `readme-clr0.txt`). Use the **Seed** page if the Data overview shows 0 music groups.
- Ports when you run with `dotnet run --launch-profile http`: AppMvc `http://localhost:5204`, AppRazor `http://localhost:5183`.

## The service you will use

```csharp
Task<ResponsePageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
```

| Parameter | What to pass |
|---|---|
| `seeded` | `true`: the exercises only cover the **seeded** music groups, the ones created by the **Seed** page |
| `flat` | `true`: only the groups, without albums and artists. That is enough for the genre |
| `filter` | `null`: no filter |
| `pageNumber`, `pageSize` | Which page to read |

The result `ResponsePageDto<IMusicGroup>` has `PageItems` (the groups on the page) and `DbItemsCount` (the number of groups in the database, whatever the page size).

## The pattern

You do not know how many groups there are, so you read in two steps:

1. Read **one** group, only to get `DbItemsCount`.
2. Read **all** groups, with `DbItemsCount` as the page size.
3. Group them by genre with **Linq**, and count each group.

> **Hint: the Linq pattern**
> ```csharp
> //1. read one group
> var first = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, 1);
>
> //2. read all groups
> var all = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, first.DbItemsCount);
>
> //3. group by genre, and count
> var genres = all.PageItems
>     .GroupBy(mg => mg.Genre)
>     .Select(g => new GenreCount(g.Key, g.Count()))
>     .OrderBy(gc => gc.Genre)
>     .ToList();
> ```
> `GroupBy` gives one group per genre. `g.Key` is the genre, and `g.Count()` is the number of music groups in it.

If there are no seeded groups, `DbItemsCount` is 0. The second read then gets `pageSize` 0 and returns an empty list, and `GroupBy` gives an empty result. Nothing fails, but the page should write `none` instead of showing nothing.

---

# Exercise 1: AppRazor

## Step 1: The page model

Open `AppRazor/Pages/DataSourceInfo.cshtml.cs`. Make these changes:

1. Add a type `GenreCount` with a genre and a count (a `record` is ideal).
2. Add a property `GenreCounts` of the type `List<GenreCount>`.
3. Inject `IMusicGroupsService` through the constructor. (It is already registered in `Program.cs`.)
4. Add a private method `GenreCountsAsync()` that uses the pattern above and returns the list.
5. In `OnGetAsync`, fill `GenreCounts` by calling the method.

`MusicGenre` and `IMusicGroup` are in the namespace `Models.Interfaces`.

<details>
<summary>Solution: <code>AppRazor/Pages/DataSourceInfo.cshtml.cs</code></summary>

**File: `AppRazor/Pages/DataSourceInfo.cshtml.cs`**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services;
namespace AppRazor.Pages
{
	//Number of music groups of one genre
	public record GenreCount(MusicGenre Genre, int Count);

	public class DataSourceInfoModel : PageModel
    {
        public Models.DTO.GstUsrInfoAllDto Info { get; set; }
        public List<GenreCount> GenreCounts { get; set; }

        readonly ILogger<DataSourceInfoModel> _logger;
        readonly IAdminService _service = null;
        readonly IMusicGroupsService _mgService = null;

        public DataSourceInfoModel(ILogger<DataSourceInfoModel> logger, IAdminService service, IMusicGroupsService mgService)
        {
            _logger = logger;
            _service = service;
            _mgService = mgService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Info = (await _service.GuestInfoAsync()).Item;
            GenreCounts = await GenreCountsAsync();
            return Page();
        }

        private async Task<List<GenreCount>> GenreCountsAsync()
        {
            //First read just one seeded group. We are only interested in DbItemsCount, the number of groups in the database
            var first = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, 1);

            //Then read all seeded groups in one go, using DbItemsCount as the page size
            var all = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, first.DbItemsCount);

            //Group the music groups by genre with Linq
            return all.PageItems
                .GroupBy(mg => mg.Genre)
                .Select(g => new GenreCount(g.Key, g.Count()))
                .OrderBy(gc => gc.Genre)
                .ToList();
        }
    }
}
```

</details>

## Step 2: The page

Open `AppRazor/Pages/DataSourceInfo.cshtml`. Add a fourth block **after** the Artists block, inside the same row (`<div class="col d-flex align-items-start">`). Show one line per genre. Write `none` when the list is empty. Reuse the icon `#boombox`.

<details>
<summary>Solution: the new block</summary>

Add after the Artists block (before the closing `</div>` of the row):

```razor
    <div class="col d-flex align-items-start">
        <svg class="bi text-body-secondary flex-shrink-0 me-3" width="1.75em" height="1.75em"><use xlink:href="#boombox"/></svg>
        <div>
        <h3 class="fw-bold mb-0 fs-4 text-body-emphasis">Groups per genre</h3>
        @if (Model.GenreCounts.Count == 0)
        {
            <p>none</p>
        }
        @foreach (var gc in Model.GenreCounts)
        {
            <p class="mb-0">@gc.Count @gc.Genre</p>
        }
        </div>
    </div>
```

</details>

## Step 3: Test and commit

```bash
dotnet build GoodMusic.slnx
cd AppRazor && dotnet run --launch-profile http
```

Open **Data overview**. You should see a block **Groups per genre**, with one line per genre (for example `26 Rock`). The numbers add up to the seeded number in the **Music groups** block.

Then go to the **Seed** page, seed another number of groups, and return to **Data overview**: the numbers change, and still add up. If you remove the seeds, the block shows `none`.

```bash
git add AppRazor
git commit -m "Show number of music groups per genre in AppRazor DataSourceInfo"
```

---

# Exercise 2: AppMvc

Do the same in AppMvc. The pattern is the same. Only the places differ:

| | AppRazor | AppMvc |
|---|---|---|
| Reading the data | `DataSourceInfoModel.OnGetAsync` | `HomeController.DataSourceInfo()` |
| Carrying the result | Property on the page model | Property on the view model `ViewModels/DataSourceInfo.cs` |
| The page | `Pages/DataSourceInfo.cshtml` | `Views/Home/DataSourceInfo.cshtml` |

## Step 1: The view model

Open `AppMvc/ViewModels/DataSourceInfo.cs`. Add the type `GenreCount` and the list property `GenreCounts`.

<details>
<summary>Solution: <code>AppMvc/ViewModels/DataSourceInfo.cs</code></summary>

**File: `AppMvc/ViewModels/DataSourceInfo.cs`**
```csharp
using System.ComponentModel.DataAnnotations;
using AppMvc.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Interfaces;

namespace AppMvc.ViewModels
{
	//Number of music groups of one genre
	public record GenreCount(MusicGenre Genre, int Count);

	public class DataSourceInfo
    {
        public GstUsrInfoAllDto Info { get; set; }
        public List<GenreCount> GenreCounts { get; set; }
    }
}
```

</details>

## Step 2: The controller

Open `AppMvc/Controllers/HomeController.cs`. Inject `IMusicGroupsService`, add the private method `GenreCountsAsync()` with the pattern, and fill the view model in `DataSourceInfo()`.

<details>
<summary>Solution: <code>AppMvc/Controllers/HomeController.cs</code></summary>

**File: `AppMvc/Controllers/HomeController.cs`**
```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.ViewModels;
using Services;

namespace AppMvc.Controllers;

public class HomeController : Controller
{
    readonly ILogger<HomeController> _logger;
    readonly IAdminService _service = null;
    readonly IMusicGroupsService _mgService = null;

    public HomeController(ILogger<HomeController> logger, IAdminService service, IMusicGroupsService mgService)
    {
        _logger = logger;
        _service = service;
        _mgService = mgService;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> DataSourceInfo()
    {
        var vm = new DataSourceInfo()
        {
            Info = (await _service.GuestInfoAsync()).Item,
            GenreCounts = await GenreCountsAsync()
        };
        return View(vm);
    }

    private async Task<List<GenreCount>> GenreCountsAsync()
    {
        //First read just one seeded group. We are only interested in DbItemsCount, the number of groups in the database
        var first = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, 1);

        //Then read all seeded groups in one go, using DbItemsCount as the page size
        var all = await _mgService.ReadMusicGroupsAsync(true, true, null, 0, first.DbItemsCount);

        //Group the music groups by genre with Linq
        return all.PageItems
            .GroupBy(mg => mg.Genre)
            .Select(g => new GenreCount(g.Key, g.Count()))
            .OrderBy(gc => gc.Genre)
            .ToList();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Error { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
```

</details>

## Step 3: The view

Open `AppMvc/Views/Home/DataSourceInfo.cshtml` and add the same fourth block as in AppRazor, after the Artists block. The list is `Model.GenreCounts`.

<details>
<summary>Solution: the new block</summary>

The markup is the same as in AppRazor:

```razor
    <div class="col d-flex align-items-start">
        <svg class="bi text-body-secondary flex-shrink-0 me-3" width="1.75em" height="1.75em"><use xlink:href="#boombox"/></svg>
        <div>
        <h3 class="fw-bold mb-0 fs-4 text-body-emphasis">Groups per genre</h3>
        @if (Model.GenreCounts.Count == 0)
        {
            <p>none</p>
        }
        @foreach (var gc in Model.GenreCounts)
        {
            <p class="mb-0">@gc.Count @gc.Genre</p>
        }
        </div>
    </div>
```

</details>

## Step 4: Test and commit

```bash
dotnet build GoodMusic.slnx
cd AppMvc && dotnet run --launch-profile http
```

Open **Data overview**. The block **Groups per genre** should show the same numbers as in AppRazor.

```bash
git add AppMvc
git commit -m "Show number of music groups per genre in AppMvc DataSourceInfo"
```

---

## Done when
- [ ] AppRazor and AppMvc both show **Groups per genre** on the Data overview page.
- [ ] The genre counts add up to the seeded number in the **Music groups** block.
- [ ] `dotnet build GoodMusic.slnx` gives 0 errors.
