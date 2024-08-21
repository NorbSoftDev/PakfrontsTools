using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media;

using System.IO;

using NorbSoftDev.SOW;


namespace NorbSoftDev.SOW.Utils
{
    public static class MapTools
    {

        public static Random random = new Random();

        public static MapArea[,] GetGrid(this Map map, int nSouthGrids, int nEastGrids, TerrainBitmap terrainBitmap, List<Terrain> allowedTerrain)
        {
            int extent = (int)map.extent;


            int southLength = extent / nSouthGrids;
            int eastLength = extent / nEastGrids;

            Console.WriteLine("Map extent "+extent+" Grid Size "+eastLength+"x"+southLength);


            MapArea[,] areas = new MapArea[nSouthGrids, nEastGrids];

            for (int s = 0; s < nSouthGrids; s++)
            {
                for (int e = 0; e < nEastGrids; e++)
                {
                    areas[s, e] = new MapArea(
                        s + "," + e,
                        map,
                        s * southLength, e * eastLength,
                        southLength, eastLength,
                        terrainBitmap, allowedTerrain
                        );
                }
            }

            return areas;
        }

        public static Matrix GetWorldMatrix(this WorldTransform worldTransform)
        {
            Matrix m = new Matrix();
            m.Rotate(worldTransform.facing);
            m.OffsetX = worldTransform.east;
            m.OffsetY = worldTransform.south;
            return m;
        }

        public static Matrix GetRotationMatrix(this WorldTransform worldTransform)
        {
            Matrix m = new Matrix();
            m.Rotate(worldTransform.facing);
            return m;
        }

        public static void TransformBy(this WorldTransform worldTransform, WorldTransform transformBy)
        {
            // TODO not really working for rotationed, just for testing
            worldTransform.south += transformBy.south;
            worldTransform.east += transformBy.east;
        }




        //public static Bitmap GetBitmap(this Map map)
        //{
        //    //map.Load();
        //    string path = map.grayscaleFilePath;
        //    Console.WriteLine("Getting bitmap for \""+map.name+"\" \""+path+"\"");
        //    Bitmap bitmap = new Bitmap(path, false);
        //    return bitmap;
        //}

        //public static Terrain GetTerrain(this TerrainBitmap bitmap, Map map, int south, int east) {
        //    int x = south;
        //    int y = east;

        //    if (x < 0 || x > bitmap.width || y  < 0 || y > bitmap.height) return null;
        //    int r = bitmap.GetPixel(x,y).R;
        //    Console.WriteLine("Lookup:"+x+","+y+" = "+r);

        //    return map.GetTerrainAtIndex(r);
        //}

        // public static int? GetGrayValue(this Bitmap bitmap, int x, int y) {
        //     if (x < 0 || x > bitmap.Width || y  < 0 || y > bitmap.Height) return null;
        //     return bitmap.GetPixel(x,y).R;
        // }

        //public static Terrain GetTerrain(this Bitmap bitmap, Map map, int south, int east) {
        //    int x = south;
        //    int y = east;

        //    if (x < 0 || x > bitmap.Width || y  < 0 || y > bitmap.Height) return null;
        //    int r = bitmap.GetPixel(x,y).R;
        //    Console.WriteLine("Lookup:"+x+","+y+" = "+r);

        //    return map.GetTerrainAtIndex(r);
        //}

        // public static byte[] ToByteArray(this Image image, ImageFormat format)
        // {
        //     using(MemoryStream ms = new MemoryStream())
        //     {
        //         image.Save(ms, format);
        //         return ms.ToArray();
        //     }
        // }

        // public static byte[] ToByteArray(this Image image) {
        //     using(MemoryStream ms = new MemoryStream())
        //     {
        //         image.Save(ms, ImageFormat.Bmp);
        //         byte [] ar = ms.ToArray();
        //         Console.WriteLine("Created Byte Array "+ar.Length+" Channels:"+( ar.Length/(image.Width*image.Height) ));
        //         return ar;
        //     }
        // }
        // public static Terrain GetTerrain(this byte [] bytes, Map map, int south, int east, int width, int height) {
        //     int x = south;
        //     int y = east;

        //     int i = (4*(y * width + x))  + 1;

        //     //i = bytes.Length - i - 1;
        //     //i -= 2;

        //     if ( i > bytes.Length) return null;
        //     byte r = bytes[i];
        //     Console.WriteLine("Lookup:"+x+","+y+" "+i+" = "+r);
        //     return map.GetTerrainAtIndex((int)r);
        // }

