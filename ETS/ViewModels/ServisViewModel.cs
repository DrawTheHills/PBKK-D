#nullable disable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using FixItHub.Helpers;
using FixItHub.Models;
using Microsoft.Data.SqlClient;

namespace FixItHub.ViewModels;

public class ServisViewModel : ObservableObject
{
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=FixItHub_DB;Trusted_Connection=True;TrustServerCertificate=True;";

    private bool _isLoading;

    // Navigasi tab
    private int _selectedTab;
    public bool IsServisTab { get => _selectedTab == 0; set { if (value) SetTab(0); } }
    public bool IsPelangganTab { get => _selectedTab == 1; set { if (value) SetTab(1); } }
    public bool IsLaporanTab { get => _selectedTab == 2; set { if (value) SetTab(2); } }

    private void SetTab(int tab)
    {
        _selectedTab = tab;
        OnPropertyChanged(nameof(IsServisTab));
        OnPropertyChanged(nameof(IsPelangganTab));
        OnPropertyChanged(nameof(IsLaporanTab));
        if (tab == 2) LoadStats();
        if (tab == 1) LoadPelanggan();
    }

    // Data
    public ObservableCollection<TransaksiServis> DaftarServis { get; } = new();
    public ObservableCollection<Pelanggan> DaftarPelanggan { get; } = new();
    public ICollectionView ServisView { get; }

    public string[] FilterOptions { get; } = { "Semua", "Antrean", "Proses", "Siap Diambil", "Selesai", "Batal" };
    public string[] StatusOptions { get; } = { "Antrean", "Proses", "Siap Diambil", "Selesai", "Batal" };

    private string _selectedFilter = "Semua";
    public string SelectedFilter
    {
        get => _selectedFilter;
        set { if (SetProperty(ref _selectedFilter, value)) ServisView.Refresh(); }
    }

    // Modal Overlay
    private bool _isModalOpen;
    public bool IsModalOpen { get => _isModalOpen; set => SetProperty(ref _isModalOpen, value); }

    private string _namaPelanggan, _noTelepon, _alamat, _perangkat, _keluhan, _estimasiBiayaText, _errorMessage;
    public string NamaPelanggan { get => _namaPelanggan; set => SetProperty(ref _namaPelanggan, value); }
    public string NoTelepon { get => _noTelepon; set => SetProperty(ref _noTelepon, value); }
    public string Alamat { get => _alamat; set => SetProperty(ref _alamat, value); }
    public string Perangkat { get => _perangkat; set => SetProperty(ref _perangkat, value); }
    public string Keluhan { get => _keluhan; set => SetProperty(ref _keluhan, value); }
    public string EstimasiBiayaText { get => _estimasiBiayaText; set => SetProperty(ref _estimasiBiayaText, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    // Mode Tambah / Edit Servis
    private int? _editServisId;
    public bool IsEditMode => _editServisId.HasValue;
    public bool IsTambahMode => !IsEditMode;
    public string ModalTitle => IsEditMode ? "Edit Servis" : "Tambah Servis Baru";

    private void SetEditServisId(int? id)
    {
        _editServisId = id;
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsTambahMode));
        OnPropertyChanged(nameof(ModalTitle));
    }

    // Modal Edit Pelanggan
    private bool _isPelangganModalOpen;
    private int _editPelangganId;
    private string _pelNama, _pelTelepon, _pelAlamat, _pelError;
    public bool IsPelangganModalOpen { get => _isPelangganModalOpen; set => SetProperty(ref _isPelangganModalOpen, value); }
    public string PelNama { get => _pelNama; set => SetProperty(ref _pelNama, value); }
    public string PelTelepon { get => _pelTelepon; set => SetProperty(ref _pelTelepon, value); }
    public string PelAlamat { get => _pelAlamat; set => SetProperty(ref _pelAlamat, value); }
    public string PelError { get => _pelError; set => SetProperty(ref _pelError, value); }

