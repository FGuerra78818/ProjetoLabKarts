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
}