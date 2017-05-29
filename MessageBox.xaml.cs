using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserChoice args = new UserChoice();
            args.Approved = true;
            OnButtonClicked(args);
            this.Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UserChoice args = new UserChoice();
            args.Approved = false;
            OnButtonClicked(args);
            this.Close();
        }
        protected virtual void OnButtonClicked(UserChoice uc)
        {
            ButtonClickedEvent?.Invoke(this, uc);
        }

        public event EventHandler<UserChoice> ButtonClickedEvent;
    }
    public class UserChoice : EventArgs
    {
        public bool Approved { get; set; }
    }
}
