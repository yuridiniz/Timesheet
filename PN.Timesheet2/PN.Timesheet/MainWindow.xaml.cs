using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PN.Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            grdConteudo.MouseDown += grdConteudo_MouseDown;
        }

        private void grdConteudo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard FecharMenu = Resources["AnimFecharMenu"] as Storyboard;
            FecharMenu.Begin(grdMenu);
        }

        private void NumberButton_Click_1(object sender, RoutedEventArgs e)
        {
            Tarefa.Number = 10;
            Tarefa.ActiveNumber = true;
        }

    }
}
