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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PN.Timesheet.View.Menu
{
    /// <summary>
    /// Interaction logic for Usuario.xaml
    /// </summary>
    public partial class Usuario : UserControl
    {
        public Usuario()
        {
            this.DataContext = this;
            InitializeComponent();
        }
    }

    public partial class Usuario : UserControl
    {
        public object Nome
        {
            get
            {
                return txtUsername.Content;
            }
            set
            {
                txtUsername.Content = value;
            }
        }

        public object Email
        {
            get
            {
                return txtEmail.Content;
            }
            set
            {
                txtEmail.Content = value;
            }
        }
    }
}
