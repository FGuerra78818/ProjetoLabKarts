using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoLabKarts.Data;
using ProjetoLabKarts.Interop;
using ProjetoLabKarts.Models;

namespace ProjetoLabKarts.Controllers
{
    public class AnalisadorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AnalisadorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string ficheiros)
        {
            if (string.IsNullOrWhiteSpace(ficheiros))
                return BadRequest("Nenhum ficheiro foi selecionado.");

            // 1) Fica com os nomes limpos e sem entradas vazias
            var nomes = ficheiros
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .ToList();

            // 2) Busca todas as sessões cujos nomes coincidam
            List<SessaoKart> sessoes = await _context.SessoesKart
                .Where(s => nomes.Contains(s.NomeFicheiro))
                .ToListAsync();

            if (!sessoes.Any())
                return NotFound("Ficheiro(s) não encontrado(s).");

            // 3) Estatística total de voltas
            int numeroDeVoltas = sessoes.Sum(s => s.NumeroVoltas);

            // 4) Melhor volta geral (string)
            string melhorVoltaStr = sessoes.Min(s => s.MelhorVolta);

            // 5) Converte essa melhor volta para TimeSpan
            TimeSpan melhorVolta = TimeSpan.Zero;
            TimeSpan.TryParseExact(melhorVoltaStr, @"m\:ss\.fff", CultureInfo.InvariantCulture, out melhorVolta);

            // 6) Prepara valores por defeito para as diferenças
            string diffMelhorVolta = "00:00:000";
            double velocidadeMax = sessoes.Max(s => s.VelocidadeMax);
            double diffVelocidade = 0;

            // 7) Só calcula diferenças se houver >= 2 ficheiros selecionados
            if (sessoes.Count >= 2)
            {
                // -- Melhor volta dos "outros" (2.º melhor)
                var ordenadoPorVolta = sessoes
                    .OrderBy(s => TimeSpan.ParseExact(s.MelhorVolta, @"m\:ss\.fff", CultureInfo.InvariantCulture))
                    .ToList();

                TimeSpan segundaMelhor = TimeSpan.ParseExact(
                    ordenadoPorVolta[1].MelhorVolta,
                    @"m\:ss\.fff",
                    CultureInfo.InvariantCulture
                );

                diffMelhorVolta = (segundaMelhor - melhorVolta)
                    .ToString(@"m\:ss\.fff");

                // -- Diferença de velocidade máxima
                var ordenadoPorVelocidade = sessoes
                    .OrderByDescending(s => s.VelocidadeMax)
                    .ToList();

                diffVelocidade = ordenadoPorVelocidade[0].VelocidadeMax
                               - ordenadoPorVelocidade[1].VelocidadeMax;
            }

            // 
            var channelsByFile = new Dictionary<string, List<ChannelData>>();
            foreach (var sessao in sessoes)
            {
                var fullPath = Path.Combine(_env.WebRootPath, "Uploads", sessao.NomeFicheiro);
                var canais = XRKReader.LoadAllChannelData(fullPath);
                channelsByFile[sessao.NomeFicheiro] = canais;
            }

            // Passa tudo para a View
            ViewBag.numeroDeVoltas = numeroDeVoltas;
            ViewBag.melhorVolta = melhorVoltaStr;
            ViewBag.diferencaMelhorVoltaDosOutrosFicheiros = diffMelhorVolta;
            ViewBag.velocidadeMaxima = velocidadeMax;
            ViewBag.diferencaVelocidadeMaximaDosOutrosFicheiros = diffVelocidade.ToString("F2");
            ViewBag.ChannelsByFile = channelsByFile;

            return View(sessoes);
        }


        private bool SessaoKartExists(string id)
        {
            return _context.SessoesKart.Any(e => e.NomeFicheiro == id);
        }
    }
}
