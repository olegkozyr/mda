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
	/// Interaction logic for BlockSumWindow.xaml
	/// </summary>
	public partial class BlockSumWindow : Window
	{
		public BlockSumWindow()
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
		
		void InPortCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			switch(e.Text)
			{
				case "+":
					break;
				case "-":
					break;
				default:
					e.Handled = true;
					break;
			}
		}
		
		void InPortCount_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space) e.Handled = true;			
		}
	}
}