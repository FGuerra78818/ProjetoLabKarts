using Microsoft.AspNetCore.Mvc;
using ProjetoLabKarts.Models;
using ProjetoLabKarts.Services;
using System.Threading.Tasks;
using System.Linq;

namespace ProjetoLabKarts.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IAppSettingsService _settingsService;
        private static readonly string[] ViewsList = new[]
        {
            "Sessao/Index",
            "Pilotos/Index",
            "Pilotos/Details",
            "Pistas/Index",
            "Pistas/Details",
            "Analisador/Index"
        };

        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            ["Sessao/Index"] = "Sessões – Lista",
            ["Pilotos/Index"] = "Pilotos – Lista",
            ["Pilotos/Details"] = "Pilotos – Detalhes",
            ["Pistas/Index"] = "Pistas – Lista",
            ["Pistas/Details"] = "Pistas – Detalhes",
            ["Analisador/Index"] = "Analisador - Index"
        };

        public SettingsController(IAppSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Lista de views para configurar
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ViewsList = ViewsList;
            ViewBag.DisplayNames = DisplayNames;

            return View();
        }

        // Edita configurações de uma view específica
        [HttpGet]
        public async Task<IActionResult> Edit(string viewName)
        {
            if (string.IsNullOrEmpty(viewName) || !ViewsList.Contains(viewName))
                viewName = ViewsList.First();

            var settings = await _settingsService.GetAsync(viewName);
            ViewBag.ViewsList = ViewsList;
            ViewBag.DisplayNames = DisplayNames;
            ViewBag.GraphsList = AppSettingsService.DefaultGraphsPerView.GetValueOrDefault(viewName, Array.Empty<string>());
            ViewBag.ProgressBarsList = AppSettingsService.DefaultProgressBarsPerView.GetValueOrDefault(viewName, Array.Empty<string>());
            ViewBag.SelectedCols = settings.SelectedColumns.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string viewName, AppSettings model, string[] columns, string[] graphs, string[] progressbars)
        {
            if (!ViewsList.Contains(viewName))
                return BadRequest();

            await _settingsService.UpdateAsync(viewName, model.RowsPerPage, columns, graphs ?? Array.Empty<string>(), progressbars ?? Array.Empty<string>());
            TempData["Success"] = "Configurações salvas com sucesso.";
            return RedirectToAction(nameof(Edit), new { viewName });
        }
    }
}
