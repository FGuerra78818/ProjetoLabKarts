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
    public class PistasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAppSettingsService _settingsService;
        private readonly IWebHostEnvironment _env;

        public PistasController(AppDbContext context, IAppSettingsService settingsService, IWebHostEnvironment env)
        {
            _context = context;
            _settingsService = settingsService;
            _env = env;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, int? pageNumber)
        {
            // Carrega configurações para esta view
            var settings = await _settingsService.GetAsync("Pistas/Index");
            int pageSize = settings.RowsPerPage;
            ViewBag.SelectedCols = settings.SelectedColumns.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // 1) Prepara lista de opções de ordenação
            var sortList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Pista (A → Z)",           Value = "NomePista_Asc",           Selected = sortOrder == "NomePista_Asc" },
                new SelectListItem { Text = "Pista (Z → A)",           Value = "NomePista_Desc",          Selected = sortOrder == "NomePista_Desc" },
                new SelectListItem { Text = "Data Inserção (↑)",       Value = "DataHoraInsercao_Asc",    Selected = sortOrder == "DataHoraInsercao_Asc" },
                new SelectListItem { Text = "Data Inserção (↓)",       Value = "DataHoraInsercao_Desc",   Selected = sortOrder == "DataHoraInsercao_Desc" }
            };

            ViewBag.SortList = sortList;
            ViewBag.SelectedSort = sortOrder;
            ViewBag.CurrentFilter = searchString;

            // datas para estatísticas
            DateTime agora = DateTime.Now;
            DateTime trintaDiasAtras = agora.AddDays(-30);

            IQueryable<SessaoKart> sessoesQuery = _context.SessoesKart.AsQueryable();

            // Filtra pela pesquisa (antes do GroupBy)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                sessoesQuery = sessoesQuery
                    .Where(s => s.NomePista.Contains(searchString));
            }

            // Agrupa e projeta
            var agrupadas = sessoesQuery
                .GroupBy(s => s.NomePista)
                .Select(g => new SessaoKart
                {
                    NomePista = g.Key,
                    DataHoraInsercao = g.Max(s => s.DataHoraInsercao),
                    DataHora = g.Max(s => s.DataHora),
                    NumeroVoltas = g.Sum(s => s.NumeroVoltas),
                    MelhorVolta = g.Min(s => s.MelhorVolta),
                    VelocidadeMax = g.Max(s => s.VelocidadeMax)
                });


            // Aplica ordenação conforme opção
            switch (sortOrder)
            {
                case "NomePista_Asc":
                    agrupadas = agrupadas.OrderBy(s => s.NomePista);
                    break;
                case "NomePista_Desc":
                    agrupadas = agrupadas.OrderByDescending(s => s.NomePista);
                    break;
                case "DataHoraInsercao_Asc":
                    agrupadas = agrupadas.OrderBy(s => s.DataHoraInsercao);
                    break;
                case "DataHoraInsercao_Desc":
                    agrupadas = agrupadas.OrderByDescending(s => s.DataHoraInsercao);
                    break;
                default:
                    agrupadas = agrupadas.OrderBy(s => s.NomePista);
                    break;
            }

            // 7) Paginação
            var pagedList = await PaginatedList<SessaoKart>
                .CreateAsync(agrupadas.AsNoTracking(), pageNumber ?? 1, pageSize);

            var sessoesAgrupadas = await agrupadas.ToListAsync();
            ViewBag.numeroDePistas = sessoesAgrupadas.Count;
            ViewBag.numeroDePistasUltimoMes = sessoesAgrupadas
                .Count(s => s.DataHoraInsercao >= trintaDiasAtras);

            return View(pagedList);
        }

        // GET: Pistas/Details/5
        public async Task<IActionResult> Details(string id, string sortOrder, string searchString, int? pageNumber)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound("Nome da pista não especificado.");

            var settings = await _settingsService.GetAsync("Pistas/Details");

            var selectedCols = settings.SelectedColumns.Split(',', StringSplitOptions.RemoveEmptyEntries);
            ViewBag.SelectedCols = selectedCols;

            // 1) Prepara as opções de ordenação
            var sortList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Piloto (A → Z)",       Value = "NomePiloto_Asc",      Selected = sortOrder == "NomePiloto_Asc" },
                new SelectListItem { Text = "Piloto (Z → A)",       Value = "NomePiloto_Desc",     Selected = sortOrder == "NomePiloto_Desc" },
                new SelectListItem { Text = "Data (↑)",             Value = "DataHoraInsercao_Asc",        Selected = sortOrder == "DataHoraInsercao_Asc" },
                new SelectListItem { Text = "Data (↓)",             Value = "DataHoraInsercao_Desc",       Selected = sortOrder == "DataHoraInsercao_Desc" }
            };
            ViewBag.SortList = sortList;
            ViewBag.SelectedSort = sortOrder;
            ViewBag.PistaSelecionada = id;
            ViewBag.CurrentFilter = searchString;

            // 2) Inicia a query filtrada
            IQueryable<SessaoKart> query = _context.SessoesKart
                .Where(s => s.NomePista == id);

            // 3) Aplica filtro de pesquisa (antes da ordenação)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(s => s.NomePiloto.Contains(searchString));
            }

            // 4) Aplica ordenação conforme escolha
            switch (sortOrder)
            {
                case "NomePiloto_Asc":
                    query = query.OrderBy(s => s.NomePiloto);
                    break;
                case "NomePiloto_Desc":
                    query = query.OrderByDescending(s => s.NomePiloto);
                    break;
                case "DataHoraInsercao_Asc":
                    query = query.OrderBy(s => s.DataHoraInsercao);
                    break;
                case "DataHoraInsercao_Desc":
                    query = query.OrderByDescending(s => s.DataHoraInsercao);
                    break;
                default:
                    // padrão: data mais recente primeiro
                    query = query.OrderByDescending(s => s.DataHoraInsercao);
                    break;
            }

            // 7) Paginação
            int pageSize = settings.RowsPerPage;
            var pagedList = await PaginatedList<SessaoKart>
                .CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize);

            // 4) Materializa
            var sessoes = await query.ToListAsync();

            return View(pagedList);
        }

        // GET /Pilotos/DownloadPistas/{ficheiro}
        [HttpGet("DownloadPistas/{ficheiro}")]
        public async Task<IActionResult> DownloadPistas(string ficheiro)
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

        // GET: Pistas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pistas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeFicheiro,NomePiloto,NomePista,NumeroVoltas,MelhorVolta,DataHora,CoordenadaLat,CoordenadaLong,VelocidadeMax")] SessaoKart sessaoKart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sessaoKart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sessaoKart);
        }

        // GET: Pistas/Edit/5
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

        // POST: Pistas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("NomeFicheiro,NomePiloto,NomePista,NumeroVoltas,MelhorVolta,DataHora,CoordenadaLat,CoordenadaLong,VelocidadeMax")] SessaoKart sessaoKart)
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

        // GET: Pistas/Delete/5
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

        // POST: Pistas/Delete/5
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
