using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using ProjetoLabKarts.Data;
using ProjetoLabKarts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoLabKarts.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly AppDbContext _db;

    public static readonly Dictionary<string, string[]> DefaultColumnsPerView = new()
    {
        ["Sessao/Index"] = new[] { "NomePiloto", "NomePista", "NumeroVoltas", "MelhorVolta", "DataHora" },
        ["Pilotos/Index"] = new[] { "NomePiloto", "NumeroVoltas", "DataHoraInsercao" },
        ["Pilotos/Details"] = new[] { "NomePista", "NumeroVoltas", "MelhorVolta", "VelocidadeMax", "DataHoraInsercao" },
        ["Pistas/Index"] = new[] { "NomePista", "NumeroVoltas", "MelhorVolta", "VelocidadeMax", "CoordenadaLat", "CoordenadaLong"},
        ["Pistas/Details"] = new[] { "NomePiloto", "NumeroVoltas", "MelhorVolta", "VelocidadeMax" }
    };

    public static readonly Dictionary<string, string[]> DefaultGraphsPerView = new Dictionary<string, string[]>
    {
        ["Analisador/Index"] = new[] { "Water Temp", "Internal Battery", "RPM", "GPS Speed", "GPS Nsat", "GPS LatAcc", "GPS LonAcc", "GPS Slope", "GPS PosAccuracy", "GPS SpdAccuracy", "GPS Latitude", "GPS Longitude", "GPS Radius", "GPS East", "GPS North", "GPS HeadGyro" }
    };

    public static readonly Dictionary<string, string[]> DefaultProgressBarsPerView = new Dictionary<string, string[]>
    {
        ["Analisador/Index"] = new[] { "Water Temp", "Internal Battery", "RPM", "GPS Speed", "GPS Nsat", "GPS LatAcc", "GPS LonAcc", "GPS Slope", "GPS PosAccuracy", "GPS SpdAccuracy", "GPS Latitude", "GPS Longitude", "GPS Radius", "GPS East", "GPS North", "GPS HeadGyro" }
    };

    public static readonly string[] DefaultColumns = new[]
    {
        "NomeFicheiro","NomePiloto","NomePista","NumeroVoltas",
        "MelhorVolta","DataHora","CoordenadaLat","CoordenadaLong",
        "VelocidadeMax","DataHoraInsercao","NumeroMelhorVolta"
    };

    public AppSettingsService(AppDbContext db) => _db = db;

    public async Task<AppSettings> GetAsync(string viewName)
    {
        var settings = await _db.AppSettings.FirstOrDefaultAsync(s => s.ViewName == viewName);
        if (settings == null)
        {
            var cols = DefaultColumnsPerView.TryGetValue(viewName, out var def)
                       ? def
                       : DefaultColumns;
            settings = new AppSettings
            {
                ViewName = viewName,
                RowsPerPage = 10,
                SelectedColumns = string.Join(',', cols)
            };
            _db.AppSettings.Add(settings);
            await _db.SaveChangesAsync();
            return settings;
        }

        if (string.IsNullOrWhiteSpace(settings.SelectedColumns))
        {
            var cols = DefaultColumnsPerView.TryGetValue(viewName, out var def)
                       ? def
                       : DefaultColumns;
            settings.SelectedColumns = string.Join(',', cols);
            _db.AppSettings.Update(settings);
            await _db.SaveChangesAsync();
        }

        return settings;
    }

    public async Task UpdateAsync(string viewName, int rowsPerPage, IEnumerable<string> columns, IEnumerable<string> graphs, IEnumerable<string> progressbars)
    {
        var settings = await GetAsync(viewName);
        settings.RowsPerPage = rowsPerPage;
        settings.SelectedColumns = string.Join(',', columns);
        settings.SelectedProgressBars = string.Join(',', progressbars);
        settings.SelectedGraphs = string.Join(',', graphs);
        _db.AppSettings.Update(settings);
        await _db.SaveChangesAsync();
    }
}
