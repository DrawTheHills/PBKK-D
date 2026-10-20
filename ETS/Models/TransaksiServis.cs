#nullable disable
using FixItHub.Helpers;

namespace FixItHub.Models;

public class TransaksiServis : ObservableObject
{
    private string _statusServis;

    public int ServisId { get; set; }
    public string NoNota { get; set; }
    public int PelangganId { get; set; }
    public string NamaPelanggan { get; set; }
    public string NoTelepon { get; set; }
    public string Alamat { get; set; }
    public string Perangkat { get; set; }
    public string Keluhan { get; set; }
    public decimal EstimasiBiaya { get; set; }
    public DateTime TanggalMasuk { get; set; }

    public string StatusServis
    {
        get => _statusServis;
        set
        {
            if (value == null) return;
            SetProperty(ref _statusServis, value);
        }
    }
}
