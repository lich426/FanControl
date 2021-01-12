﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace FanCtrl
{
    public enum LibraryType
    {
        LibreHardwareMonitor = 0,
        OpenHardwareMonitor,
    };

    public class OptionManager
    {
        private string mOptionFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "Option.json";

        private static OptionManager sManager = new OptionManager();
        public static OptionManager getInstance() { return sManager; }

        private StartupControl mStartupControl = new StartupControl();

        private OptionManager()
        {
            Interval = 1000;
            IsGigabyte = false;
            LibraryType = LibraryType.LibreHardwareMonitor;
            IsNvAPIWrapper = false;
            IsDimm = true;
            IsKraken = true;
            IsCLC = true;
            IsRGBnFC = true;
            IsAnimation = true;
            IsFahrenheit = false;
            IsMinimized = false;
            IsStartUp = false;
        }

        public int Interval { get; set; }

        public bool IsGigabyte { get; set; }

        public LibraryType LibraryType { get; set; }

        public bool IsNvAPIWrapper { get; set; }

        public bool IsDimm { get; set; }

        public bool IsKraken { get; set; }

        public bool IsCLC { get; set; }

        public bool IsRGBnFC { get; set; }

        public bool IsAnimation { get; set; }

        public bool IsFahrenheit { get; set; }

        public bool IsMinimized { get; set; }

        public int DelayTime
        {
            get
            {
                return mStartupControl.DelayTime;
            }
            set
            {

                mStartupControl.DelayTime = value;
            }
        }

        public bool IsStartUp
        {
            get
            {
                return mStartupControl.Startup;
            }
            set
            {

                mStartupControl.Startup = value;
            }
        }

        public bool read()
        {
            try
            {
                var jsonString = File.ReadAllText(mOptionFileName);
                var rootObject = JObject.Parse(jsonString);

                Interval = (rootObject.ContainsKey("interval") == true) ? rootObject.Value<int>("interval") : 1000;

                IsGigabyte = (rootObject.ContainsKey("gigabyte") == true) ? rootObject.Value<bool>("gigabyte") : false;

                if (rootObject.ContainsKey("library") == false)
                    LibraryType = LibraryType.LibreHardwareMonitor;
                else
                    LibraryType = (rootObject.Value<int>("library") == 0) ? LibraryType.LibreHardwareMonitor : LibraryType.OpenHardwareMonitor;

                IsNvAPIWrapper = (rootObject.ContainsKey("nvapi") == true) ? rootObject.Value<bool>("nvapi") : false;
                IsDimm = (rootObject.ContainsKey("dimm") == true) ? rootObject.Value<bool>("dimm") : true;
                IsKraken = (rootObject.ContainsKey("kraken") == true) ? rootObject.Value<bool>("kraken") : true;
                IsCLC = (rootObject.ContainsKey("clc") == true) ? rootObject.Value<bool>("clc") : true;
                IsRGBnFC = (rootObject.ContainsKey("rgbnfc") == true) ? rootObject.Value<bool>("rgbnfc") : true;
                IsAnimation = (rootObject.ContainsKey("animation") == true) ? rootObject.Value<bool>("animation") : true;
                IsFahrenheit = (rootObject.ContainsKey("fahrenheit") == true) ? rootObject.Value<bool>("fahrenheit") : false;
                IsMinimized = (rootObject.ContainsKey("minimized") == true) ? rootObject.Value<bool>("minimized") : false;
                IsStartUp = (rootObject.ContainsKey("startup") == true) ? rootObject.Value<bool>("startup") : false;
                DelayTime = (rootObject.ContainsKey("delay") == true) ? rootObject.Value<int>("delay") : 0;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void write()
        {
            try
            {
                var rootObject = new JObject();
                rootObject["interval"] = Interval;
                rootObject["gigabyte"] = IsGigabyte;
                rootObject["library"] = (LibraryType == LibraryType.LibreHardwareMonitor) ? 0 : 1;
                rootObject["dimm"] = IsDimm;
                rootObject["nvapi"] = IsNvAPIWrapper;
                rootObject["kraken"] = IsKraken;
                rootObject["clc"] = IsCLC;
                rootObject["rgbnfc"] = IsRGBnFC;
                rootObject["animation"] = IsAnimation;
                rootObject["fahrenheit"] = IsFahrenheit;
                rootObject["minimized"] = IsMinimized;
                rootObject["startup"] = IsStartUp;
                rootObject["delay"] = DelayTime;
                File.WriteAllText(mOptionFileName, rootObject.ToString());
            }
            catch {}
        }

    }
}
