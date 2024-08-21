using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class Graphic : LogisticsEntryBitmap {
        public Graphic() : base() {}

        public int cellWidth, cellHeight, sourceX, sourceY;

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try {

                id = csv[i++];
                file = csv[i++];
                cellWidth = csv[i++].ToInt32();
                cellHeight = csv[i++].ToInt32();
                sourceX = csv[i++].ToInt32();
                sourceY = csv[i++].ToInt32();

                ResetNiceName(config);

            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Error(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }
        }

        public string ImageMagickExtractCommand(Config config) {
            return string.Format("convert {0} -crop {1}x{2}+{3}+{4} +repage -resize 64x64\\> /home/tims/png/{5}.png",
                "/home/tims/Dropbox/SOWIO/SOWWL/SDK/SOWScenarioEditor/rose.png",//config.FindGraphic(file), 
                cellWidth, cellHeight, sourceX, sourceY,
                id);
        }


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


}