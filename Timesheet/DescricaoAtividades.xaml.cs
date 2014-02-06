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
using Controller.Extends;
using Timesheet.Model;


namespace Timesheet
{
    /// <summary>
    /// Interaction logic for DescricaoAtividades.xaml
    /// </summary>
    public partial class DescricaoAtividades : Window
    {
        public static MainWindow MainContext;

        public DescricaoAtividades(MainWindow mainContext)
        {
            InitializeComponent();

            MainContext = mainContext;
            btnCancelar.Click += btnCancelar_Click;
            btnOk.Click += btnOk_Click;
        }

        private void btnCancelar_Click(object s, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object s, EventArgs e)
        {
            try
            {
                var registro = new Registro();
                //Adiciona 3 minutus para bater com o timesheet de papel

                registro.Saida = DateTime.Now.AddMinutes(3).ToLongTimeString(); 
                registro.Atividade = txtAtividade.Text;
                registro.Conferir = (MainContext.ckbConferir.IsChecked == true ? "Conferir" : "OK");

                registro.RegistrarSaida();

                MainContext.btnEntrada.IsEnabled = true;
                MainContext.btnSair.IsEnabled = false;

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
