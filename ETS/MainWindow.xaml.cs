using System.Windows;
using FixItHub.ViewModels;

namespace FixItHub;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ServisViewModel();
    }
}
