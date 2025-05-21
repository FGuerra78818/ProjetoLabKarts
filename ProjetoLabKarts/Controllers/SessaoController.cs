using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoLabKarts.Data;
using ProjetoLabKarts.Models;
using ProjetoLabKarts.Services;

namespace ProjetoLabKarts.Controllers
{
    public class SessaoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAppSettingsService _settingsService;
        private readonly IWebHostEnvironment _env;

        public SessaoController(AppDbContext context, IAppSettingsService settingsService, IWebHostEnvironment env)
        {
            _context = context;
            _settingsService = settingsService;
            _env = env;
        }

        // GET: Sessao
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            var settings = await _settingsService.GetAsync("Sessao/Index");
            int pageSize = settings.RowsPerPage;
            ViewBag.SelectedCols = settings.SelectedColumns.Split(',', StringSplitOptions.RemoveEmptyEntries);

            DateTime agora = DateTime.Now;
            DateTime trintaDiasAtras = agora.AddDays(-30);

            // Prepara as opções e marca a selecionada
            var sortList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Piloto (A → Z)",            Value = "NomePiloto_Asc",         Selected = sortOrder == "NomePiloto_Asc" },
                new SelectListItem { Text = "Piloto (Z → A)",            Value = "NomePiloto_Desc",        Selected = sortOrder == "NomePiloto_Desc" },
                new SelectListItem { Text = "Pista (A → Z)",            Value = "NomePista_Asc",         Selected = sortOrder == "NomePista_Asc" },
                new SelectListItem { Text = "Pista (Z → A)",            Value = "NomePista_Desc",        Selected = sortOrder == "NomePista_Desc" },
                new SelectListItem { Text = "Data Inserção (↑)",         Value = "DataHoraInsercao_Asc",   Selected = sortOrder == "DataHoraInsercao_Asc" },
                new SelectListItem { Text = "Data Inserção (↓)",         Value = "DataHoraInsercao_Desc",  Selected = sortOrder == "DataHoraInsercao_Desc" }
            };

            ViewBag.SortList = sortList;
            ViewBag.SelectedSort = sortOrder;
            ViewBag.CurrentFilter = searchString;

            // Inicia query sem ordenar ainda
            IQueryable<SessaoKart> sessoesQuery = _context.SessoesKart;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                sessoesQuery = sessoesQuery
                    .Where(s => s.NomePiloto.Contains(searchString));
            }

            // Aplica ordenação
            switch (sortOrder)
            {
                case "NomePiloto_Asc":
                    sessoesQuery = sessoesQuery.OrderBy(s => s.NomePiloto);
                    break;
                case "NomePiloto_Desc":
                    sessoesQuery = sessoesQuery.OrderByDescending(s => s.NomePiloto);
                    break;
                case "NomePista_Asc":
                    sessoesQuery = sessoesQuery.OrderBy(s => s.NomePista);
                    break;
                case "NomePista_Desc":
                    sessoesQuery = sessoesQuery.OrderByDescending(s => s.NomePista);
                    break;
                case "DataHoraInsercao_Asc":
                    sessoesQuery = sessoesQuery.OrderBy(s => s.DataHoraInsercao);
                    break;
                case "DataHoraInsercao_Desc":
                    sessoesQuery = sessoesQuery.OrderByDescending(s => s.DataHoraInsercao);
                    break;
                default:
                    sessoesQuery = sessoesQuery.OrderByDescending(s => s.DataHoraInsercao);
                    break;
            }

            // 7) Paginação
            var pagedList = await PaginatedList<SessaoKart>
                .CreateAsync(sessoesQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            var sessoes = await sessoesQuery.ToListAsync();

            ViewBag.numeroDeSessoes = sessoes.Count;
            ViewBag.numeroDeSessoesUltimoMes = sessoes.Count(s => s.DataHoraInsercao >= trintaDiasAtras);

            return View(pagedList);
        }

        // GET /Pilotos/DownloadSessao/{ficheiro}
        [HttpGet("DownloadSessao/{ficheiro}")]
        public async Task<IActionResult> DownloadSessao(string ficheiro)
        {
            if (string.IsNullOrWhiteSpace(ficheiro))
                return BadRequest("Nome de ficheiro não especificado.");

            // Ajusta esta pasta para onde guardas os ficheiros
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            var caminho = Path.Combine(uploadsDir, ficheiro);

            if (!System.IO.File.Exists(caminho))
                return NotFound($"Ficheiro «{ficheiro}» não encontrado.");

            var mem = new MemoryStream();
            await using (var stream = new FileStream(caminho, FileMode.Open, FileAccess.Read))
                await stream.CopyToAsync(mem);

            mem.Position = 0;
            // application/octet-stream força o "Guardar como…"
            return File(mem, "application/octet-stream", ficheiro);
        }

        // GET: Sessao/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sessaoKart = await _context.SessoesKart
                .FirstOrDefaultAsync(m => m.NomeFicheiro == id);
            if (sessaoKart == null)
            {
                return NotFound();
            }

            return View(sessaoKart);
        }

        // GET: Sessao/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sessao/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeFicheiro,NomePiloto,NomePista,NumeroVoltas,MelhorVolta,DataHora,CoordenadaLat,CoordenadaLong")] SessaoKart sessaoKart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sessaoKart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sessaoKart);
        }

        // GET: Sessao/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sessaoKart = await _context.SessoesKart.FindAsync(id);
            if (sessaoKart == null)
            {
                return NotFound();
            }
            return View(sessaoKart);
        }

        // POST: Sessao/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NomeFicheiro,NomePiloto,NomePista,NumeroVoltas,MelhorVolta,DataHora,CoordenadaLat,CoordenadaLong")] SessaoKart sessaoKart)
        {
            if (id != sessaoKart.NomeFicheiro)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sessaoKart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessaoKartExists(sessaoKart.NomeFicheiro))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sessaoKart);
        }

        // GET: Sessao/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sessaoKart = await _context.SessoesKart
                .FirstOrDefaultAsync(m => m.NomeFicheiro == id);
            if (sessaoKart == null)
            {
                return NotFound();
            }

            return View(sessaoKart);
        }

        // POST: Sessao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sessaoKart = await _context.SessoesKart.FindAsync(id);
            if (sessaoKart != null)
            {
                _context.SessoesKart.Remove(sessaoKart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessaoKartExists(string id)
        {
            return _context.SessoesKart.Any(e => e.NomeFicheiro == id);
        }
    }
}
