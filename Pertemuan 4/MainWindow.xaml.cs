
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

        private void TambahMahasiswa_Click(
            object sender, RoutedEventArgs e)
        {
            string nim = txtNIM.Text;
            string nama = txtNama.Text;

            if (string.IsNullOrWhiteSpace(nim) ||
                string.IsNullOrWhiteSpace(nama) ||
                cmbProdi.SelectedItem == null)
            {
                MessageBox.Show("Semua data harus diisi!");
                return;
            }

            ComboBoxItem prodi =
                (ComboBoxItem)cmbProdi.SelectedItem;

            string data = nim + " - " + nama +
                          " - " + prodi.Content;

            lstMahasiswa.Items.Add(data);

            txtNIM.Clear();
            txtNama.Clear();
            cmbProdi.SelectedIndex = -1;

            MessageBox.Show("Data mahasiswa berhasil ditambahkan!");
        }
    }
}
