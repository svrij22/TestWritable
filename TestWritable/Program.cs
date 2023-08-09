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
            w.SizeChanged += W_SizeChanged;

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
            StartFPSTimer();
            StartWriterTimer();

            app.Run();
        }

        public static bool isAwaitingInvoke = false;
        private static void W_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!isAwaitingInvoke)
            {
                isAwaitingInvoke = true;
                Dispatcher.CurrentDispatcher.Invoke(async () =>
                {
                    await Task.Delay(100);

                    _calcFpsTimer.Stop();
                    _writeFpsTimer.Stop();
                    _gpuComputeTaskPaused = true;

                    await Task.Delay(100);

                    writeableBitmap = new WriteableBitmap(
                        (int)w.ActualWidth,
                        (int)w.ActualHeight,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null);

                    i.Source = writeableBitmap;

                    bitmapFPSWriter = new FPSBitmapWriter(writeableBitmap);
                    renderer.Update(writeableBitmap, w.Width, w.Height);

                    _gpuComputeTaskPaused = false;
                    StartFPSTimer();
                    StartWriterTimer();

                    isAwaitingInvoke = false;
                });
            }
        }

        private static DispatcherTimer _calcFpsTimer;
        private static DispatcherTimer _writeFpsTimer;

        private static int _fpsCounter = 0;
        private static int _lastFps = 0;
        private static long _gpuRenderSpeed = 0;
        private static void StartFPSTimer()
        {
            _calcFpsTimer = new DispatcherTimer();
            _calcFpsTimer.Interval = TimeSpan.FromSeconds(1);
            _calcFpsTimer.Tick += (s, e) =>
            {
                Debug.WriteLine($"Executions per second: {_fpsCounter}");
                _lastFps = _fpsCounter;
                _fpsCounter = 0;
            };
            _calcFpsTimer.Start();
        }

        public static Task _gpuComputeTask;
        public static bool _gpuComputeTaskPaused = false;
        private static void StartGPUTimer()
        {
            _gpuComputeTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        if (!_gpuComputeTaskPaused)
                        {
                            _gpuRenderSpeed = renderer.Compute();
                            _fpsCounter++;
                            Thread.Sleep(1);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Thread.Sleep(100);
                    StartGPUTimer();
                }
            });
        }
        private static void StartWriterTimer()
        {
            _writeFpsTimer = new DispatcherTimer();
            _writeFpsTimer.Interval = TimeSpan.FromMilliseconds(12);
            _writeFpsTimer.Tick += (s, e) =>
            {
                WriteTick();
                bitmapFPSWriter.WriteFPS($"{_lastFps}fps - {_gpuRenderSpeed}ms");
            };
            _writeFpsTimer.Start();
        }

        const float Speed = 1.3f;
        public static void WriteTick()
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
            try
            {
                renderer.WriteToBitmap();
            }
            catch (Exception ex) { Debug.WriteLine("Write exception"); }
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
