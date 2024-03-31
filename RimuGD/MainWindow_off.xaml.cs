using AowVN.MVVM;
using System;
using System.Windows;


namespace RimuGD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new PatchWindowViewModel();
        }
        private void Storyboard_Completed(object sender, EventArgs e)
        {
            Close();
        }

    }

}


