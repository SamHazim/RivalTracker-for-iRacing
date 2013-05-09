using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RivalTrackerDemo
{
    class TelemData
    {
        private Dictionary<int, Driver> drivers;
        private Int32 trackId = 0;

        public TelemData(Dictionary<int, Driver> drivers)
        {
            this.drivers = drivers;
        }

        public void setTrackId(Int32 trackId)
        {
            this.trackId = trackId;
        }

        public Int32 getTrackId()
        {
            return this.trackId;
        }
        public string toJson()
        {
            StringBuilder json = new StringBuilder("{");

            json.Append("\"trackId\" : " + this.trackId + ", \n");
            json.Append("\"drivers\" : {");


            string separator = "";
            foreach (int carIdx in drivers.Keys)
            {
                Driver driver = drivers[carIdx];
                if ((driver.LapPct != -1)&&(!driver.OnPitRoad))
                {
                    json.Append(separator);
                    json.Append("\"" + driver.CarNum + "\"");
                    json.Append(":");
                    json.Append(driver.LapPct);
                    if (separator == "")
                    {
                        separator = ",";
                    }
                }
            }
            json.Append("} \n}");
            return json.ToString();
        }
    }
}
