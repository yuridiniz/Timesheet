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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Timesheet.Model;

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for CadastrarAtividade.xaml
    /// </summary>
    public partial class CadastrarAtividade : Window
    {
        public CadastrarAtividade()
        {
            InitializeComponent();

            btnCancelar.Click += (s, e) => { this.Close(); };
            btnOk.Click += btnOk_Click;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Atividade.Registrar(txtAtividade.Text);
            this.Close();
        }
    }
}
