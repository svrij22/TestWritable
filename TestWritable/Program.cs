using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Numerics;

namespace TestWritable
{
    class Program
    {
        static WriteableBitmap writeableBitmap;
        static Window w;
        static Image i;
        static GPURenderer renderer;

        [STAThread]
        static void Main(string[] args)
        {
            i = new Image();
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);

            w = new Window();
            w.Height = 400;
            w.Width = 600;
            w.Content = i;
            w.Show();
            w.Closing += W_Closing;

            writeableBitmap = new WriteableBitmap(
                (int)w.ActualWidth,
                (int)w.ActualHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            i.Source = writeableBitmap;

            i.Stretch = Stretch.None;
            i.HorizontalAlignment = HorizontalAlignment.Left;
            i.VerticalAlignment = VerticalAlignment.Top;

            i.MouseRightButtonDown +=
                new MouseButtonEventHandler(i_MouseRightButtonDown);
            i.MouseLeftButtonDown +=
                new MouseButtonEventHandler(i_MouseLeftButtonDown);

            w.MouseWheel += new MouseWheelEventHandler(w_MouseWheel);

            w.KeyDown += W_KeyDown;

            Application app = new Application();

            renderer = new GPURenderer(writeableBitmap, w.Width, w.Height);
            renderer.Compute();

            app.Run();
        }
        private static void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            renderer.Dispose();
        }

        private static void W_KeyDown(object sender, KeyEventArgs e)
        {
            const float Speed = 1.3f;

            switch (e.Key)
            {
                case Key.A:
                    // Move left
                    renderer.Origin = new Vector3(renderer.Origin.X - Speed,    renderer.Origin.Y,          renderer.Origin.Z);
                    break;
                case Key.D:
                    // Move right
                    renderer.Origin = new Vector3(renderer.Origin.X + Speed,    renderer.Origin.Y,          renderer.Origin.Z);
                    break;
                case Key.W:
                    // Move left
                    renderer.Origin = new Vector3(renderer.Origin.X,            renderer.Origin.Y ,         renderer.Origin.Z - Speed);
                    break;
                case Key.S:
                    // Move right
                    renderer.Origin = new Vector3(renderer.Origin.X,            renderer.Origin.Y ,         renderer.Origin.Z + Speed);
                    break;
            }

            // Assuming you have a Render method to redraw the scene
            renderer.Compute();
        }

        static void i_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            renderer.Compute();
        }

        static void i_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int column = (int)e.GetPosition(i).X;
            int row = (int)e.GetPosition(i).Y;
            //scene.TracePoint(column, row);
        }

        static void w_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Media.Matrix m = i.RenderTransform.Value;

            if (e.Delta > 0)
            {
                m.ScaleAt(
                    1.5,
                    1.5,
                    e.GetPosition(w).X,
                    e.GetPosition(w).Y);
            }
            else
            {
                m.ScaleAt(
                    1.0 / 1.5,
                    1.0 / 1.5,
                    e.GetPosition(w).X,
                    e.GetPosition(w).Y);
            }

            i.RenderTransform = new MatrixTransform(m);
        }
    }
}
