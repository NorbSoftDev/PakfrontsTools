using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#if __MonoCS__
using System.Linq;
using System.IO;
#else
#endif

// http://www.codeproject.com/Tips/240428/Work-with-bitmap-faster-with-Csharp
//https://msdn.microsoft.com/en-us/library/5ey6h79d.aspx
namespace NorbSoftDev.SOW.Utils
{

    public class TerrainBitmap
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;
        Map map { get; set; }

        public byte[] bytes { get; set; }
        public int depth { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }

        public TerrainBitmap(Map map)
        {
            this.map = map;
            
            string path = map.grayscaleFilePath;
            //Console.WriteLine("Getting bitmap for \""+map.name+"\" \""+path+"\"");
            this.source = new Bitmap(path, false);
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                width = source.Width;
                height = source.Height;

                // get total locked pixels count
                int PixelCount = width * height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, width, height);

                // get source bitmap pixel format size
                depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (depth != 8 && depth != 24 && depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }


#if __MonoCS__
                Console.WriteLine("BitConverter.IsLittleEndian = "+BitConverter.IsLittleEndian);
                Console.WriteLine("BitConverter.IsLittleEndian = "+BitConverter.IsLittleEndian);

                using(MemoryStream ms = new MemoryStream())
                {
                    source.Save(ms,  ImageFormat.Bmp);
                    Pixels = ms.ToArray();
                    Console.WriteLine("Depth:"+Depth+" "+source.PixelFormat);
                    Console.WriteLine("Created Byte Array "+Pixels.Length+" Channels:"+( Pixels.Length/(source.Width*source.Height) ));
                    Console.WriteLine("Bytes 0,0 "+GetPixel(0,0));
                    SwapSingles(Pixels);
                    Console.WriteLine("Swapped Bytes 0,0 "+GetPixel(0,0));

                   // ImageConverter converter = new ImageConverter();
                   // Pixels = converter.ConvertTo(source, typeof(byte[]));

                    // Console.WriteLine("Created Byte Array "+ar.Length+" Channels:"+( ar.Length/(image.Width*image.Height) ));
                }

#else
                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = depth / 8;
                bytes = new byte[PixelCount * step];

                // Copy data from pointer to array
                Iptr = bitmapData.Scan0;
                Marshal.Copy(Iptr, bytes, 0, bytes.Length);
#endif
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Bytes 0,0 Color [A=0, R=77, G=77, B=66]
        //Bytes 0,0 Color [A=0, R=77, G=77, B=66]


#if __MonoCS__
        public static unsafe void SwapSingles(byte[] data)
        {
            int cnt = data.Length / 4;
            fixed (byte* d = data)
            {
                byte* p = d;
                while (cnt-- > 0)
                {
                    byte a = *p;
                    p++;
                    byte b = *p;
                    *p = *(p + 1);
                    p++;
                    *p = b;
                    p++;
                    *(p - 3) = *p;
                    *p = a;
                    p++;
                }
            }
        }
#endif
        // /// <summary>
        // /// Unlock bitmap data
        // /// </summary>
        // public void UnlockBits()
        // {
        //     try
        //     {
        //         // Copy data from byte array to pointer
        //         Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

        //         // Unlock bitmap data
        //         source.UnlockBits(bitmapData);
        //     }
        //     catch (Exception ex)
        //     {
        //         throw ex;
        //     }
        // }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = depth / 8;

            // Get start index of the specified pixel
            int i = ((y * width) + x) * cCount;

            if (i > bytes.Length - cCount)
                throw new IndexOutOfRangeException();

            if (depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = bytes[i];
                byte g = bytes[i + 1];
                byte r = bytes[i + 2];
                byte a = bytes[i + 3]; // a
#if __MonoCS__
                // byte[] bytes = BitConverter.GetBytes(r);
                // Array.Reverse(bytes);
                // r = bytes[0];

                // byte [] bytes = new byte [] {b,g,r,a};
                // Array.Reverse(bytes, 0, bytes.Length);
                // r = bytes[2];

                // b = (byte)((b * 0x0202020202 & 0x010884422010) % 1023); 
                // g = (byte)((g * 0x0202020202 & 0x010884422010) % 1023); 
                // r = (byte)((r * 0x0202020202 & 0x010884422010) % 1023); 
                // a = (byte)((a * 0x0202020202 & 0x010884422010) % 1023); 
#endif
                clr = Color.FromArgb(a, r, g, b);
            }
            if (depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = bytes[i];
                byte g = bytes[i + 1];
                byte r = bytes[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = bytes[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            int cCount = depth / 8;

            // Get start index of the specified pixel
            int i = ((y * width) + x) * cCount;

            if (depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                bytes[i] = color.B;
                bytes[i + 1] = color.G;
                bytes[i + 2] = color.R;
                bytes[i + 3] = color.A;
            }
            if (depth == 24) // For 24 bpp set Red, Green and Blue
            {
                bytes[i] = color.B;
                bytes[i + 1] = color.G;
                bytes[i + 2] = color.R;
            }
            if (depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                bytes[i] = color.B;
            }
        }

        public Terrain GetTerrainAtPosition(float south, float east)
        {
            int x = (int)(width * east/map.extent) ;
            int y = (int)(height * south/map.extent);

            if (x < 0 || x > width || y < 0 || y > height) return null;
            int r = GetPixel(x, y).R;
            //Console.WriteLine("Lookup:" + x + "," + y + " = " + r);

            return map.GetTerrainAtIndex(r);
        }

        public Terrain GetTerrainAtPixel(int x, int y)
        {
            if (x < 0 || x > width || y < 0 || y > height) return null;
            int r = GetPixel(x, y).R;
            //Console.WriteLine("Lookup:" + x + "," + y + " = " + r);

            return map.GetTerrainAtIndex(r);
        }
    }
}