        // public static byte[] BitmapToByteArray(Bitmap bitmap)
        // {

        //     BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //     int numbytes = bmpdata.Stride * bitmap.Height;
        //     byte[] bytedata = new byte[numbytes];
        //     IntPtr ptr = bmpdata.Scan0;

        //     Marshal.Copy(ptr, bytedata, 0, numbytes);

        //     bitmap.UnlockBits(bmpdata);

        //     return bytedata;

        // }

    }


    public class MapArea
    {
        public MapArea(string name, Map map, int south, int east, int southLength, int eastLength, TerrainBitmap lockBitmap, List<Terrain> allowedTerrain)
        {
            this.name = name;
            this.map = map;
            this.south = south;
            this.east = east;
            this.southLength = southLength;
            this.eastLength = eastLength;


            SetAllowedTerrain(lockBitmap, allowedTerrain);

        }


        public void SetAllowedTerrain(TerrainBitmap terrainBitmap, List<Terrain> allowedTerrain ) {
            if (allowedPositions  != null) allowedPositions.Clear();
            if (terrainBitmap == null) return;
            if (allowedTerrain == null) return;

            if (allowedPositions  == null) allowedPositions = new List<Position>();
            if (allowedTerrain.Count < 1) return;


            double xPixelToUnit = map.extent/terrainBitmap.width;
            double xp = east/(double)map.extent;
            double xl = eastLength/(double)map.extent;

            int xstart = (int)(terrainBitmap.width * xp);
            int xend = (int)(terrainBitmap.width * (xp + xl));

            double yPixelToUnit = map.extent/terrainBitmap.height;
            double yp = south/(double)map.extent;
            double yl = southLength/(double)map.extent;

            int ystart = (int)(terrainBitmap.height * yp);
            int yend = (int)(terrainBitmap.height * (yp + yl));

            Console.WriteLine(name+" Image Domain "+xstart+","+ystart+" to "+xend+","+yend);

            int [] histogram = new int[256];
            int nulls = 0;
            int max = 0;
            for (int y = ystart; y < yend; y++ ) {
                for (int x = xstart; x < xend; x++ ) {

                    if (x >= terrainBitmap.width || y >= terrainBitmap.height) {
                        Console.WriteLine("Out of bounds"+x,y);
                    }


                    Terrain terrain = terrainBitmap.GetTerrainAtPixel(x,y);
                    if (terrain == null) {
                        nulls++;
                        continue;
                    }

                    histogram[terrain.grayscale]++; 
                    if (histogram[terrain.grayscale] > max) {
                        max = histogram[terrain.grayscale];
                    }

                    if ( allowedTerrain.Contains(terrain) ) {
                        allowedPositions.Add( new Position( (int)(x*xPixelToUnit), (int)(y*yPixelToUnit) ) );
                    }
                } 
            }

            int npixels = nulls;
            Console.WriteLine("nulls:"+nulls);

            for (int i = 0; i < histogram.Length; i++) {
                int n = histogram[i];
                npixels += n;
                if (n == 0) continue;
                Terrain terrain = map.GetTerrainAtIndex((int)i);
                if (terrain == null) {
                    Console.WriteLine(i+" ?? "+n);

                } else {
                    int c = (40 * n)/max;
                // Console.WriteLine(terrain.grayscale+" "+terrain.id+" "+n);
                Console.WriteLine( String.Format("{0,3} {1,-24} {2,8} {3}", terrain.grayscale, terrain.id, n,  new String('-',c) ) );

                }
            }
            Console.WriteLine("npixels:"+npixels);




            if (allowedPositions.Count < 1) {
                Log.Warn(this, this.name+" No Allowed Positions found");
            }
        }

        public Map map;
        public string name;
        public int south, east, southLength, eastLength;

        public Bitmap terrainBitmap;
        public List<Position> allowedPositions;


        public Position GetRandomPosition()
        {


            return new Position(
                south + MapTools.random.Next(southLength),
                east + MapTools.random.Next(eastLength)
                );
        }

        public Position GetRandomAllowedPosition()
        {
            if (allowedPositions == null) return GetRandomPosition();
            if (allowedPositions.Count < 1) return null;


            return new Position( allowedPositions[ MapTools.random.Next(allowedPositions.Count) ] );


        }


        public override string ToString()
        {
            return east + "," + south + ":" + eastLength + "x" + southLength;
        }

        // public List<Position> GetRoadPositions() {

        // }

        // public List<Position> GetRandomBorderPosition() {

        // }
    }
}

