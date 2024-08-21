using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NorbSoftDev.SOW
{

    public class ScreenMessage : IHasId, INotifyPropertyChanged
    {
        string _id;
        public string id {
            get { return _id; }

            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged("id");
            }
        }
        //public List<string> lines { get; set;}
        //public string contents { 
        //    get {
        //        return String.Join(Environment.NewLine, lines);
        //    }
        //}


        string _contents;
        public string contents
        {
            get { return _contents; }

            set
            {
                if (value == _contents) return;
                _contents = value;
                OnPropertyChanged("contents");
            }
        }

        public ScreenMessage(string id, List<string> lines)
        {
            this.id = id;
            this.contents = "";
            foreach (string line in lines) {
                this.contents +=  line+Environment.NewLine;
            } 
            //this.contents = String.Join(Environment.NewLine, lines);
        }


        //public ScreenMessage (string id, List<string> lines) {
        //    this.id = id;
        //    this.lines = lines;
        //}

        public void AddLine(string currentLine)
        {
            this.contents += currentLine + Environment.NewLine;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}