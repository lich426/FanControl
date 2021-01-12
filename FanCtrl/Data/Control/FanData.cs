﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanCtrl
{
    public enum FanValueUnit : int
    {
        Size_1 = 0,
        Size_5 = 1,
        Size_10 = 2,
    };

    public class FanData
    {
        public const int MAX_FAN_VALUE_SIZE_1 = 101;
        public const int MAX_FAN_VALUE_SIZE_5 = 21;
        public const int MAX_FAN_VALUE_SIZE_10 = 11;

        public int Index { get; set; }

        public string Name { get; set; }

        public FanValueUnit Unit { get; set; }

        public bool IsStep { get; set; }

        public int Hysteresis { get; set; }

        private int[] mValueList = null;
        public int[] ValueList
        {
            get { return mValueList; }
        }

        public int LastChangedValue { get; set; }
        public int LastChangedTemp { get; set; }

        public FanData(int index, string name, FanValueUnit unit, bool isStep, int hysteresis)
        {
            Index = index;
            Name = name;
            IsStep = isStep;
            Hysteresis = hysteresis;
            Unit = unit;
            mValueList = new int[this.getMaxFanValue()];
            for (int i = 0; i < this.getMaxFanValue(); i++)
            {
                mValueList[i] = 50;
            }
        }

        public FanData clone()
        {
            var fanData = new FanData(Index, Name, Unit, IsStep, Hysteresis);
            for (int i = 0; i < this.getMaxFanValue(); i++)
            {
                fanData.ValueList[i] = mValueList[i];
            }
            return fanData;
        }

        public void setChangeUnitAndFanValue(FanValueUnit newUnit)
        {
            if (Unit == newUnit)
                return;

            if (Unit == FanValueUnit.Size_1)
            {
                // 1 -> 5
                if (newUnit == FanValueUnit.Size_5)
                {
                    var valueList = new int[MAX_FAN_VALUE_SIZE_5];
                    for (int i = 0; i < valueList.Length; i++)
                    {
                        int value = mValueList[i * 5] / 5 * 5;
                        valueList[i] = value;
                    }
                    mValueList = valueList;
                }

                // 1 -> 10
                else if (newUnit == FanValueUnit.Size_10)
                {
                    var valueList = new int[MAX_FAN_VALUE_SIZE_10];
                    for (int i = 0; i < valueList.Length; i++)
                    {
                        int value = mValueList[i * 10] / 10 * 10;
                        valueList[i] = value;
                    }
                    mValueList = valueList;
                }
            }

            else if (Unit == FanValueUnit.Size_5)
            {
                // 5 -> 10
                if (newUnit == FanValueUnit.Size_10)
                {
                    var valueList = new int[MAX_FAN_VALUE_SIZE_10];
                    for (int i = 0; i < valueList.Length; i++)
                    {
                        int value = mValueList[i * 2] / 10 * 10;
                        valueList[i] = value;
                    }
                    mValueList = valueList;
                }

                // 5 -> 1
                else if (newUnit == FanValueUnit.Size_1)
                {
                    int count = 5;
                    var valueList = new int[MAX_FAN_VALUE_SIZE_1];
                    for (int i = 0; i < mValueList.Length; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (i * count + j >= valueList.Length)
                                break;
                            valueList[i * count + j] = mValueList[i];
                        }
                    }
                    mValueList = valueList;
                }
            }

            else if (Unit == FanValueUnit.Size_10)
            {
                // 10 -> 1
                if (newUnit == FanValueUnit.Size_1)
                {
                    int count = 10;
                    var valueList = new int[MAX_FAN_VALUE_SIZE_1];
                    for (int i = 0; i < mValueList.Length; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (i * count + j >= valueList.Length)
                                break;
                            valueList[i * count + j] = mValueList[i];
                        }
                    }
                    mValueList = valueList;
                }

                // 10 -> 5
                else if (newUnit == FanValueUnit.Size_5)
                {
                    int count = 2;
                    var valueList = new int[MAX_FAN_VALUE_SIZE_5];
                    for (int i = 0; i < mValueList.Length; i++)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (i * count + j >= valueList.Length)
                                break;
                            valueList[i * count + j] = mValueList[i];
                        }
                    }
                    mValueList = valueList;
                }
            }
            Unit = newUnit;
        }

        public int getMaxFanValue()
        {
            if (Unit == FanValueUnit.Size_1) return MAX_FAN_VALUE_SIZE_1;
            else if (Unit == FanValueUnit.Size_5) return MAX_FAN_VALUE_SIZE_5;
            else return MAX_FAN_VALUE_SIZE_10;
        }

        public double getDivideValue()
        {
            if (Unit == FanValueUnit.Size_1) return 1.0;
            else if (Unit == FanValueUnit.Size_5) return 5.0;
            else return 10.0;
        }

        public int getValue(int temperature)
        {
            if (temperature >= 100)
            {
                LastChangedTemp = 100;
                LastChangedValue = mValueList[this.getMaxFanValue() - 1];
                return mValueList[this.getMaxFanValue() - 1];
            }

            double divide = temperature / this.getDivideValue();
            int prevIndex = (int)Math.Truncate(divide);
            int nextIndex = (int)Math.Ceiling(divide);

            int prevValue = mValueList[prevIndex];
            int nextValue = mValueList[nextIndex];

            // step
            if (IsStep == true)
            {
                if(LastChangedValue == 0 || prevValue >= LastChangedValue ||
                    LastChangedTemp - temperature > Hysteresis)    // check hysteresis
                {
                    int minIndex = this.getSameValueMinIndex(prevIndex);
                    LastChangedValue = mValueList[minIndex];
                    LastChangedTemp = minIndex * (int)this.getDivideValue();
                }
                return LastChangedValue;
            }

            // same temperature
            if (prevValue == nextValue)
            {
                return prevValue;
            }

            double x1 = prevIndex * this.getDivideValue();
            double y1 = (double)prevValue;

            double x2 = nextIndex * this.getDivideValue();
            double y2 = (double)nextValue;

            double slope = (y2 - y1) / (x2 - x1);
            double result = slope * ((double)temperature - x1) + y1;
            return (int)Math.Round(result);
        }

        private int getSameValueMinIndex(int index)
        {
            int minIndex = index;
            int nowValue = mValueList[index];
            for(int i = index - 1; i >= 0; i--)
            {
                if (nowValue == mValueList[i])
                {
                    minIndex = i;
                    continue;
                }
                break;
            }
            return minIndex;
        }
    }
}
