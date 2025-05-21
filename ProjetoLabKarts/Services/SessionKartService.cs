using ProjetoLabKarts.Interop;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Linq;
using ProjetoLabKarts.Models;
using ProjetoLabKarts.Data;
using Microsoft.EntityFrameworkCore;

public class SessaoKartService
{
    private readonly AppDbContext _context;

    public SessaoKartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessarFicheiroXRKAsync(string caminhoCompleto)
    {
        int idxf = XRKInterop.open_file(caminhoCompleto);

        if (idxf < 0)
        {
            string erro = Marshal.PtrToStringAnsi(XRKInterop.get_last_open_error());
            throw new Exception($"Erro ao abrir o ficheiro XRK: {erro}");
        }

        try
        {
            string nomeFicheiro = Path.GetFileName(caminhoCompleto);

            // Verifica se já existe e remove se necessário
            var existente = await _context.SessoesKart
                .FirstOrDefaultAsync(s => s.NomeFicheiro == nomeFicheiro);

            if (existente != null)
            {
                _context.SessoesKart.Remove(existente);
                await _context.SaveChangesAsync();
            }

            int numVoltas = XRKInterop.get_laps_count(idxf);
            string nomePiloto = Marshal.PtrToStringAnsi(XRKInterop.get_racer_name(idxf));
            string nomePista = Marshal.PtrToStringAnsi(XRKInterop.get_track_name(idxf));
            string dataHoraStr = Marshal.PtrToStringAnsi(XRKInterop.get_date_and_time(idxf));
            DateTime dataHoraInsercao = DateTime.Now;
            (double coordenadalat, double coordenadalong) = GetCoordenadasMediasDaVolta(idxf, 0);

            DateTime dataHora = DateTime.TryParse(dataHoraStr, out var data)
                ? data
                : DateTime.MinValue;

            // Melhor volta
            double menorTempo = double.MaxValue;
            int indiceMelhorVolta = -1;

            for (int i = 0; i < numVoltas; i++)
            {
                XRKInterop.get_lap_info(idxf, i, out _, out double duracao);
                if (duracao > 0 && duracao < menorTempo)
                {
                    menorTempo = duracao;
                    indiceMelhorVolta = i;
                }
            }

            TimeSpan melhorVolta = TimeSpan.FromSeconds(menorTempo);
            string melhorVoltaFormatada = melhorVolta.ToString(@"m\:ss\.fff", CultureInfo.InvariantCulture);

            double velocidadeMax = 0;
            int gpsChannelCount = XRKInterop.get_GPS_channels_count(idxf);
            int idxVelocidade = -1;

            for (int i = 0; i < gpsChannelCount; i++)
            {
                string nome = Marshal.PtrToStringAnsi(XRKInterop.get_GPS_channel_name(idxf, i)).ToLower();
                string unidade = Marshal.PtrToStringAnsi(XRKInterop.get_GPS_channel_units(idxf, i)).ToLower();

                Console.WriteLine($"Canal GPS [{i}]: {nome} | Unidade: {unidade}");

                if (nome.Contains("speed") && !nome.Contains("vertical"))
                {
                    idxVelocidade = i;
                    break;
                }
            }

            if (idxVelocidade >= 0)
            {
                int numSamples = XRKInterop.get_GPS_channel_samples_count(idxf, idxVelocidade);

                if (numSamples > 0)
                {
                    double[] tempos = new double[numSamples];
                    double[] valores = new double[numSamples];

                    int sucesso = XRKInterop.get_GPS_channel_samples(idxf, idxVelocidade, tempos, valores, numSamples);
                    if (sucesso >= 0)
                    {
                        // Conversão para km/h se necessário
                        double maxRaw = valores.Max();
                        velocidadeMax = Math.Round(maxRaw * 3.6, 2);
                    }
                    else
                    {
                        Console.WriteLine("Erro ao ler as amostras GPS!");
                    }
                }
            }


            var sessao = new SessaoKart
            {
                NomeFicheiro = nomeFicheiro,
                NomePiloto = nomePiloto,
                NomePista = nomePista,
                NumeroVoltas = numVoltas,
                MelhorVolta = melhorVoltaFormatada,
                NumeroMelhorVolta = indiceMelhorVolta,
                DataHora = dataHora,
                CoordenadaLat = coordenadalat,
                CoordenadaLong = coordenadalong,
                DataHoraInsercao = dataHoraInsercao,
                VelocidadeMax = velocidadeMax
            };

            _context.SessoesKart.Add(sessao);
            await _context.SaveChangesAsync();
        }
        finally
        {
            XRKInterop.close_file_i(idxf);
        }
    }

    public (double latitudeMedia, double longitudeMedia) GetCoordenadasMediasDaVolta(int idxf, int idxVolta)
    {
        int gpsChannelCount = XRKInterop.get_GPS_channels_count(idxf);

        int idxLatitude = -1;
        int idxLongitude = -1;

        // Procurar canais de latitude e longitude
        for (int i = 0; i < gpsChannelCount; i++)
        {
            string nome = Marshal.PtrToStringAnsi(XRKInterop.get_GPS_channel_name(idxf, i)).ToLower();

            if (nome.Contains("latitude")) idxLatitude = i;
            else if (nome.Contains("longitude")) idxLongitude = i;
        }

        if (idxLatitude < 0 || idxLongitude < 0)
            throw new Exception("Canais de Latitude ou Longitude não encontrados no ficheiro.");

        // Obter contagem de amostras da volta
        int sampleCountLat = XRKInterop.get_lap_GPS_channel_samples_count(idxf, idxVolta, idxLatitude);
        int sampleCountLng = XRKInterop.get_lap_GPS_channel_samples_count(idxf, idxVolta, idxLongitude);

        if (sampleCountLat == 0 || sampleCountLng == 0)
            throw new Exception("Sem dados GPS nesta volta.");

        double[] latTimes = new double[sampleCountLat];
        double[] latValues = new double[sampleCountLat];

        double[] lngTimes = new double[sampleCountLng];
        double[] lngValues = new double[sampleCountLng];

        XRKInterop.get_lap_GPS_channel_samples(idxf, idxVolta, idxLatitude, latTimes, latValues, sampleCountLat);
        XRKInterop.get_lap_GPS_channel_samples(idxf, idxVolta, idxLongitude, lngTimes, lngValues, sampleCountLng);

        // Cálculo da média
        double latMedia = latValues.Average();
        double lngMedia = lngValues.Average();

        return (latMedia, lngMedia);
    }
}
