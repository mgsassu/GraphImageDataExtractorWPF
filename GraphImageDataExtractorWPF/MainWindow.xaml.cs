using Microsoft.VisualBasic;
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
      #region Constants
      private const double MinZoom = 0.8;
      private const double MaxZoom = 12.0;
      private const double NoZoom = 1.0;

      private const double ZoomInc = 0.1;
      #endregion

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
      private double currentZoom = 1.0;
      #endregion

      #region Constructor
      public MainWindow()
      {
         InitializeComponent();

         DataContext = this;

         // Set up Zoom
         
         PreviewMouseWheel += new MouseWheelEventHandler(MainWindow_PreviewMouseWheel);
         //scrollViewer.ScrollChanged += new ScrollChangedEventHandler(scrollViewer_ScrollChanged);
         ResetZoom();
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
         

         // Open file
         OpenFileDialog ofd = new OpenFileDialog();
         ofd.Multiselect = false;
         ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
         if (ofd.ShowDialog(this) == true)
         {
            // New object
            bitmapImage = new BitmapImage();

            // Initialize
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(ofd.FileName);
            bitmapImage.EndInit();

            // Get pixel array separately in memory
            GetPixels(bitmapImage, ref pixels);

            // Set as source
            imageControl.Source = bitmapImage;
            imageControl.UpdateLayout();

            // reset zoom
            ResetZoom();
         }
      }

      private void btnSetColor_Click(object sender, RoutedEventArgs e)
      {
         setColor = true;
      }

      private void imageControl_MouseMove(object sender, MouseEventArgs e)
      {
         Point p = e.GetPosition(imageControl);
         int i = ((int)p.X + (int)p.Y * (int)bitmapImage.Width) * bytesPerPixel;

         if (i < 0 || i >= pixels.Length)
            return;
         
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

      private void ResetZoom()
      {
         currentZoom = NoZoom;
         imageViewBox.Width = scrollViewer.ViewportWidth;
         imageViewBox.Height = scrollViewer.ViewportHeight;
      }

      private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
      {
         e.Handled = true;
         Point pt = e.GetPosition(imageViewBox);
         Point pt2 = e.GetPosition(scrollViewer);
         double zoom = currentZoom;
         if (e.Delta > 0)
            zoom += ZoomInc;
         else
            zoom -= ZoomInc;
         if (zoom < MinZoom)
            zoom = MinZoom;
         if (zoom > MaxZoom)
            zoom = MaxZoom;

         if (zoom == currentZoom)
            return;

         pt = new Point((pt.X / currentZoom) * zoom, (pt.Y / currentZoom) * zoom);
         pt.Offset(-pt2.X, -pt2.Y);

         currentZoom = zoom;
         imageViewBox.Width = this.scrollViewer.ViewportWidth * zoom;
         imageViewBox.Height = this.scrollViewer.ViewportHeight * zoom;


         this.imageViewBox.BringIntoView(new Rect(pt, new Size(this.scrollViewer.ViewportWidth, this.scrollViewer.ViewportHeight)));
      }

      //private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
      //{
      //   if (e.ViewportWidthChange != 0)
      //   {
      //      imageViewBox.Width = this.scrollViewer.ViewportWidth * currentZoom;
      //   }
      //   if (e.ViewportHeightChange != 0)
      //   {
      //      imageViewBox.Height = this.scrollViewer.ViewportHeight * currentZoom;
      //   }
      //}
      #endregion
   }
}
