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
        static Scene scene;

        [STAThread]
        static void Main(string[] args)
        {
            i = new Image();
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);

            w = new Window();
            w.Height = 560;
            w.Width = 960;
            w.Content = i;
            w.Show();

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
            scene = new Scene(writeableBitmap, w.Width, w.Height);
            scene.Draw();
            app.Run();
        }

        private static void W_KeyDown(object sender, KeyEventArgs e)
        {
            const float Speed = 0.1f;

            switch (e.Key)
            {
                case Key.A:
                    // Move left
                    scene.Origin = new Vector3(scene.Origin.X - Speed,    scene.Origin.Y,          scene.Origin.Z);
                    break;
                case Key.D:
                    // Move right
                    scene.Origin = new Vector3(scene.Origin.X + Speed,    scene.Origin.Y,          scene.Origin.Z);
                    break;
                case Key.W:
                    // Move left
                    scene.Origin = new Vector3(scene.Origin.X,            scene.Origin.Y ,         scene.Origin.Z - Speed);
                    break;
                case Key.S:
                    // Move right
                    scene.Origin = new Vector3(scene.Origin.X,            scene.Origin.Y ,         scene.Origin.Z + Speed);
                    break;
            }

            // Assuming you have a Render method to redraw the scene
            scene.Draw();
        }

        static void i_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            scene.Draw();
        }

        static void i_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int column = (int)e.GetPosition(i).X;
            int row = (int)e.GetPosition(i).Y;
            scene.TracePoint(column, row);
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
