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
    /// Interaction logic for BlockTwoWaysWindow.xaml
    /// </summary>
    public partial class BlockTwoWaysWindow : Window
    {
        public BlockTwoWaysWindow()
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
    }
}
