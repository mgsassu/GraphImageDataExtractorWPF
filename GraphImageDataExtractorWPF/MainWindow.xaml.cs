using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace GraphImageDataExtractorWPF
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private BitmapImage bitmapImage = new BitmapImage();
      
      public MainWindow()
      {
         InitializeComponent();
      }



      #region Event Handlers
      private void btnLoadImage_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog ofd = new OpenFileDialog();
         ofd.Multiselect = false;
         ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
         if (ofd.ShowDialog(this) == true)
         {
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(ofd.FileName);
            bitmapImage.EndInit();
         }

         imageControl.Source = bitmapImage;
         imageControl.UpdateLayout();
      }

      #endregion
   }
}