    // Statistik 
    private decimal _totalPendapatan;
    private int _totalSelesai, _totalProses, _totalAntrean;
    public decimal TotalPendapatan { get => _totalPendapatan; set => SetProperty(ref _totalPendapatan, value); }
    public int TotalSelesai { get => _totalSelesai; set => SetProperty(ref _totalSelesai, value); }
    public int TotalProses { get => _totalProses; set => SetProperty(ref _totalProses, value); }
    public int TotalAntrean { get => _totalAntrean; set => SetProperty(ref _totalAntrean, value); }

    //  Commands 
    public ICommand BukaModalCommand { get; }
    public ICommand SimpanCommand { get; }
    public ICommand BatalCommand { get; }
    public ICommand HapusCommand { get; }
    public ICommand EditServisCommand { get; }
    public ICommand EditPelangganCommand { get; }
    public ICommand SimpanPelangganCommand { get; }
    public ICommand BatalPelangganCommand { get; }
    public ICommand HapusPelangganCommand { get; }

    public ServisViewModel()
    {
        ServisView = CollectionViewSource.GetDefaultView(DaftarServis);
        ServisView.Filter = FilterServis;

        BukaModalCommand = new RelayCommand(_ => BukaModal());
        SimpanCommand = new RelayCommand(_ => Simpan());
        BatalCommand = new RelayCommand(_ => TutupModal());
        HapusCommand = new RelayCommand(p => Hapus(p as TransaksiServis), p => p is TransaksiServis);
        EditServisCommand = new RelayCommand(p => BukaEditServis(p as TransaksiServis), p => p is TransaksiServis);
        EditPelangganCommand = new RelayCommand(p => BukaEditPelanggan(p as Pelanggan), p => p is Pelanggan);
        SimpanPelangganCommand = new RelayCommand(_ => SimpanPelanggan());
        BatalPelangganCommand = new RelayCommand(_ => IsPelangganModalOpen = false);
        HapusPelangganCommand = new RelayCommand(p => HapusPelanggan(p as Pelanggan), p => p is Pelanggan);

        LoadServis();
        LoadPelanggan();
        LoadStats();
    }

    // Filter real-time
    private bool FilterServis(object obj)
    {
        if (obj is not TransaksiServis s) return false;
        return SelectedFilter == "Semua" || s.StatusServis == SelectedFilter;
    }

    // Modal
    private void BukaModal()
    {
        NamaPelanggan = NoTelepon = Alamat = Perangkat = Keluhan = EstimasiBiayaText = ErrorMessage = string.Empty;
        SetEditServisId(null);
        IsModalOpen = true;
    }

    private void BukaEditServis(TransaksiServis item)
    {
        if (item == null) return;
        SetEditServisId(item.ServisId);
        NamaPelanggan = item.NamaPelanggan;
        NoTelepon = item.NoTelepon;
        Alamat = item.Alamat;
        Perangkat = item.Perangkat;
        Keluhan = item.Keluhan;
        EstimasiBiayaText = item.EstimasiBiaya.ToString("0.##", CultureInfo.GetCultureInfo("id-ID"));
        ErrorMessage = string.Empty;
        IsModalOpen = true;
    }

    private void TutupModal() => IsModalOpen = false;

