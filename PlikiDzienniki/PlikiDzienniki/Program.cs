using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PlikiDzienniki
{
    internal class Program
    {
        static string sciezka;
        static int czas;
        static string dziennik;
        static string zrodlo;
        static LogLevel logowanie;


        static void Main(string[] args)
        {
            sciezka = ConfigurationManager.AppSettings["Sciezka"];
            czas = int.Parse(ConfigurationManager.AppSettings["Czas"]);
            dziennik = ConfigurationManager.AppSettings["Dziennik"];
            zrodlo = ConfigurationManager.AppSettings["Zrodlo"];
            TraceSwitch traceSwitch = new TraceSwitch("Logowanie", "Poziom logowania");
            logowanie = (LogLevel)traceSwitch.Level;


            if (!EventLog.Exists(dziennik)) 
            {
                EventLog.CreateEventSource(zrodlo, dziennik);
            }

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = sciezka;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;

            watcher.Changed += (sender, e) => LogujZdarzenie("Info", e.FullPath, "Zmiana zawartości pliku");
            watcher.Changed += (sender, e) => LogujZdarzenie("Error", e.FullPath, "Utworzenie pliku");
            watcher.Changed += (sender, e) => LogujZdarzenie("Error", e.FullPath, "Usuniecie pliku");
            watcher.Changed += (sender, e) => LogujZdarzenie("Warning", e.FullPath, $"Zmiana nazwy pliku na {e.FullPath}");
            if(logowanie == LogLevel.Info || logowanie == LogLevel.Warning || logowanie == LogLevel.Error)
            {
                watcher.EnableRaisingEvents = true;
            }
                
            
            Thread.Sleep(czas * 10000);
            watcher.EnableRaisingEvents = false;

        }

        static void LogujZdarzenie(string poziom, string sciezka, string opis)
        {    
                switch (poziom)
                {
                    case "Info":
                        logowanie = LogLevel.Info;
                        break;
                    case "Error":
                        logowanie = LogLevel.Error;
                        break;
                    case "Warning":
                        logowanie = LogLevel.Warning;
                        break;  
                }
                using (EventLog eventLog = new EventLog(dziennik)) 
                {
                eventLog.Source = zrodlo;

                    switch (logowanie)
                    {

                    case LogLevel.Info:
                            eventLog.WriteEntry($"{opis}: {sciezka}", EventLogEntryType.Information);
                            break;
                        case LogLevel.Warning:
                            eventLog.WriteEntry($"{opis}: {sciezka}", EventLogEntryType.Warning);
                            break;
                        case LogLevel.Error:
                            eventLog.WriteEntry($"{opis}: {sciezka}", EventLogEntryType.Error);
                            break;
                        default:
                            eventLog.WriteEntry($"{opis}: {sciezka}", EventLogEntryType.Information);
                            break;
                    }
                }

               
            
        }
        static bool isLogEnable(string logLevelSetting, string currentLogLevel)
        {
            if(string.IsNullOrEmpty(logLevelSetting) || string.IsNullOrEmpty(currentLogLevel))
            {
                return false;
            }

            int configuredLogLevel = Array.IndexOf(new string[] { "info", "warning", "error" }, logLevelSetting.ToLower());
            int currentLogLEvelIndex = Array.IndexOf(new string[] { "info", "warning", "error" }, currentLogLevel.ToLower());

            return currentLogLEvelIndex >= configuredLogLevel;
        }
        enum LogLevel
        {
            Off = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }
    }

}
