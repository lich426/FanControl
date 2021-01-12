﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Reflection;

namespace FanCtrl
{
    public class OSDManager
    {
        private string mOSDFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "OSD.json";

        private static OSDManager sManager = new OSDManager();
        public static OSDManager getInstance() { return sManager; }

        private Object mLock = new Object();

        private bool mIsUpdate = false;
        public bool IsUpdate
        {
            get
            {
                Monitor.Enter(mLock);
                bool isUpdate = mIsUpdate;
                Monitor.Exit(mLock);
                return isUpdate;
            }
            set
            {
                Monitor.Enter(mLock);
                mIsUpdate = value;
                Monitor.Exit(mLock);
            }
        }

        private bool mIsEnable = false;
        public bool IsEnable
        {
            get
            {
                Monitor.Enter(mLock);
                bool isEnable = mIsEnable;
                Monitor.Exit(mLock);
                return isEnable;
            }
            set
            {
                Monitor.Enter(mLock);
                mIsEnable = value;
                Monitor.Exit(mLock);
            }
        }

        private bool mIsTime = false;
        public bool IsTime
        {
            get
            {
                Monitor.Enter(mLock);
                bool isTime = mIsTime;
                Monitor.Exit(mLock);
                return isTime;
            }
            set
            {
                Monitor.Enter(mLock);
                mIsTime = value;
                Monitor.Exit(mLock);
            }
        }

        private List<OSDGroup> mGroupList = new List<OSDGroup>();

        private OSDManager()
        {

        }

        private void clear()
        {
            mIsEnable = false;
            mGroupList.Clear();
        }

        public void reset()
        {
            Monitor.Enter(mLock);
            this.clear();
            Monitor.Exit(mLock);
        }

        public void read()
        {
            Monitor.Enter(mLock);
            String jsonString;
            try
            {
                jsonString = File.ReadAllText(mOSDFileName);
            }
            catch
            {
                this.clear();
                Monitor.Exit(mLock);
                this.write();
                return;
            }

            try
            {
                var rootObject = JObject.Parse(jsonString);

                mIsEnable = (rootObject.ContainsKey("enable") == true) ? rootObject.Value<bool>("enable") : false;
                mIsTime = (rootObject.ContainsKey("time") == true) ? rootObject.Value<bool>("time") : false;

                if (rootObject.ContainsKey("group") == true)
                {
                    var groupList = rootObject.Value<JArray>("group");
                    for(int i = 0; i < groupList.Count; i++)
                    {
                        var groupObject = (JObject)groupList[i];

                        string name = (groupObject.ContainsKey("name") == false) ? "" : groupObject.Value<string>("name");
                        bool isColor = (groupObject.ContainsKey("isColor") == false) ? false : groupObject.Value<bool>("isColor");
                        byte r = (groupObject.ContainsKey("r") == false) ? (byte)0xFF : groupObject.Value<byte>("r");
                        byte g = (groupObject.ContainsKey("g") == false) ? (byte)0xFF : groupObject.Value<byte>("g");
                        byte b = (groupObject.ContainsKey("b") == false) ? (byte)0xFF : groupObject.Value<byte>("b");
                        int digit = (groupObject.ContainsKey("digit") == false) ? 5 : groupObject.Value<int>("digit");

                        var group = new OSDGroup();
                        group.Name = name;
                        group.IsColor = isColor;
                        group.Color = Color.FromArgb(r, g, b);
                        group.Digit = digit;

                        if (groupObject.ContainsKey("item") == true)
                        {
                            var itemList = groupObject.Value<JArray>("item");
                            for(int j = 0; j < itemList.Count; j++)
                            {
                                var itemObject = (JObject)itemList[j];

                                var itemType = (itemObject.ContainsKey("itemType") == false) ? (int)OSDItemType.Unknown : itemObject.Value<int>("itemType");
                                var unitType = (itemObject.ContainsKey("unitType") == false) ? (int)OSDUnitType.Unknown : itemObject.Value<int>("unitType");
                                int index = (itemObject.ContainsKey("index") == false) ? 0 : itemObject.Value<int>("index");
                                isColor = (itemObject.ContainsKey("isColor") == false) ? false : itemObject.Value<bool>("isColor");
                                r = (itemObject.ContainsKey("r") == false) ? (byte)0xFF : itemObject.Value<byte>("r");
                                g = (itemObject.ContainsKey("g") == false) ? (byte)0xFF : itemObject.Value<byte>("g");
                                b = (itemObject.ContainsKey("b") == false) ? (byte)0xFF : itemObject.Value<byte>("b");

                                if (itemType >= (int)OSDItemType.Unknown || unitType >= (int)OSDUnitType.Unknown)
                                    continue;

                                var item = new OSDItem();
                                item.ItemType = (OSDItemType)itemType;
                                item.UnitType = (OSDUnitType)unitType;
                                item.Index = index;
                                item.IsColor = isColor;
                                item.Color = Color.FromArgb(r, g, b);
                                group.ItemList.Add(item);
                            }
                        }

                        mGroupList.Add(group);
                    }
                }
            }
            catch
            {
                this.clear();
                Monitor.Exit(mLock);
                return;
            }

            Monitor.Exit(mLock);
        }

