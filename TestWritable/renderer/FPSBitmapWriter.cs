using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Windows;

namespace TestWritable.renderer
{
    internal class FPSBitmapWriter
    {
        private readonly WriteableBitmap _bitmap;
        private readonly int _fontSize = 20;

        public FPSBitmapWriter(WriteableBitmap _bitmap)
        {
            this._bitmap = _bitmap;
        }

        public WriteableBitmap Bitmap => _bitmap;

        public void WriteFPS(string fpsString)
        {
            var stride = _bitmap.BackBufferStride;
            var pixelPtr = _bitmap.BackBuffer;

            // this is fast, changes to one object pixels will now be mirrored to the other 
            var bm2 = new Bitmap(200, 100, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, pixelPtr);

            _bitmap.Lock();

            // you might wanna use this in combination with Lock / Unlock, AddDirtyRect, Freeze
            // before you write to the shared Ptr
            using (var g = Graphics.FromImage(bm2))
            {
                g.DrawString(fpsString, new Font("Tahoma", _fontSize), System.Drawing.Brushes.White, 0, 0);
            }

            _bitmap.AddDirtyRect(new Int32Rect(0, 0, 200, 100));
            _bitmap.Unlock();
        }

    }
}
