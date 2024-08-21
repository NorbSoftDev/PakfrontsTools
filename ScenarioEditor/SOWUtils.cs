using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorbSoftDev.SOW;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace ScenarioEditor
{
    /// <summary>
    /// Utility functions with Windows a Windows Media dependencies that prevent
    /// them from being in mono compatible base lib
    /// </summary>
    static class SOWUtils
    {
        static Random random = new Random();
        static public void RandomizePositions(ScenarioEchelon echelon, Rect rect)
        {
            if (echelon.unit != null)
            {
                echelon.unit.transform.south = (float)(rect.Left + random.NextDouble() * rect.Width);

                echelon.unit.transform.east = (float)(rect.Top + random.NextDouble() * rect.Height);
            }

            foreach (ScenarioEchelon child in echelon.children)
            {
                RandomizePositions(child, rect);
            }
        }

        //static Dictionary<string, System.Drawing.Bitmap> bitmaps = new Dictionary<string, System.Drawing.Bitmap>(StringComparer.OrdinalIgnoreCase);
        
        //static public System.Drawing.Bitmap GetBitmap(this LogisticsEntryBitmap entry, Config config)
        //{


        //    if (entry.file == null) return null;
        //    if (bitmaps.ContainsKey(entry.file)) return bitmaps[entry.file];

        //    System.Drawing.Bitmap bitmap;

        //    string filepath = config.FindGraphic(entry.file);
        //    if (filepath != null)
        //    {
        //        Console.WriteLine("Loading Bitmap " + filepath);
        //        bitmap = DevIL.DevIL.LoadBitmap(filepath);
        //        bitmaps[entry.file] = bitmap;
        //        return bitmap;
        //    }

        //    return null;

        //}

        // http://social.msdn.microsoft.com/Forums/windowshardware/en-US/358120f1-81f4-4a10-91da-e321731537bc/going-crazy-with-thishow-to-loadsave-dds-images?forum=csharpgeneral

        // http://blogs.msdn.com/b/ryantrem/archive/2010/06/06/loading-dds-files-as-wpf-imagesources-using-xna.aspx
        // http://blogs.msdn.com/b/nicgrave/archive/2010/07/25/rendering-with-xna-framework-4-0-inside-of-a-wpf-application.aspx
        //public static ImageSource Convert(Stream stream)
        //{
        //    // Create the graphics device
        //    using (var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.NullReference, IntPtr.Zero, new PresentationParameters()))
        //    {
        //        // Setup the texture creation parameters
        //        var textureCreationParameters = new TextureCreationParameters()
        //        {
        //            Width = -1,
        //            Height = -1,
        //            Depth = 1,
        //            TextureUsage = TextureUsage.None,
        //            Format = SurfaceFormat.Color,
        //            Filter = FilterOptions.None,
        //            MipFilter = FilterOptions.None,
        //            MipLevels = 1
        //        };

        //        // Load the texture
        //        using (var texture = Texture2D.FromFile(graphicsDevice, stream, textureCreationParameters))
        //        {
        //            // Get the pixel data
        //            var pixelColors = new Microsoft.Xna.Framework.Graphics.Color[texture.Width * texture.Height];
        //            texture.GetData(pixelColors);

        //            // Copy the pixel colors into a byte array
        //            var bytesPerPixel = 3;
        //            var stride = texture.Width * bytesPerPixel;

        //            var pixelData = new byte[pixelColors.Length * bytesPerPixel];
        //            for (var i = 0; i < pixelColors.Length; i++)
        //            {
        //                pixelData[i * bytesPerPixel + 0] = pixelColors[i].R;
        //                pixelData[i * bytesPerPixel + 1] = pixelColors[i].G;
        //                pixelData[i * bytesPerPixel + 2] = pixelColors[i].B;
        //            }

        //            // Create a bitmap source
        //            return BitmapSource.Create(texture.Width, texture.Height, 96, 96, PixelFormats.Rgb24, null, pixelData, stride);
        //        }
        //    }
        //}
    }
}

