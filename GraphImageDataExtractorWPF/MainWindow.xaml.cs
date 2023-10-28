using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

      private bool panning = false;
      private Point startPt;

      private bool setXStart = false;
      private bool setXEnd = false;
      private bool setYStart = false;
      private bool setYEnd = false;
      private double xStart = 0;
      private double xEnd = 0;
      private double yStart = 0;
      private double yEnd = 0;
      private double xDataStart = 0;
      private double xDataEnd = 0;
      private double yDataStart = 0;
      private double yDataEnd = 0;
      #endregion

      #region Constructor
      public MainWindow()
      {
         InitializeComponent();

         DataContext = this;

         // Set up Zoom
         PreviewMouseWheel += new MouseWheelEventHandler(MainWindow_PreviewMouseWheel);
         ResetZoom();

         // Set up panning

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

      /// <summary>
      /// Current XStart value
      /// </summary>
      public double XStart
      {
         get
         {
            return xStart;
         }
         set
         {
            xStart = value;
            OnPropertyChanged(nameof(XStart));
         }
      }

      /// <summary>
      /// Current XEnd value
      /// </summary>
      public double XEnd
      {
         get
         {
            return xEnd;
         }
         set
         {
            xEnd = value;
            OnPropertyChanged(nameof(XEnd));
         }
      }

      /// <summary>
      /// Current YStart value
      /// </summary>
      public double YStart
      {
         get
         {
            return yStart;
         }
         set
         {
            yStart = value;
            OnPropertyChanged(nameof(YStart));
         }
      }

      /// <summary>
      /// Current YEnd value
      /// </summary>
      public double YEnd
      {
         get
         {
            return yEnd;
         }
         set
         {
            yEnd = value;
            OnPropertyChanged(nameof(YEnd));
         }
      }

      /// <summary>
      /// Current XDataStart value
      /// </summary>
      public double XDataStart
      {
         get
         {
            return xDataStart;
         }
         set
         {
            xDataStart = value;
            OnPropertyChanged(nameof(XDataStart));
         }
      }

      /// <summary>
      /// Current XEnd value
      /// </summary>
      public double XDataEnd
      {
         get
         {
            return xDataEnd;
         }
         set
         {
            xDataEnd = value;
            OnPropertyChanged(nameof(XDataEnd));
         }
      }

      /// <summary>
      /// Current YStart value
      /// </summary>
      public double YDataStart
      {
         get
         {
            return yDataStart;
         }
         set
         {
            yDataStart = value;
            OnPropertyChanged(nameof(YDataStart));
         }
      }

      /// <summary>
      /// Current YEnd value
      /// </summary>
      public double YDataEnd
      {
         get
         {
            return yDataEnd;
         }
         set
         {
            yDataEnd = value;
            OnPropertyChanged(nameof(YDataEnd));
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

      private void imageControl_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (e.MouseDevice.MiddleButton == MouseButtonState.Pressed)
         {
            panning = true;
            startPt = e.GetPosition(scrollViewer);
            Cursor = Cursors.Hand;
         }
      }

      private void imageControl_MouseMove(object sender, MouseEventArgs e)
      {
         // If the middle mouse button is pressed, handle accordingly.
         if (panning)
         {
            double deltaX = e.GetPosition(scrollViewer).X - startPt.X;
            double deltaY = e.GetPosition(scrollViewer).Y - startPt.Y;
            startPt = e.GetPosition(scrollViewer);
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);
         }
         // Otherwise, output the colors of the current pixel
         else
         {
            Point p = e.GetPosition(imageControl);
            int i = ((int)p.X + (int)p.Y * (int)bitmapImage.Width) * bytesPerPixel;

            if (i < 0 || i >= pixels.Length)
               return;

            BMove = pixels[i];
            GMove = pixels[i + 1];
            RMove = pixels[i + 2];
         }

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
            // Set bool back to false so chosen colors aren't overwritten
            setColor = false;
         }
         else if (panning)
         {
            panning = false;
            Cursor = Cursors.Arrow;
         }
         else if (setXStart)
         {
            XStart = e.GetPosition(imageControl).X;
            setXStart = false;
         }
         else if (setXEnd)
         {
            XEnd = e.GetPosition(imageControl).X;
            setXEnd = false;
         }
         else if (setYStart)
         {
            YStart = e.GetPosition(imageControl).Y;
            setYStart = false;
         }
         else if (setYEnd)
         {
            YEnd = e.GetPosition(imageControl).Y;
            setYEnd = false;
         }
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

      private void btnSetXStart_Click(object sender, RoutedEventArgs e)
      {
         setXStart = true;
      }

      private void btnSetXEnd_Click(object sender, RoutedEventArgs e)
      {
         setXEnd = true;
      }

      private void btnSetYStart_Click(object sender, RoutedEventArgs e)
      {
         setYStart = true;
      }

      private void btnSetYEnd_Click(object sender, RoutedEventArgs e)
      {
         setYEnd = true;
      }

      private void btnExportData_Click(object sender, RoutedEventArgs e)
      {
         // Whole thing in a try catch block
         try
         {
            // Loop through each column, and get an average of the pixel locations that contain the selected color
            Dictionary<int, List<int>> pixelDict = new Dictionary<int, List<int>>();
            for (int col = 0; col < bitmapImage.PixelWidth; col++)
            {
               // New dict entry for the column
               pixelDict[col] = new List<int>();
               for (int row = 0; row < bitmapImage.PixelHeight; row++)
               {
                  // Get pixel location from array
                  int index = col + row * bitmapImage.PixelWidth;
                  if (pixels[index] == BSaved &&
                      pixels[index + 1] == GSaved &&
                      pixels[index + 2] == RSaved)
                  {
                     pixelDict[col].Add(row);
                  }
               }
            }

            // Loop through dict, get an average location for each column
            Dictionary<int, double> averageDict = new Dictionary<int, double>();
            foreach (int col in pixelDict.Keys)
            {
               averageDict[col] = Average(pixelDict[col]);
            }

            // Get conversions from pixel to data ranges
            // Note: The y start and end on the pixel end are flipped because row increases as you go down an image. 
            // In pictures of a graph, this will be reversed of course. 
            double xMultiplier = (xDataEnd - xDataStart) / (xEnd - xStart);
            double yMultiplier = (yDataEnd - yDataStart) / (yStart - yEnd);

            // Just going to make 2 string arrays, for speed
            List<string> xArr = new List<string>();
            List<string> yArr = new List<string>();
            foreach (int col in averageDict.Keys)
            {
               // Convert row and col to data
               // Note: The y pixel shift is flipped here too, same reason as above
               // Also, if value is -1, append empty strings
               if (averageDict[col] == -1)
               {
                  xArr.Add("");
                  yArr.Add("");
               }
               else
               {
                  xArr.Add(Convert.ToString((col - xStart) * xMultiplier));
                  yArr.Add(Convert.ToString((yStart - averageDict[col]) * yMultiplier));
               }
            }

            // Export to csv
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
               using (StreamWriter sw = new StreamWriter(sfd.FileName))
               {
                  sw.WriteLine("x,y");
                  for (int i = 0; i < xArr.Count; i++)
                  {
                     sw.WriteLine(xArr[i] + "," + yArr[i]);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show("Exception Encountered: " + ex.ToString());
            return;
         }
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

      private static double Average(List<int> array)
      {
         if (array.Count == 0)
            return -1;
         else
         {
            double sum = 0;
            for (int i = 0; i < array.Count; i++)
            {
               sum += array[i];
            }
            return sum / array.Count;
         }
      }
      #endregion
   }
}
