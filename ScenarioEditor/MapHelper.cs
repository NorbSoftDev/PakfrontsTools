using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataEditor
{
    class MapHelper
    {
        public Canvas canvas;
        public Scenario scenario;

        MatrixTransform matrixTransform;
        Line newLine;
        Point clickPoint;
        Point drawPoint;


        //DrawingVisual ghostVisual = new DrawingVisual();

        public MapHelper(Canvas canvas)
        {
            this.canvas = canvas;
            matrixTransform = new MatrixTransform();
            canvas.RenderTransform = matrixTransform;
            canvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(canvas_PreviewMouseLeftButtonDown);
            canvas.PreviewMouseMove += new MouseEventHandler(canvas_PreviewMouseMove);
            canvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(canvas_PreviewMouseLeftButtonUp);


//            using (DrawingContext dc = ghostVisual.RenderOpen())
//            {
//                // The body
//                dc.DrawGeometry(Brushes.Blue, null, Geometry.Parse(
//                @"M 240,250
//                  C 200,375 200,250 175,200
//                  C 100,400 100,250 100,200
//                  C 0,350 0,250 30,130
//                  C 75,0 100,0 150,0
//                  C 200,0 250,0 250,150 Z"));
//                // Left eye
//                dc.DrawEllipse(Brushes.Black, new Pen(Brushes.White, 10),
//                    new Point(95, 95), 15, 15);
//                // Right eye
//                dc.DrawEllipse(Brushes.Black, new Pen(Brushes.White, 10),
//                    new Point(170, 105), 15, 15);
//                // The mouth
//                Pen p = new Pen(Brushes.Black, 10);
//                p.StartLineCap = PenLineCap.Round;
//                p.EndLineCap = PenLineCap.Round;
//                dc.DrawLine(p, new Point(75, 160), new Point(175, 150));
//            }
        }

        public void loadMap(Scenario scenario)
        {
            this.scenario = scenario;
            scenario.map.Load();
            if (scenario.map.grayscaleFilePath != null)
            {
                Image image = new Image();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(scenario.map.grayscaleFilePath);
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.UriSource = new Uri(fileInfo.FullName, UriKind.Relative);
                src.EndInit();
                image.Source = src;
                image.Stretch = Stretch.Uniform;
                canvas.Children.Add(image);
                int zindex = canvas.Children.Count;
                Canvas.SetZIndex(image, zindex);
                Canvas.SetLeft(image, 5);
                Canvas.SetTop(image, 5);
                canvas.Width = src.Width;
                canvas.Height = src.Height;



            }
        }

        public void ClearDrawings()
        {
            var images = canvas.Children.OfType<Shape>().ToList();
            foreach (var image in images)
            {
                canvas.Children.Remove(image);
            }
        }

        void canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickPoint = (Point)e.GetPosition(canvas);
            newLine = new Line();
            newLine.Stroke = Brushes.Black;
            newLine.Fill = Brushes.Black;
            newLine.StrokeLineJoin = PenLineJoin.Bevel;
            newLine.X1 = clickPoint.X;
            newLine.Y1 = clickPoint.Y;
            newLine.X2 = clickPoint.X + 10;
            newLine.Y2 = clickPoint.Y + 10;
            newLine.StrokeThickness = 2;
            canvas.Children.Add(newLine);
            int zindex = canvas.Children.Count;
            Canvas.SetZIndex(newLine, zindex);
        }

        void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            drawPoint = (Point)e.GetPosition(canvas);
            if (newLine != null & e.LeftButton == MouseButtonState.Pressed)
            {
                newLine.X2 = drawPoint.X;
                newLine.Y2 = drawPoint.Y;
            }
        }
        void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            newLine = null;

        }

        public void loadImage()
        {
            Image image = new Image();


            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(ofd.FileName);
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.UriSource = new Uri(fileInfo.FullName, UriKind.Relative);
                src.EndInit();
                image.Source = src;
                image.Stretch = Stretch.Uniform;
                canvas.Children.Add(image);
                int zindex = canvas.Children.Count;
                Canvas.SetZIndex(image, zindex);
                Canvas.SetLeft(image, 50);
                Canvas.SetTop(image, 50);
            }
        }

        internal void zoom(double scale)
        {

            Matrix m = matrixTransform.Value;
            m.ScaleAtPrepend(scale, scale, 0, 0);

            MatrixAnimation matrixAnimation = new MatrixAnimation(matrixTransform.Value, m,
                TimeSpan.FromMilliseconds(300)
                );
            this.matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }

        internal void zoomOff()
        {

            Matrix m = matrixTransform.Value;
            m.M11 = 1; m.M12 = 0;
            m.M21 = 0; m.M22 = 1;

            MatrixAnimation matrixAnimation = new MatrixAnimation(matrixTransform.Value, m,
                TimeSpan.FromMilliseconds(300)
                );
            this.matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }

    }


}
