using System.Windows;
using System.Windows.Controls;

namespace StudentRegistrationApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNim.Text))
            {
                MessageBox.Show("NIM wajib diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNim.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Mahasiswa wajib diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNama.Focus();
                return;
            }

            if (cmbProdi.SelectedItem == null)
            {
                MessageBox.Show("Silakan pilih Program Studi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rbLaki.IsChecked != true && rbPerempuan.IsChecked != true)
            {
                MessageBox.Show("Silakan pilih Jenis Kelamin!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            string nim = txtNim.Text.Trim();
            string nama = txtNama.Text.Trim();
            string prodi = ((ComboBoxItem)cmbProdi.SelectedItem).Content.ToString();
            string gender = rbLaki.IsChecked == true ? "Laki-laki" : "Perempuan";

            string formattedData = $"{nim} - {nama} | {prodi} ({gender})";
            lstMahasiswa.Items.Add(formattedData);

            MessageBox.Show("Data mahasiswa berhasil disimpan!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

            ResetForm();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (lstMahasiswa.SelectedItem != null)
            {
                var result = MessageBox.Show("Apakah Anda yakin ingin menghapus data terpilih?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    lstMahasiswa.Items.Remove(lstMahasiswa.SelectedItem);
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang ingin dihapus terlebih dahulu!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetForm()
        {
            txtNim.Clear();
            txtNama.Clear();
            cmbProdi.SelectedIndex = -1;
            rbLaki.IsChecked = false;
            rbPerempuan.IsChecked = false;
            txtNim.Focus();
        }
    }
}
