using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace ReviewRetriever
{
    static class config
    {

        private static string configFile = "\\config.json";

        public static string ConnectionString 
        {
            get 
            {
                string sRet = null;
                getProperty("ConnectionString", ref sRet);
                return sRet;
            }
            set { }
        }

        public static Model.Store[] Stores
        {
            get {
                Model.Store[] storeRet = new Model.Store[0];
                getProperty("Stores", ref storeRet);
                return storeRet;
            }
            set { }
        }

        public static string LogPath
        {
            get 
            {
                string sRet = null;
                getProperty("LogPath", ref sRet);
                return sRet;
            }
            set { }
        }

        public static string CronJobSchedule 
        {
            get
            {
                string sRet = null;
                getProperty("CronJobSchedule", ref sRet);
                return sRet;
            }
            set { }
        }

        //=========================================================================================

        private static bool getProperty(string propName, ref string property)
        {
            string filepath = Directory.GetCurrentDirectory() + configFile;
            property = null;
            if (File.Exists(filepath))
            {
                string sConfig = File.ReadAllText(filepath);
                Config jsonConfig = JsonConvert.DeserializeObject<Config>(sConfig);

                switch (propName)
                {
                    case "ConnectionString":
                        property = jsonConfig.ConnectionString;
                        break;
                    case "LogPath":
                        property = jsonConfig.LogPath;
                        break;
                    case "CronJobSchedule":
                        property = jsonConfig.CronJobSchedule;
                        break;
                }
            }
            return (property != null);
        }

        private static bool getProperty(string propName, ref Model.Store[] property) 
        {
            string filepath = Directory.GetCurrentDirectory() + configFile;
            property = null;
            if (File.Exists(filepath))
            {
                    string sConfig = File.ReadAllText(filepath);
                    Config jsonConfig = JsonConvert.DeserializeObject<Config>(sConfig);
                switch (propName) 
                {
                    case "Stores":
                        property = jsonConfig.Stores.ToArray();
                        break;                    
                }
            }
            return (property != null);
        }

        struct Config 
        {
            public List<Model.Store> Stores { get; set; }
            public string LogPath { get; set; }
            public string ConnectionString { get; set; }
            public string CronJobSchedule { get; set; }
        }
    }
}
