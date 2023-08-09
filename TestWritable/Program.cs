using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Numerics;
using System;
using System.Timers;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Timer = System.Threading.Timer;
using TestWritable.renderer;

namespace TestWritable
{
    class Program
    {
        static WriteableBitmap writeableBitmap;
        static Window w;
        static Image i;
        static GPURenderer renderer;
        static FPSBitmapWriter bitmapFPSWriter;

        [STAThread]
        static void Main(string[] args)
        {
            i = new Image();
            RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(i, EdgeMode.Aliased);

            w = new Window();
            w.Height = 980;
            w.Width = 1500;
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
            bitmapFPSWriter = new FPSBitmapWriter(writeableBitmap);

            StartGPUTimer();
            StartCounterTimer();
            StartWriterTimer();

            app.Run();
        }

        private static Timer _timer;
        private static DispatcherTimer _secondTimer;
        private static int _fpsCounter = 0;
        private static int _lastFps = 0;
        private static long _gpuRenderSpeed = 0;
        private static void StartCounterTimer()
        {
            _secondTimer = new DispatcherTimer();
            _secondTimer.Interval = TimeSpan.FromSeconds(1);
            _secondTimer.Tick += (s, e) =>
            {
                Debug.WriteLine($"Executions per second: {_fpsCounter}");
                _lastFps = _fpsCounter;
                _fpsCounter = 0;
            };
            _secondTimer.Start();
        }
        private static void StartGPUTimer()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        _gpuRenderSpeed = renderer.Compute();
                        _fpsCounter++;
                    }
                }catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }
        private static void StartWriterTimer()
        {
            _secondTimer = new DispatcherTimer();
            _secondTimer.Interval = TimeSpan.FromMilliseconds(12);
            _secondTimer.Tick += (s, e) =>
            {
                TimerTick();
                bitmapFPSWriter.WriteFPS($"{_lastFps}fps - {_gpuRenderSpeed}ms");
            };
            _secondTimer.Start();
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

            if (Keyboard.IsKeyDown(Key.Q))
            {
                // Move backward
                renderer.Origin = new Vector3(renderer.Origin.X, renderer.Origin.Y - Speed, renderer.Origin.Z);
                renderer.ResetPixelBuffer();
            }

            if (Keyboard.IsKeyDown(Key.E))
            {
                // Move backward
                renderer.Origin = new Vector3(renderer.Origin.X, renderer.Origin.Y + Speed, renderer.Origin.Z + Speed);
                renderer.ResetPixelBuffer();
            }
            renderer.WriteToBitmap();
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
