using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoLabKarts.Models;

public class AppSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ViewName { get; set; } = string.Empty; // ex: "Sessao/Index"

    [Display(Name = "Linhas por página (tabelas)")]
    public int RowsPerPage { get; set; } = 10;

    [Display(Name = "Colunas selecionadas")]
    public string SelectedColumns { get; set; } = string.Empty; // ex: "NomePiloto,NomePista,DataHora"

    [Display(Name = "Gráficos selecionados")]
    public string SelectedGraphs { get; set; } = string.Empty;

    // Propriedade auxiliar para a view
    public IEnumerable<string> SelectedGraphsList
        => string.IsNullOrEmpty(SelectedGraphs)
           ? Array.Empty<string>()
           : SelectedGraphs.Split(',', StringSplitOptions.RemoveEmptyEntries);

    [Display(Name = "ProgressBar selecionadas")]
    public string SelectedProgressBars { get; set; } = string.Empty;

    // Propriedade auxiliar para a view
    public IEnumerable<string> SelectedProgressBarsList
        => string.IsNullOrEmpty(SelectedProgressBars)
           ? Array.Empty<string>()
           : SelectedProgressBars.Split(',', StringSplitOptions.RemoveEmptyEntries);
}