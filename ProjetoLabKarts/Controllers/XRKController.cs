using Microsoft.AspNetCore.Mvc;
using ProjetoLabKarts.Interop;
using ProjetoLabKarts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ProjetoLabKarts.Controllers
{
    public class XrkController : Controller
    {
        private readonly string _uploadPath;
        public XrkController(IWebHostEnvironment env)
        {
            _uploadPath = Path.Combine(env.WebRootPath, "Uploads");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new List<CanalXRK>());
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile arquivo, bool substituir = false)
        {
            if (arquivo == null || Path.GetExtension(arquivo.FileName).ToLower() != ".xrk")
            {
                ModelState.AddModelError("", "Apenas arquivos .xrk são permitidos.");
                return View(new List<CanalXRK>());
            }

            string nomeArquivo = Path.GetFileName(arquivo.FileName);
            string filePath = Path.Combine(_uploadPath, nomeArquivo);

            if (System.IO.File.Exists(filePath) && !substituir)
            {
                TempData["NomeArquivo"] = nomeArquivo;
                TempData["Aviso"] = "Já existe um arquivo com esse nome. Deseja substituir?";
                return RedirectToAction("ConfirmarSubstituicao");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            int idx = XRKInterop.open_file(filePath);
            if (idx <= 0)
            {
                string erro = Marshal.PtrToStringAnsi(XRKInterop.get_last_open_error());
                ModelState.AddModelError("", $"Erro ao abrir arquivo: {erro}");
                return View(new List<CanalXRK>());
            }

            int totalCanais = XRKInterop.get_channels_count(idx);
            var canais = new List<CanalXRK>();

            for (int i = 0; i < totalCanais; i++)
            {
                string nome = Marshal.PtrToStringAnsi(XRKInterop.get_channel_name(idx, i));
                int nSamples = XRKInterop.get_channel_samples_count(idx, i);
                double[] tempos = new double[nSamples];
                double[] valores = new double[nSamples];

                if (XRKInterop.get_channel_samples(idx, i, tempos, valores, nSamples) > 0)
                {
                    canais.Add(new CanalXRK
                    {
                        Nome = nome,
                        Valores = new List<double>(valores[..Math.Min(10, valores.Length)])
                    });
                }
            }

            XRKInterop.close_file_i(idx);
            return View(canais);
        }

        [HttpGet]
        public IActionResult ConfirmarSubstituicao()
        {
            ViewBag.NomeArquivo = TempData["NomeArquivo"];
            ViewBag.Aviso = TempData["Aviso"];
            return View();
        }

        [HttpPost]
        public IActionResult VerificarArquivo([FromForm] string nomeArquivo)
        {
            string filePath = Path.Combine(_uploadPath, nomeArquivo);
            bool existe = System.IO.File.Exists(filePath);
            return Json(new { existe });
        }
    }
}
