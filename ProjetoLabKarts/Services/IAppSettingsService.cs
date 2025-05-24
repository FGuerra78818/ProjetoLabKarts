using NuGet.Configuration;
using ProjetoLabKarts.Models;

namespace ProjetoLabKarts.Services;

public interface IAppSettingsService
{
    Task<AppSettings> GetAsync(string viewName);
    Task UpdateAsync(string viewName, int rowsPerPage, IEnumerable<string> columns, IEnumerable<string> graphs, IEnumerable<string> progressbars);
}
