using System;
using System.Windows;
using StudentRegistrationApp.Models;
using StudentRegistrationApp.Repositories;

namespace StudentRegistrationApp
{
    public partial class MainWindow : Window
    {
        private readonly StudentRepository _studentRepository;

        public MainWindow()
        {
            InitializeComponent();
            _studentRepository = new StudentRepository();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPrograms();
            LoadStudents();
        }

        private void LoadPrograms()
        {
            try
            {
                var programs = _studentRepository.GetPrograms();
                cmbProdi.ItemsSource = programs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat prodi: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudents()
        {
            try
            {
                var students = _studentRepository.GetAll();
                lstMahasiswa.ItemsSource = students;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data mahasiswa: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNim.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || 
                cmbProdi.SelectedValue == null || (rbLaki.IsChecked != true && rbPerempuan.IsChecked != true))
            {
                MessageBox.Show("Lengkapi seluruh data terlebih dahulu!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newStudent = new Student
            {
                NIM = txtNim.Text.Trim(),
                Name = txtNama.Text.Trim(),
                ProgramId = (int)cmbProdi.SelectedValue,
                Gender = rbLaki.IsChecked == true ? "Laki-laki" : "Perempuan"
            };

            try
            {
                if (_studentRepository.Add(newStudent))
                {
                    MessageBox.Show("Data mahasiswa berhasil disimpan ke database!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (lstMahasiswa.SelectedItem is Student selectedStudent)
            {
                if (MessageBox.Show($"Yakin ingin menghapus {selectedStudent.Name}?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_studentRepository.Delete(selectedStudent.StudentId))
                        {
                            MessageBox.Show("Data berhasil dihapus dari database!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadStudents();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gagal menghapus data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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