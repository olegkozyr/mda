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
    /// Interaction logic for BlockSwitchWindow.xaml
    /// </summary>
    public partial class BlockSwitchWindow : Window
    {
        public BlockSwitchWindow()
        {
            InitializeComponent();
        }

        private void yesBtn_Click(object sender, RoutedEventArgs e)
        {
        	if ((LeftPortCount.Text == "") || (RightPortCount.Text == ""))
        	{
        		MessageBox.Show("Кількість портів неможе бути меншою одиниці");
        		return;
        	} 

        	if ((Convert.ToInt16(LeftPortCount.Text) < 1) || (Convert.ToInt16(RightPortCount.Text) < 1))
        	{
        		MessageBox.Show("Кількість портів неможе бути меншою одиниці");
        		return;
        	} 
        	
        	if ((Convert.ToInt16(LeftPortCount.Text) > 1) && (Convert.ToInt16(RightPortCount.Text) > 1))
        	{
        		MessageBox.Show("Кількість портів на одному кінці не повинна бути більшою одиниці");
        		return;
        	}        	       
        	
            DialogResult = true;
            this.Close();
        }

        private void noBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }            
		
		void PortCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
		}
		
		void PortCount_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space) e.Handled = true;
		}
    }
}
