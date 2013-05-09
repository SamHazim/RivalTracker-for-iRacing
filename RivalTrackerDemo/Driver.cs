using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RivalTrackerDemo
{
    public class Driver
    {
        public Driver() 
        {
        }

        public int Index { get; set; }

        public string Name { get; set; }

        public string CarNum { get; set; }

        public float LapPct { get; set; }

        public bool OnPitRoad { get; set; }
    }
}
