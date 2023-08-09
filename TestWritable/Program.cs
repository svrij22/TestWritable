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
using System;
using System.Timers;
using System.Windows.Threading;

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
            w.Height = 800;
            w.Width = 800;
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

            w.MouseWheel += new MouseWheelEventHandler(w_MouseWheel);

            Application app = new Application();

            renderer = new GPURenderer(writeableBitmap, w.Width, w.Height);

            StartTimer();
            app.Run();
        }

        private static void StartTimer()
        {
            DispatcherTimer _timer;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                TimerTick();
                _timer.Start();
            };
            _timer.Start();
        }

        const float Speed = 1.3f;
        public static void TimerTick()
        {
            if (Keyboard.IsKeyDown(Key.A))
            {
                // Move left
                renderer.Origin = new Vector3(renderer.Origin.X - Speed, renderer.Origin.Y, renderer.Origin.Z);
                renderer.ResetPixelBuffer();
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                // Move right
                renderer.Origin = new Vector3(renderer.Origin.X + Speed, renderer.Origin.Y, renderer.Origin.Z);
                renderer.ResetPixelBuffer();
            }
            if (Keyboard.IsKeyDown(Key.W))
            {
                // Move forward
                renderer.Origin = new Vector3(renderer.Origin.X, renderer.Origin.Y, renderer.Origin.Z - Speed);
                renderer.ResetPixelBuffer();
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                // Move backward
                renderer.Origin = new Vector3(renderer.Origin.X, renderer.Origin.Y, renderer.Origin.Z + Speed);
                renderer.ResetPixelBuffer();
            }
            renderer.Compute();
        }

        private static void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            renderer.Dispose();
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
