using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NorbSoftDev.SOW
{
    class ScreenReader
    {
        delegate ScreenMethodDelegate ScreenMethodDelegate();
        Dictionary<string, ScreenMessage> screenMessages;
        string currentLine;
        ScreenMessage screenMessage;


        public static void ReadScreen( Dictionary<string, ScreenMessage> screenMessages, string filepath)
        {
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            ReadScreens(screenMessages, stream);
            stream.Close();
        }


        public static void ReadScreens( Dictionary<string, ScreenMessage> screenMessages, Stream stream)
        {
            ScreenReader reader = new ScreenReader();
            reader.StartRead(screenMessages, stream);
        }

        // void StartReadFormations(Dictionary<string, ScreenMessage> screenMessages, string filepath) {
        //     FileStream stream = new FileStream(filepath, , FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //     ReadScreen(stream);
        //     stream.Close();
        // }


        void StartRead(Dictionary<string, ScreenMessage> screenMessages, Stream stream)
        {
            this.screenMessages = screenMessages;

            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null) {
                streamName = fs.Name;
            }

            Log.Info(this,"ReadScreen "+streamName);

            StreamReader sr = new StreamReader(stream, Config.TextFileEncoding);
            ScreenMethodDelegate screenMethod = ReadFirstLine;

            while (sr.Peek() >= 0) 
            {
                // Console.WriteLine(sr.Peek());
                currentLine = sr.ReadLine();
                // Console.WriteLine(currentLine);                
                screenMethod = screenMethod();
            } 
        }

        ScreenMethodDelegate ReadFirstLine() {
            if (! currentLine.StartsWith("$")) {
                return ReadFirstLine;
            }

            List<string> parts = new List<string>(currentLine.Split());
            string id = parts[0].Substring(1);
            parts.RemoveAt(0);
            List<string> contents = new List<string>();
            contents.Add(String.Join(" ",parts.ToArray() ) );
            screenMessage = new ScreenMessage(id, contents);
            screenMessages[id] = screenMessage;
            return ReadFollowingLines;

        }

        ScreenMethodDelegate ReadFollowingLines() {
            if (currentLine.StartsWith("$")) {
                return ReadFirstLine(); 
            }

            //screenMessage.lines.Add(currentLine);
            screenMessage.AddLine(currentLine);
            return ReadFollowingLines;
        } 
    }
}