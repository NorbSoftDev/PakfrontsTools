using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScenarioEditor
{
    /// <summary>
    //App.config needs startup modified to support older dlls of DeVIL
    /// \<startup useLegacyV2RuntimeActivationPolicy="true">
    /// </summary>
    static class GraphicsManager
    {

        static Dictionary<Graphic, BitmapSource> bitmapSources = new Dictionary<Graphic, BitmapSource>();
        static Dictionary<string, System.Drawing.Bitmap> bitmaps = new Dictionary<string, System.Drawing.Bitmap>(StringComparer.OrdinalIgnoreCase);

        static bool DiDWriteImageMagickHeader = false;


        public static BitmapSource GetBitmapSource(this NorbSoftDev.SOW.Graphic graphic, Config config)
        {
            if (graphic == null) return null;
            if (graphic.file == null) return null;

            BitmapSource bitmapSource = null;
            if (bitmapSources.TryGetValue(graphic, out bitmapSource)) return bitmapSource;

            System.Drawing.Bitmap bmp;

            if (! bitmaps.TryGetValue(graphic.file, out bmp) ) {
                string filepath = config.FindGraphic(graphic.file);

                if (!File.Exists(filepath)) {
                    // cannot find file, and mark as null
                    bitmaps[graphic.file] = null;
                    bitmapSources[graphic] = null;
                    Log.Warn(graphic, graphic.id+" "+graphic.file+" Unable to find image file " + filepath);
                    return null;
                }

                //App..config needs startup modified to support older dlls of DeVIL
                //<configuration>
                //<startup useLegacyV2RuntimeActivationPolicy="true">
                //  <supportedRuntime version="v4.0"/>
                //</startup>
                //</configuration>
                bmp = DevIL.DevIL.LoadBitmap(filepath);
                bitmaps[graphic.file] = bmp;

                if (bmp == null)
                {
                    bitmaps[graphic.file] = null;
                    bitmapSources[graphic] = null;
                    Log.Warn(graphic, graphic.id + " " + graphic.file + " Unable to load image file " + filepath);
                    return null;
                }
            }

            if (bmp == null)
            {
                return null;
            }

            System.Drawing.Rectangle cropArea = new System.Drawing.Rectangle(
                graphic.sourceX, graphic.sourceY,
                graphic.cellWidth, graphic.cellHeight); 

            System.Drawing.Bitmap bmpCrop = bmp.Clone(
                cropArea,
                bmp.PixelFormat);

            bitmapSource = bmpCrop.ToBitmapSource();
            bitmapSources[graphic] = bitmapSource;
            
            return bitmapSource;
        }

        //public static BitmapImage GetBitmapImageOLD(this NorbSoftDev.SOW.Graphic graphic, Config config)
        //{
        //    if (graphic == null) return null;

        //    BitmapImage bitmapImage = null;
        //    bitmapSources.TryGetValue(graphic, out bitmapImage);
        //    if (bitmapImage != null) return bitmapImage;

        //    // http://stackoverflow.com/questions/11880946/how-to-load-image-to-wpf-in-runtime
        //    string filepath = config.GetResourceFile(
        //        "Thumbnails",
        //        String.Format("{0}.png", graphic.id)
        //        );

        //    Uri uri = new Uri(filepath);

        //    if (!File.Exists(filepath))
        //    {

        //        string sowFilepath = config.FindGraphic(graphic.file);
        //        if (sowFilepath == null) return null;

        //        string batfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SOWWL", "CreateThumbnails.bat");

        //        if (!DiDWriteImageMagickHeader)
        //        {
        //            System.IO.File.WriteAllText(batfile,
        //                "SETLOCAL EnableDelayedExpansion" + Environment.NewLine +
        //                "SET IMCONV=\"C:/Program Files/ImageMagick-6.8.6-Q16/convert.exe\"" + Environment.NewLine
        //            );
        //            DiDWriteImageMagickHeader = true;
        //        }
        //        // write to a file that we can run with ImageMagick later
        //        // a real hack, since I cant get tga and dds reading working


        //        string args = String.Format("\"{0}\" -crop {1}x{2}+{3}+{4} +repage -resize 64x64 \"{5}\"",
        //            sowFilepath,
        //            graphic.cellWidth, graphic.cellHeight, graphic.sourceX, graphic.sourceY,
        //            filepath);

        //        using (StreamWriter sw = File.AppendText(batfile))
        //        {
        //            sw.WriteLine("%IMCONV% " + args);

        //        }

        //        Console.WriteLine(args);
        //        return null;


        //        //Process proc = new Process
        //        //{
        //        //    StartInfo = new ProcessStartInfo
        //        //    {
        //        //        FileName = "C:/Program Files/ImageMagick-6.8.6-Q16/convert.exe",
        //        //        Arguments = args,
        //        //        UseShellExecute = false,
        //        //        RedirectStandardOutput = true,
        //        //        CreateNoWindow = true
        //        //    }
        //        //};

        //        //proc.Start();
        //        //while (!proc.StandardOutput.EndOfStream) {
        //        //    string line = proc.StandardOutput.ReadLine();
        //        //    Console.WriteLine(line);
        //        //}

        //    }

        //    bitmapImage = new BitmapImage(uri);

        //    bitmapSources[graphic] = bitmapImage;
        //    return bitmapImage;
        //}

        //should work for bitmapimage as well
        public static Color GetPixelColor(this BitmapSource source, int x, int y)
        {
            Color c = Colors.White;
            if (source != null)
            {
                try
                {
                    CroppedBitmap cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                    var pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    c = Color.FromRgb(pixels[2], pixels[1], pixels[0]);
                }
                catch (Exception) { }
            }
            return c;
        }

        public static Color GetPixelColor(this BitmapImage source, int x, int y)
        {
            return GetPixelColor((BitmapSource)source, x, y);
        }


        public static byte? GetPixelGrayscale(this BitmapSource source, int x, int y)
        {
            if (source != null)
            {
                try
                {
                    CroppedBitmap cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                    var pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    return pixels[0];
                }
                catch (Exception)
                {
                    return null;

                }
            }
            return null;
        }

        public static byte? GetPixelGrayscale(this BitmapImage source, int x, int y)
        {
            return GetPixelGrayscale((BitmapSource)source, x, y);
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Image source)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(source);

            var bitSrc = bitmap.ToBitmapSource();

            bitmap.Dispose();
            bitmap = null;

            return bitSrc;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        /// <summary>
        /// FxCop requires all Marshalled functions to be in a class called NativeMethods.
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }



        //public static  BitmapImage GetBitmapImage(this NorbSoftDev.SOW.Graphic graphic, Config config) {
        //    BitmapImage bitmapImage = null;
        //    _graphicImages.TryGetValue(graphic, out bitmapImage);
        //    if (bitmapImage != null) return bitmapImage;

        //    // http://stackoverflow.com/questions/11880946/how-to-load-image-to-wpf-in-runtime
        //    string filepath =  Path.Combine(Environment.CurrentDirectory, "Thumbnails", 
        //        String.Format("{0}.png",graphic.id) );

        //    var uri = new Uri(filepath);

        //    if ( !File.Exists(filepath) ) {
        //        //create it on the fly
        //        string sowFilepath = config.FindGraphic(graphic.file);

        //        if (sowFilepath == null) return null;

        //        var imImage = new ImageMagickNET.Image(sowFilepath);
        //        imImage.Crop( new ImageMagickNET.Geometry(
        //            string.Format("{0}x{1}+{2}+{3}", graphic.cellWidth, graphic.cellHeight, graphic.sourceX, graphic.sourceY) 
        //            ));
        //        imImage.Resize( new ImageMagickNET.Geometry( "64x64^" ));
        //        Log.Info(graphic, "Writing thumbnail "+filepath);
        //        imImage.Write( filepath) ;
        //    }

        //    bitmapImage = new BitmapImage(uri);

        //    _graphicImages[graphic] = bitmapImage;
        //    return bitmapImage;
        //}

        // public string ImageMagickExtractCommand(Config config) {
        //     return string.Format("convert {0} -crop {1}x{2}+{3}+{4} +repage -resize 64x64\\> /home/tims/png/{5}.png",
        //         "/home/tims/Dropbox/SOWIO/SOWWL/SDK/SOWScenarioEditor/rose.png",//config.FindGraphic(file), 
        //         cellWidth, cellHeight, sourceX, sourceY,
        //         id);
        // }


        // // http://stackoverflow.com/questions/2996973/how-to-use-imagemagick-net-in-net-please-for-examples
        // // http://imagemagick.codeplex.com/SourceControl/latest
        // public string ImageMagickExtract(Config config) {
        //     var image = new ImageMagickNET.Image("/home/tims/Dropbox/SOWIO/SOWWL/SDK/SOWScenarioEditor/rose.png");
        //     image.Crop( new ImageMagickNET.Geometry(
        //         string.Format("{0}x{1}+{2}+{3}", cellWidth, cellHeight, sourceX, sourceY) 
        //         ));
        //     image.Resize( new ImageMagickNET.Geometry( "64x64^" )) ;
        //     image.Write( string.Format("{0}.png",id )) ;
        //     return string.Format("{0}.png",id );
        // }
    }

    // http://www.codeproject.com/Tips/451129/Csharp-SpriteSheet-Reader-Parser
    //class Spritesheet
    //    {
    //        public string Name { get; set; }
    //        public Dictionary<string, Image> Sprites { get; set; }

    //        public Spritesheet(string spriteSheetName)
    //        {
    //            this.Sprites = new Dictionary<string, Image>(50);
    //            this.Name = spriteSheetName;
    //        }

    //        public static Spritesheet LoadSpriteSheet(string coordinatesFile)
    //        {
    //            XmlDocument doc = new XmlDocument();
    //            doc.Load(coordinatesFile);
    //            XmlNode metadata = doc.SelectSingleNode("/plist/dict/key[.='metadata']");
    //            XmlNode realTextureFileName = 
    //            metadata.NextSibling.SelectSingleNode("key[.='realTextureFileName']");
    //            string spritesheetName = realTextureFileName.NextSibling.InnerText;
    //            Image spriteSheetImage = Image.FromFile(spritesheetName);
    //            XmlNode frames = doc.SelectSingleNode("/plist/dict/key[.='frames']");
    //            XmlNodeList list = frames.NextSibling.SelectNodes("key");

    //            Spritesheet spritesheet = new Spritesheet(coordinatesFile);

    //            foreach (XmlNode node in list)
    //            {
    //                XmlNode dict = node.NextSibling;
    //                string strRectangle = dict.SelectSingleNode
    //                ("key[.='frame']").NextSibling.InnerText;
    //                string strOffset = dict.SelectSingleNode
    //                ("key[.='offset']").NextSibling.InnerText;
    //                string strSourceRect = dict.SelectSingleNode
    //                ("key[.='sourceColorRect']").NextSibling.InnerText;
    //                string strSourceSize = dict.SelectSingleNode
    //                ("key[.='sourceSize']").NextSibling.InnerText;
    //                Rectangle frame = parseRectangle(strRectangle);
    //                Point offset = parsePoint(strOffset);
    //                Rectangle sourceRectangle = parseRectangle(strSourceRect);
    //                Point size = parsePoint(strSourceSize);

    //                string spriteFrameName = node.InnerText;
    //                Image sprite = new Bitmap(size.X, size.Y);
    //                Graphics drawer = Graphics.FromImage(sprite);
    //                drawer.DrawImage(spriteSheetImage, sourceRectangle, frame, GraphicsUnit.Pixel);
    //                drawer.Save();
    //                drawer.Dispose();
    //                spritesheet.Sprites.Add(spriteFrameName, sprite);
    //            }
    //            return spritesheet;
    //        }

    //        private static Rectangle parseRectangle(string rectangle)
    //        {
    //            Regex expression = new Regex(@"\{\{(\d+),(\d+)\},\{(\d+),(\d+)\}\}");
    //            Match match = expression.Match(rectangle);
    //            if (match.Success)
    //            {
    //                int x = int.Parse(match.Groups[1].Value);
    //                int y = int.Parse(match.Groups[2].Value);
    //                int w = int.Parse(match.Groups[3].Value);
    //                int h = int.Parse(match.Groups[4].Value);
    //                return new Rectangle(x, y, w, h);
    //            }
    //            return Rectangle.Empty;
    //        }

    //        private static Point parsePoint(string point)
    //        {
    //            Regex expression = new Regex(@"\{(\d+),(\d+)\}");
    //            Match match = expression.Match(point);
    //            if (match.Success)
    //            {
    //                int x = int.Parse(match.Groups[1].Value);
    //                int y = int.Parse(match.Groups[2].Value);
    //                return new Point(x, y);
    //            }
    //            return Point.Empty;
    //        }
    //    }            

}
