using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
   public partial class MainWindow : Window , INotifyPropertyChanged
   {
      #region Private Properties
      private BitmapImage bitmapImage;
      private int bytesPerPixel = 4;
      private byte[] pixels = new byte[0];
      private bool setColor = false;
      private int rMove = 0;
      private int gMove = 0;
      private int bMove = 0;
      private int rSaved = 0;
      private int gSaved = 0;
      private int bSaved = 0;
      #endregion

      #region Constructor
      public MainWindow()
      {
         InitializeComponent();
         DataContext = this;
      }
      #endregion

      #region Public Properties
      /// <summary>
      /// Current R value
      /// </summary>
      public int RMove
      {
         get
         {
            return rMove;
         }
         set
         {
            rMove = value;
            OnPropertyChanged(nameof(RMove));
         }
      }

      /// <summary>
      /// Current G value
      /// </summary>
      public int GMove
      {
         get
         {
            return gMove;
         }
         set
         {
            gMove = value;
            OnPropertyChanged(nameof(GMove));
         }
      }

      /// <summary>
      /// Current B value
      /// </summary>
      public int BMove
      {
         get
         {
            return bMove;
         }
         set
         {
            bMove = value;
            OnPropertyChanged(nameof(BMove));
         }
      }

      /// <summary>
      /// Current R value
      /// </summary>
      public int RSaved
      {
         get
         {
            return rSaved;
         }
         set
         {
            rSaved = value;
            OnPropertyChanged(nameof(RSaved));
         }
      }

      /// <summary>
      /// Current G value
      /// </summary>
      public int GSaved
      {
         get
         {
            return gSaved;
         }
         set
         {
            gSaved = value;
            OnPropertyChanged(nameof(GSaved));
         }
      }

      /// <summary>
      /// Current B value
      /// </summary>
      public int BSaved
      {
         get
         {
            return bSaved;
         }
         set
         {
            bSaved = value;
            OnPropertyChanged(nameof(BSaved));
         }
      }
      #endregion

      #region Event Handlers and Functions
      public event PropertyChangedEventHandler? PropertyChanged;

      protected void OnPropertyChanged([CallerMemberName] string name = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      }

      private void btnLoadImage_Click(object sender, RoutedEventArgs e)
      {
         // New object
         bitmapImage = new BitmapImage();

         // Open file
         OpenFileDialog ofd = new OpenFileDialog();
         ofd.Multiselect = false;
         ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
         if (ofd.ShowDialog(this) == true)
         {
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(ofd.FileName);
            bitmapImage.EndInit();
         }

         // Get pixel array separately in memory
         GetPixels(bitmapImage, ref pixels);

         // Set as source
         imageControl.Source = bitmapImage;
         imageControl.UpdateLayout();
      }

      private void btnSetColor_Click(object sender, RoutedEventArgs e)
      {
         setColor = true;
      }

      private void imageControl_MouseMove(object sender, MouseEventArgs e)
      {
         Point p = e.GetPosition(imageControl);
         int i = ((int)p.X + (int)p.Y * (int)bitmapImage.Width) * bytesPerPixel;
         
         BMove = pixels[i];
         GMove = pixels[i+1];
         RMove = pixels[i+2];
      }

      private void imageControl_MouseUp(object sender, MouseButtonEventArgs e)
      {
         // Set the selected colors.
         if (setColor)
         {
            Point p = e.GetPosition(imageControl);
            int i = ((int)p.X + (int)p.Y * (int)bitmapImage.Width) * bytesPerPixel;

            BSaved = pixels[i];
            GSaved = pixels[i + 1];
            RSaved = pixels[i + 2];
         }

         // Set bool back to false so chosen colors aren't overwritten
         setColor= false;
      }

      #endregion

      #region Private Methods
      public void GetPixels(BitmapSource source, ref byte[] pc)
      {
         if (source.Format != PixelFormats.Bgra32)
            source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
         
         int width = source.PixelWidth;
         int height = source.PixelHeight;

         pc = new byte[width * height * bytesPerPixel];
         source.CopyPixels(pc, width * bytesPerPixel, 0);
      }

      #endregion
   }
}
