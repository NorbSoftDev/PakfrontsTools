using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class SowStr : LogisticsEntry {

        public string value { get; protected set; }
        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            this.id = csv[0];
            
            //TODO make this a change by language
            this.value = csv[1];
        }

        public override string ToString() {
            return id+":"+value;
        }
    }    
}
