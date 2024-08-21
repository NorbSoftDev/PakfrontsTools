#define DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
#if UNITY_STANDALONE
using UnityEngine;
#endif

namespace NorbSoftDev.SOW {


    public static class Log {


        static public ObservableCollection<string> Warnings { get; set; }
        static public ObservableCollection<string> Errors { get; set; }
        static TextWriterTraceListener textWriterTraceListener = null;


        static Log()
        {
            Warnings = new ObservableCollection<string>();
            Errors = new ObservableCollection<string>();
        }

        static public void SetupUserLog()
        {

            if (textWriterTraceListener != null)
            {
                Trace.WriteLine("SOWWL ScenarioEditor Log Continued " + DateTime.Now.ToString());
            }

            Process currentProcess = Process.GetCurrentProcess();


            string logFilePath = Path.Combine(
               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SOWWL"),
               "ScenarioEditor.log");

            Stream logFile;
            try
            {

               logFile = File.Create(logFilePath);

            }
            catch (IOException e)
            {
               
                logFilePath = Path.Combine(
              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SOWWL"),
              "ScenarioEditor-" + currentProcess.Id + ".log");
                logFile = File.Create(logFilePath);
            }

            textWriterTraceListener  = new TextWriterTraceListener(logFile);
            Trace.Listeners.Add(textWriterTraceListener);

            // Write output to the file.
            Trace.WriteLine("SOWWL ScenarioEditor Log Started " + DateTime.Now.ToString());
            Trace.WriteLine(currentProcess.Modules[0].FileName);
            Trace.Flush();


        }

        static public  void CloseUserLog()
        {
            Trace.WriteLine("SOWWL ScenarioEditor Log Ended " + DateTime.Now.ToString());
            Trace.Flush();
            if (textWriterTraceListener != null) {
            textWriterTraceListener.Close();
            }
        }


        public static void Debug(object obj, string msg) {
            Log.Debug(obj == null ? null : obj.GetType(), msg);
        }

        public static void Info(object obj, string msg) {
            Log.Info(obj == null ? null : obj.GetType() ,msg);
        }


        public static void Warn(object obj, string msg) {
            Log.Warn(obj == null ? null : obj.GetType(), msg);
        } 
        

        public static void Error(object obj, string msg) {
            Log.Error(obj == null ? null : obj.GetType(), msg);
            Trace.Flush();

        }

        public static void Exception(object obj, Exception e)
        {
            Log.Error(obj.GetType(), e.StackTrace);
            Log.Error(obj.GetType(), e.Message);
            Trace.Flush();

        }


        public static void Debug(Type type, string msg) {
            //Console.Error.WriteLine("DBG {0,-14} {1}",type.Name,msg);
#if ! UNITY_STANDALONE
			System.Diagnostics.Debug.WriteLine(
                "DBG "+(type == null ? "null" : type.Name)+" "+msg);
#else
			UnityEngine.Debug.Log( 
                (type == null ? "null" : type.Name)
                +" "+msg);
#endif
		}

        public static void Info(Type type, string msg) {
            // Console.Error.WriteLine("INF {0,-14} {1}",type.Name,msg);
#if ! UNITY_STANDALONE
            System.Diagnostics.Debug.WriteLine("INF "+(type == null ? "null" : type.Name)+" "+msg);
#else
			UnityEngine.Debug.Log((type == null ? "null" : type.Name)+" "+msg);
#endif
			
        }


        public static void Warn(Type type, string msg) {
           Warnings.Add(msg);
           // Console.Error.WriteLine("WRN {0,-14} {1}",type.Name,msg);
#if ! UNITY_STANDALONE
           System.Diagnostics.Debug.WriteLine("WRN "+(type == null ? "null" : type.Name)+" "+msg);
#else
			UnityEngine.Debug.LogWarning((type == null ? "null" : type.Name)+" "+msg);
#endif
        } 
        
        public static void Error(Type type, string msg) {
            Errors.Add(msg);
            // Console.Error.WriteLine("ERR {0,-14} {1}",type.Name,msg);
#if ! UNITY_STANDALONE
            System.Diagnostics.Debug.WriteLine("ERR "+(type == null ? "null" : type.Name)+" "+msg);
#else
			UnityEngine.Debug.LogError((type == null ? "null" : type.Name)+" "+msg);
#endif
        }


        public static void Flush()
        {
            Trace.Flush();
        }

    }
}