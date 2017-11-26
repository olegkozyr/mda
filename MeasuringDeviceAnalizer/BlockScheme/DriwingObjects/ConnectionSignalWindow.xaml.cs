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

namespace BlockScheme
{
    /// <summary>
    /// Interaction logic for ConnectionSignalWindow.xaml
    /// </summary>
    public partial class ConnectionSignalWindow : Window
    {
        public ConnectionSignalWindow()
        {
            InitializeComponent();
        }

        private void yesBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void noBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        
		void Signal_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
		}
		
		void Signal_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space) e.Handled = true;
		}
		
		void Signal_LostFocus(object sender, RoutedEventArgs e)
		{
			string text = ((TextBox)sender).Text;
			int num;
			if (text.Equals("0")) 
			{
				MessageBox.Show("Номер сигналу неможе дорівнювати 0");
				((TextBox)sender).Text = "";
			}
			else if (!int.TryParse(text, out num))
			{
				MessageBox.Show("Завеликий номер вхідного сигналу");
				((TextBox)sender).Text = "";			         	
			}
		}
    }
}
