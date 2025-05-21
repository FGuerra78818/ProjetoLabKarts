using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjetoLabKarts.Models;
public class SessaoKart
{
    [Key]
    [Display(Name = "Nome do Ficheiro")]
    public string NomeFicheiro { get; set; }

    [Display(Name = "Piloto")]
    public string NomePiloto { get; set; } = string.Empty;

    [Display(Name = "Pista")]
    public string NomePista { get; set; } = string.Empty;

    [Display(Name = "Número de Voltas")]
    public int NumeroVoltas { get; set; }

    [Display(Name = "Melhor Volta")]
    public string MelhorVolta { get; set; } = string.Empty; // formato "mm:ss.fff"

    [Display(Name = "Data e Hora")]
    public DateTime DataHora { get; set; }

    [Display(Name = "Latitude")]
    public double CoordenadaLat { get; set; }

    [Display(Name = "Longitude")]
    public double CoordenadaLong { get; set; }

    [Display(Name = "Velocidade Máxima")]
    public double VelocidadeMax { get; set; }

    [Display(Name = "Data e Hora Inserção")]
    public DateTime DataHoraInsercao { get; set; }

    [Display(Name = "Numero da Melhor Volta")]
    public int NumeroMelhorVolta { get; set; }
}
