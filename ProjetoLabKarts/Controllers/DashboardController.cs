using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using ProjetoLabKarts.Interop;
using ProjetoLabKarts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ProjetoLabKarts.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjetoLabKarts.Controllers;

public class DeleteRequest
{
    public string NomeFicheiro { get; set; }
}

public class DashboardController : Controller
{
    private readonly string _uploadPath;
    private readonly SessaoKartService _sessaoService;
    private readonly AppDbContext _context;

    public DashboardController(IWebHostEnvironment env, SessaoKartService sessaoService, AppDbContext context)
    {
        _uploadPath = Path.Combine(env.WebRootPath, "Uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
        _sessaoService = sessaoService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        DateTime agora = DateTime.Now;
        DateTime trintaDiasAtras = agora.AddDays(-30);

        // Apenas carrega as sessões necessárias, já ordenadas
        List<SessaoKart> sessoes = await _context.SessoesKart
            .OrderByDescending(s => s.DataHoraInsercao)
            .ToListAsync();

        if (sessoes == null)
        {
            return NotFound();
        }

        // Filtros otimizados
        List<SessaoKart> sessoesUltimoMes = sessoes.Where(s => s.DataHoraInsercao >= trintaDiasAtras).ToList();

        // Métricas gerais
        int numeroDeVoltas = sessoes.Sum(s => s.NumeroVoltas);
        int numeroDeSessoes = sessoes.Count;
        int numeroDePistasCarregadas = sessoes.Select(s => s.NomePista).Distinct().Count();

        // Métricas último mês
        int numeroDeVoltasUltimoMes = sessoesUltimoMes.Sum(s => s.NumeroVoltas);
        int numeroDeSessoesUltimoMes = sessoesUltimoMes.Count;
        int numeroDePistasCarregadasUltimoMes = sessoesUltimoMes.Select(s => s.NomePista).Distinct().Count();

        // ViewBag
        ViewBag.numeroDeVoltas = numeroDeVoltas;
        ViewBag.numeroDeVoltasUltimoMes = numeroDeVoltasUltimoMes;
        ViewBag.numeroDeSessoes = numeroDeSessoes;
        ViewBag.numeroDeSessoesUltimoMes = numeroDeSessoesUltimoMes;
        ViewBag.numeroDePistasCarregadas = numeroDePistasCarregadas;
        ViewBag.numeroDePistasCarregadasUltimoMes = numeroDePistasCarregadasUltimoMes;

        // Apenas as 6 sessões mais recentes para exibição
        List<SessaoKart> sessoesRecentes = sessoes.Take(6).ToList();

        return View(sessoesRecentes);
    }


    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile arquivo)
    {
        if (arquivo == null || Path.GetExtension(arquivo.FileName).ToLower() != ".xrk")
        {
            return BadRequest("Ficheiro inválido.");
        }

        var filePath = Path.Combine(_uploadPath, Path.GetFileName(arquivo.FileName));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        await _sessaoService.ProcessarFicheiroXRKAsync(filePath);

        return Ok("Ficheiro carregado com sucesso!");
    }

    [HttpGet]
    public IActionResult CheckFileExists(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);
        bool exists = System.IO.File.Exists(filePath);
        return Json(new { exists });
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeFicheiro))
            return BadRequest("ID inválido.");

        var sessao = await _context.SessoesKart.FindAsync(request.NomeFicheiro);
        if (sessao == null)
            return NotFound("Sessão não encontrada.");

        _context.SessoesKart.Remove(sessao);
        await _context.SaveChangesAsync();

        var filePath = Path.Combine(_uploadPath, request.NomeFicheiro);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        return Ok("Sessão eliminada com sucesso.");
    }
}