    // Load data
    private void LoadServis()
    {
        _isLoading = true;
        try
        {
            foreach (var old in DaftarServis) old.PropertyChanged -= OnServisPropertyChanged;
            DaftarServis.Clear();

            const string sql = @"SELECT s.ServisId, s.NoNota, s.PelangganId, p.Nama, p.NoTelepon,
                                        s.Perangkat, s.Keluhan, s.EstimasiBiaya, s.StatusServis, s.TanggalMasuk,
                                        ISNULL(p.Alamat,'')
                                 FROM TransaksiServis s
                                 JOIN Pelanggan p ON p.PelangganId = s.PelangganId
                                 ORDER BY s.TanggalMasuk DESC";

            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var item = new TransaksiServis
                {
                    ServisId = r.GetInt32(0),
                    NoNota = r.GetString(1),
                    PelangganId = r.GetInt32(2),
                    NamaPelanggan = r.GetString(3),
                    NoTelepon = r.GetString(4),
                    Perangkat = r.GetString(5),
                    Keluhan = r.GetString(6),
                    EstimasiBiaya = r.GetDecimal(7),
                    StatusServis = r.GetString(8),
                    TanggalMasuk = r.GetDateTime(9),
                    Alamat = r.GetString(10)
                };
                item.PropertyChanged += OnServisPropertyChanged;
                DaftarServis.Add(item);
            }
        }
        catch (Exception ex) { ShowError("Gagal memuat data servis", ex); }
        finally { _isLoading = false; }
    }

    private void LoadPelanggan()
    {
        try
        {
            DaftarPelanggan.Clear();
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(
                "SELECT PelangganId, Nama, NoTelepon, ISNULL(Alamat,'') FROM Pelanggan ORDER BY PelangganId", con);
            con.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
                DaftarPelanggan.Add(new Pelanggan
                {
                    PelangganId = r.GetInt32(0),
                    Nama = r.GetString(1),
                    NoTelepon = r.GetString(2),
                    Alamat = r.GetString(3)
                });
        }
        catch (Exception ex) { ShowError("Gagal memuat data pelanggan", ex); }
    }

    private void LoadStats()
    {
        try
        {
            const string sql = @"SELECT
                ISNULL(SUM(CASE WHEN StatusServis='Selesai' THEN EstimasiBiaya END),0),
                COUNT(CASE WHEN StatusServis='Selesai' THEN 1 END),
                COUNT(CASE WHEN StatusServis='Proses'  THEN 1 END),
                COUNT(CASE WHEN StatusServis='Antrean' THEN 1 END)
                FROM TransaksiServis";
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, con);
            con.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                TotalPendapatan = r.GetDecimal(0);
                TotalSelesai = r.GetInt32(1);
                TotalProses = r.GetInt32(2);
                TotalAntrean = r.GetInt32(3);
            }
        }
        catch (Exception ex) { ShowError("Gagal memuat statistik", ex); }
    }

    // Simpan servis baru
    private void Simpan()
    {
        if (IsEditMode) { SimpanEdit(); return; }

        if (string.IsNullOrWhiteSpace(NamaPelanggan) || string.IsNullOrWhiteSpace(NoTelepon) ||
            string.IsNullOrWhiteSpace(Perangkat) || string.IsNullOrWhiteSpace(Keluhan))
        {
            ErrorMessage = "Nama, No. Telepon, Perangkat, dan Keluhan wajib diisi.";
            return;
        }

        var biayaText = string.IsNullOrWhiteSpace(EstimasiBiayaText) ? "0" : EstimasiBiayaText.Trim();
        if (!decimal.TryParse(biayaText, NumberStyles.Number, CultureInfo.GetCultureInfo("id-ID"), out var biaya) &&
            !decimal.TryParse(biayaText, NumberStyles.Number, CultureInfo.InvariantCulture, out biaya) || biaya < 0)
        {
            ErrorMessage = "Estimasi biaya harus berupa angka yang valid.";
            return;
        }

        try
        {
            using var con = new SqlConnection(ConnectionString);
            con.Open();
            using var tx = con.BeginTransaction();

            // Cari pelanggan berdasarkan No. Telepon
            int pelangganId;
            using (var cmd = new SqlCommand("SELECT TOP 1 PelangganId FROM Pelanggan WHERE NoTelepon=@tel", con, tx))
            {
                cmd.Parameters.AddWithValue("@tel", NoTelepon.Trim());
                var existing = cmd.ExecuteScalar();
                if (existing != null) pelangganId = (int)existing;
                else
                {
                    using var ins = new SqlCommand(
                        @"INSERT INTO Pelanggan (Nama, NoTelepon, Alamat)
                          OUTPUT INSERTED.PelangganId VALUES (@nama, @tel, @alamat)", con, tx);
                    ins.Parameters.AddWithValue("@nama", NamaPelanggan.Trim());
                    ins.Parameters.AddWithValue("@tel", NoTelepon.Trim());
                    ins.Parameters.AddWithValue("@alamat", (object)Alamat?.Trim() ?? DBNull.Value);
                    pelangganId = (int)ins.ExecuteScalar();
                }
            }

            // Generate No. Nota: FIX-yyyyMMdd-NNN
            int urut;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) + 1 FROM TransaksiServis WHERE CAST(TanggalMasuk AS DATE) = CAST(GETDATE() AS DATE)", con, tx))
                urut = (int)cmd.ExecuteScalar();
            var noNota = $"FIX-{DateTime.Now:yyyyMMdd}-{urut:000}";

            // Insert transaksi
            using (var cmd = new SqlCommand(
                @"INSERT INTO TransaksiServis (NoNota, PelangganId, Perangkat, Keluhan, EstimasiBiaya, StatusServis, TanggalMasuk)
                  VALUES (@nota, @pid, @perangkat, @keluhan, @biaya, 'Antrean', GETDATE())", con, tx))
            {
                cmd.Parameters.AddWithValue("@nota", noNota);
                cmd.Parameters.AddWithValue("@pid", pelangganId);
                cmd.Parameters.AddWithValue("@perangkat", Perangkat.Trim());
                cmd.Parameters.AddWithValue("@keluhan", Keluhan.Trim());
                cmd.Parameters.Add("@biaya", System.Data.SqlDbType.Decimal).Value = biaya;
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            TutupModal();
            LoadServis();
            LoadPelanggan();
            LoadStats();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menyimpan: " + ex.Message;
        }
    }

    // Hapus transaksi servis
    private void Hapus(TransaksiServis item)
    {
        if (item == null) return;

        var konfirmasi = MessageBox.Show(
            $"Hapus transaksi {item.NoNota} ({item.Perangkat} - {item.NamaPelanggan})?\nTindakan ini tidak dapat dibatalkan.",
            "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        if (konfirmasi != MessageBoxResult.Yes) return;

        try
        {
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("DELETE FROM TransaksiServis WHERE ServisId=@id", con);
            cmd.Parameters.AddWithValue("@id", item.ServisId);
            con.Open();
            cmd.ExecuteNonQuery();

            item.PropertyChanged -= OnServisPropertyChanged;
            DaftarServis.Remove(item);
            LoadStats();
        }
        catch (Exception ex) { ShowError("Gagal menghapus data", ex); }
    }

    // Simpan perubahan servis (Edit)
    private void SimpanEdit()
    {
        if (string.IsNullOrWhiteSpace(Perangkat) || string.IsNullOrWhiteSpace(Keluhan))
        {
            ErrorMessage = "Perangkat dan Keluhan wajib diisi.";
            return;
        }

        var teks = string.IsNullOrWhiteSpace(EstimasiBiayaText) ? "0" : EstimasiBiayaText.Trim();
        if (!decimal.TryParse(teks, NumberStyles.Number, CultureInfo.GetCultureInfo("id-ID"), out var biaya) || biaya < 0)
        {
            ErrorMessage = "Estimasi biaya harus berupa angka yang valid.";
            return;
        }

        try
        {
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(
                @"UPDATE TransaksiServis SET Perangkat=@perangkat, Keluhan=@keluhan, EstimasiBiaya=@biaya
                  WHERE ServisId=@id", con);
            cmd.Parameters.AddWithValue("@perangkat", Perangkat.Trim());
            cmd.Parameters.AddWithValue("@keluhan", Keluhan.Trim());
            cmd.Parameters.Add("@biaya", System.Data.SqlDbType.Decimal).Value = biaya;
            cmd.Parameters.AddWithValue("@id", _editServisId.Value);
            con.Open();
            cmd.ExecuteNonQuery();

            TutupModal();
            LoadServis();
            LoadStats();
        }
        catch (Exception ex) { ErrorMessage = "Gagal menyimpan perubahan: " + ex.Message; }
    }

    // Edit pelanggan
    private void BukaEditPelanggan(Pelanggan p)
    {
        if (p == null) return;
        _editPelangganId = p.PelangganId;
        PelNama = p.Nama;
        PelTelepon = p.NoTelepon;
        PelAlamat = p.Alamat;
        PelError = string.Empty;
        IsPelangganModalOpen = true;
    }

    private void SimpanPelanggan()
    {
        if (string.IsNullOrWhiteSpace(PelNama) || string.IsNullOrWhiteSpace(PelTelepon))
        {
            PelError = "Nama dan No. Telepon wajib diisi.";
            return;
        }

        try
        {
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(
                "UPDATE Pelanggan SET Nama=@nama, NoTelepon=@tel, Alamat=@alamat WHERE PelangganId=@id", con);
            cmd.Parameters.AddWithValue("@nama", PelNama.Trim());
            cmd.Parameters.AddWithValue("@tel", PelTelepon.Trim());
            cmd.Parameters.AddWithValue("@alamat", string.IsNullOrWhiteSpace(PelAlamat) ? DBNull.Value : PelAlamat.Trim());
            cmd.Parameters.AddWithValue("@id", _editPelangganId);
            con.Open();
            cmd.ExecuteNonQuery();

            IsPelangganModalOpen = false;
            LoadPelanggan();
            LoadServis(); 
        }
        catch (Exception ex) { PelError = "Gagal menyimpan perubahan: " + ex.Message; }
    }

    // Hapus pelanggan
    private void HapusPelanggan(Pelanggan p)
    {
        if (p == null) return;

        try
        {
            using var con = new SqlConnection(ConnectionString);
            con.Open();

            int jumlahServis;
            using (var cek = new SqlCommand("SELECT COUNT(*) FROM TransaksiServis WHERE PelangganId=@id", con))
            {
                cek.Parameters.AddWithValue("@id", p.PelangganId);
                jumlahServis = (int)cek.ExecuteScalar();
            }

            var pesan = jumlahServis == 0
                ? $"Hapus pelanggan \"{p.Nama}\"?"
                : $"Pelanggan \"{p.Nama}\" memiliki {jumlahServis} transaksi servis.\nMenghapus pelanggan ini juga akan MENGHAPUS SEMUA transaksinya.\n\nLanjutkan?";

            if (MessageBox.Show(pesan, "Konfirmasi Hapus Pelanggan", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            using var tx = con.BeginTransaction();
            using (var delServis = new SqlCommand("DELETE FROM TransaksiServis WHERE PelangganId=@id", con, tx))
            {
                delServis.Parameters.AddWithValue("@id", p.PelangganId);
                delServis.ExecuteNonQuery();
            }
            using (var delPel = new SqlCommand("DELETE FROM Pelanggan WHERE PelangganId=@id", con, tx))
            {
                delPel.Parameters.AddWithValue("@id", p.PelangganId);
                delPel.ExecuteNonQuery();
            }
            tx.Commit();

            LoadPelanggan();
            LoadServis();
            LoadStats();
        }
        catch (Exception ex) { ShowError("Gagal menghapus pelanggan", ex); }
    }

    // Ubah status langsung dari ComboBox -> SQL Server
    private void OnServisPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_isLoading || e.PropertyName != nameof(TransaksiServis.StatusServis)) return;
        if (sender is not TransaksiServis item) return;

        try
        {
            using var con = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(
                "UPDATE TransaksiServis SET StatusServis=@st WHERE ServisId=@id", con);
            cmd.Parameters.AddWithValue("@st", item.StatusServis);
            cmd.Parameters.AddWithValue("@id", item.ServisId);
            con.Open();
            cmd.ExecuteNonQuery();

            LoadStats();
            Application.Current.Dispatcher.BeginInvoke(new Action(ServisView.Refresh));
        }
        catch (Exception ex)
        {
            ShowError("Gagal mengubah status", ex);
            Application.Current.Dispatcher.BeginInvoke(new Action(LoadServis));
        }
    }

    private static void ShowError(string title, Exception ex)
        => MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