        public void write()
        {
            Monitor.Enter(mLock);
            try
            {
                var rootObject = new JObject();
                rootObject["enable"] = mIsEnable;
                rootObject["time"] = mIsTime;

                var groupList = new JArray();
                for(int i = 0; i < mGroupList.Count; i++)
                {
                    var group = mGroupList[i];

                    var groupObject = new JObject();
                    groupObject["name"] = group.Name;
                    groupObject["isColor"] = group.IsColor;
                    groupObject["r"] = group.Color.R;
                    groupObject["g"] = group.Color.G;
                    groupObject["b"] = group.Color.B;
                    groupObject["digit"] = group.Digit;

                    var itemList = new JArray();
                    for (int j = 0; j < group.ItemList.Count; j++)
                    {
                        var item = group.ItemList[j];

                        var itemObject = new JObject();
                        itemObject["itemType"] = (int)item.ItemType;
                        itemObject["unitType"] = (int)item.UnitType;
                        itemObject["index"] = item.Index;
                        itemObject["isColor"] = item.IsColor;
                        itemObject["r"] = item.Color.R;
                        itemObject["g"] = item.Color.G;
                        itemObject["b"] = item.Color.B;

                        itemList.Add(itemObject);
                    }
                    groupObject["item"] = itemList;

                    groupList.Add(groupObject);
                }
                rootObject["group"] = groupList;

                File.WriteAllText(mOSDFileName, rootObject.ToString());
            }
            catch
            {
                this.clear();
            }
            Monitor.Exit(mLock);
        }

        public int getGroupCount()
        {
            Monitor.Enter(mLock);
            int count = mGroupList.Count;
            Monitor.Exit(mLock);
            return count;
        }

        public OSDGroup getGroup(int index)
        {
            Monitor.Enter(mLock);
            if(index >= mGroupList.Count)
            {
                Monitor.Exit(mLock);
                return null;
            }
            var group = mGroupList[index];
            Monitor.Exit(mLock);
            return group;
        }

        public void setGroupList(List<OSDGroup> groupList)
        {
            Monitor.Enter(mLock);
            mGroupList.Clear();
            for (int i = 0; i < groupList.Count; i++)
            {
                mGroupList.Add(groupList[i].clone());
            }
            Monitor.Exit(mLock);
        }

        public List<OSDGroup> getCloneGroupList()
        {
            Monitor.Enter(mLock);
            var groupList = new List<OSDGroup>();
            for (int i = 0; i < mGroupList.Count; i++)
            {
                groupList.Add(mGroupList[i].clone());
            }
            Monitor.Exit(mLock);
            return groupList;
        }
    }
}
