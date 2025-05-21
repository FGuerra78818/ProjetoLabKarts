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
}
