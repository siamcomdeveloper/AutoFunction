using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartGrid
{
    /// <summary>
    /// Interaction logic for SelectDevice.xaml
    /// </summary>
    public partial class SelectDevice : Window
    {
        public String device;
        public int Nodevice;
        public SelectDevice()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //a = "OK";
            Nodevice = listDevice.SelectedIndex;
            device = listDevice.SelectedValue.ToString();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //device = 
        }
    }
}
