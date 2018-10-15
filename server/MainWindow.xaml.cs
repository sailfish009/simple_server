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

namespace server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => {AsyncServer.StartListening();});
            AsyncServer.init();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteData = Encoding.ASCII.GetBytes("hello, world");
            AsyncServer.send(0, byteData);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Task.Factory.StartNew(() => {AsyncServer.close();});
        }

    }
}
