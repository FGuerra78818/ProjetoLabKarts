using System;
using System.Runtime.InteropServices;

namespace ProjetoLabKarts.Interop;
public static class XRKInterop
{
    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_library_date();

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int open_file(string fullPath);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int open_file_with_licence(string fullPath, string licencePath);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_last_open_error();

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int close_file_n(string fullPath);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int close_file_i(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint get_logger_id(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_number_of_devices(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint get_device_id(int idxf, int idxd);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_vehicle_name(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_track_name(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_racer_name(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_championship_name(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_session_type_name(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_date_and_time(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_laps_count(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_info(int idxf, int idxl, out double start, out double duration);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_session_duration(int idxf, out double duration);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_channels_count(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_channel_name(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_channel_name_no_spaces(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_channel_units(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_channel_samples_count(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_channel_samples(int idxf, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_channel_samples_count(int idxf, int idxl, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_channel_samples(int idxf, int idxl, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int set_GPS_sample_freq(double freq);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_channels_count(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_channel_name(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_channel_name_no_spaces(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_channel_units(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_channel_samples_count(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_channel_samples(int idxf, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_GPS_channel_samples_count(int idxf, int idxl, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_GPS_channel_samples(int idxf, int idxl, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_raw_channels_count(int idxf);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_raw_channel_name(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_raw_channel_name_no_spaces(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr get_GPS_raw_channel_units(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_raw_channel_samples_count(int idxf, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_GPS_raw_channel_samples(int idxf, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_GPS_raw_channel_samples_count(int idxf, int idxl, int idxc);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_lap_GPS_raw_channel_samples(int idxf, int idxl, int idxc, double[] ptimes, double[] pvalues, int cnt);

    [DllImport("MatLabXRK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr library_test_on_open_files();
}