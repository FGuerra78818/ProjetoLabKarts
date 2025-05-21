using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProjetoLabKarts.Interop;

namespace ProjetoLabKarts.Interop;

public class ChannelData
{
    public string Name { get; set; }
    public string Units { get; set; }
    public double[] Times { get; set; }
    public double[] Values { get; set; }
}

public class LapInfo
{
    public int LapIndex { get; set; }
    public double Start { get; set; }
    public double Duration { get; set; }
}

public static class XRKReader
{
    public static List<ChannelData> LoadAllChannelData(string fullPath)
    {
        // 1) Abre ficheiro
        int idxf = XRKInterop.open_file(fullPath);
        if (idxf < 0)
        {
            string err = Marshal.PtrToStringAnsi(XRKInterop.get_last_open_error());
            throw new InvalidOperationException($"Erro ao abrir {fullPath}: {err}");
        }

        var channels = new List<ChannelData>();

        // 2) Leitura dos canais de telemetria
        int chanCount = XRKInterop.get_channels_count(idxf);
        for (int c = 0; c < chanCount; c++)
        {
            string name = Marshal.PtrToStringAnsi(XRKInterop.get_channel_name(idxf, c));
            string units = Marshal.PtrToStringAnsi(XRKInterop.get_channel_units(idxf, c));

            int cnt = XRKInterop.get_channel_samples_count(idxf, c);
            var times = new double[cnt];
            var values = new double[cnt];
            int read = XRKInterop.get_channel_samples(idxf, c, times, values, cnt);
            if (read != cnt)
                throw new Exception($"Lidos {read} de {cnt} para canal '{name}'");

            channels.Add(new ChannelData
            {
                Name = name,
                Units = units,
                Times = times,
                Values = values
            });
        }

        // 3) Leitura dos canais GPS
        int gpsCount = XRKInterop.get_GPS_channels_count(idxf);
        for (int c = 0; c < gpsCount; c++)
        {
            string name = Marshal.PtrToStringAnsi(XRKInterop.get_GPS_channel_name(idxf, c));
            string units = Marshal.PtrToStringAnsi(XRKInterop.get_GPS_channel_units(idxf, c));

            int cnt = XRKInterop.get_GPS_channel_samples_count(idxf, c);
            var times = new double[cnt];
            var values = new double[cnt];
            int read = XRKInterop.get_GPS_channel_samples(idxf, c, times, values, cnt);
            if (read != cnt)
                throw new Exception($"Lidos {read} de {cnt} (GPS) para canal '{name}'");

            channels.Add(new ChannelData
            {
                Name = name,
                Units = units,
                Times = times,
                Values = values
            });
        }

        // 4) Fecha ficheiro
        XRKInterop.close_file_i(idxf);

        return channels;
    }

    public static List<LapInfo> LoadLaps(string fullPath)
    {
        // 1) Abre ficheiro
        int idxf = XRKInterop.open_file(fullPath);
        if (idxf < 0)
        {
            string err = Marshal.PtrToStringAnsi(XRKInterop.get_last_open_error());
            throw new InvalidOperationException($"Erro ao abrir {fullPath}: {err}");
        }

        var laps = new List<LapInfo>();

        try
        {
            // 2) Número de voltas
            int lapCount = XRKInterop.get_laps_count(idxf);

            // 3) Para cada volta, lê início e duração
            for (int i = 0; i < lapCount; i++)
            {
                double start, duration;
                int ok = XRKInterop.get_lap_info(idxf, i, out start, out duration);
                if (ok == 0)
                    throw new Exception($"Erro a obter info da volta {i} no ficheiro {fullPath}");

                laps.Add(new LapInfo
                {
                    LapIndex = i,
                    Start = start,
                    Duration = duration
                });
            }
        }
        finally
        {
            // 4) Fecha ficheiro
            XRKInterop.close_file_i(idxf);
        }

        return laps;
    }
}
