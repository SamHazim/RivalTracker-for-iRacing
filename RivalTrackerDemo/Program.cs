using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using iRSDKSharp;
using Fleck;

namespace RivalTrackerDemo
{
    class Program
    {
        private static Dictionary<int, Driver> drivers = new Dictionary<int, Driver>();
        private static Dictionary<String, int> cameras = new Dictionary<String, int>();
        private static iRacingSDK sdk;
        private static int length, start, end;
        private static TelemData telemData = new TelemData(drivers);
        private static string callbackCamera = "TV1";   // camera group to switch to when the client requests it
        static void Main(string[] args)
        {
            sdk = new iRacingSDK();
            int lastUpdate = -1;

            //setup WebSocketServer
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://localhost:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client Connected");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Client Disconnected");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine("Received -> " + message);
                    int groupNum = cameras[callbackCamera];
                    sdk.BroadcastMessage(BroadcastMessageTypes.CamSwitchNum, Convert.ToInt32(message), groupNum, 0);
                };
            });

            while (true)
            {
                if (sdk.IsConnected())
                {
                    //If it is connected then see if the Session Info has been updated
                    int newUpdate = sdk.Header.SessionInfoUpdate;
                    if (telemData.getTrackId() == 0)
                    {
                        telemData.setTrackId(Convert.ToInt32(YamlParser.Parse(sdk.GetSessionInfo(), "WeekendInfo:TrackID:")));
                        DiscoverCameras();
                    }
                    
                    if (newUpdate != lastUpdate)
                    {
                        // Session Info updated (e.g. perhaps a client has connected/disconnected)
                        lastUpdate = newUpdate;
                        // Update the current Driver list
                        string yaml = sdk.GetSessionInfo();
                        length = yaml.Length;
                        start = yaml.IndexOf("DriverInfo:\n", 0, length);
                        end = yaml.IndexOf("\n\n", start, length - start);
                        string DriverInfo = yaml.Substring(start, end - start);
                        ParseDrivers(DriverInfo);                   
                    }
                    UpdateDriverPositions(drivers);
                    foreach (var socket in allSockets.ToList())
                    {
                        Console.WriteLine("Broadcast sent...");
                        Console.WriteLine(telemData.toJson());
                        socket.Send(telemData.toJson());                        
                    }

                }
                else if (sdk.IsInitialized)
                {
                    drivers.Clear();
                    cameras.Clear();
                    telemData.setTrackId(0);
                    sdk.Shutdown();
                    lastUpdate = -1;
                }
                else
                {
                    drivers.Clear();
                    cameras.Clear();
                    telemData.setTrackId(0);
                    Console.WriteLine("NOT CONNECTED!");
                    sdk.Startup();
                }                
                System.Threading.Thread.Sleep(1000);               
            }
        }

        private static void ParseDrivers(string driverInfo)
        {
            length = driverInfo.Length;
            start = driverInfo.IndexOf("Drivers:\n") + 9;
            driverInfo = driverInfo.Substring(start, length-start);
            int carIdx;
            string userName;
            string carNumber;
            Driver driver;

            string[] driversYaml = driverInfo.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);
     
            foreach (string match in driversYaml)
            {               
                start = match.IndexOf("CarIdx: ") + 8;
                end = match.IndexOf("\n");
                carIdx = Convert.ToInt32(match.Substring(start, end - start));
                if(!drivers.ContainsKey(carIdx)) 
                {
                    // new driver
                    start = match.IndexOf("  UserName: ") + 12;
                    end = match.IndexOf("\n", start);
                    userName = match.Substring(start, end - start);

                    start = match.IndexOf("  CarNumber: ") + 13;
                    end = match.IndexOf("\n", start);
                    carNumber = match.Substring(start, end - start);

                    driver = new Driver();
                    driver.Index = carIdx;
                    driver.Name = userName;
                    driver.CarNum = carNumber;
                    drivers.Add(carIdx, driver);
                }
            }  
        }

        private static void UpdateDriverPositions(Dictionary<int, Driver> drivers)
        {
            float[] distPcts = (float[])sdk.GetData("CarIdxLapDistPct");
            bool[] onPitRoad = (bool[])sdk.GetData("CarIdxOnPitRoad");
            if (distPcts != null)
            {
                foreach (int carIdx in drivers.Keys)
                {
                    Driver driver = drivers[carIdx];
                    driver.LapPct = distPcts[driver.Index];
                    driver.OnPitRoad = onPitRoad[driver.Index];
                }
            }
        }

        
        /**
         * Very hacky/not robust regex fudge to 'discover' camera groups!  Only called once per track
         */
        private static void DiscoverCameras()
        {
            String yaml = sdk.GetSessionInfo();
            int length = yaml.Length;
            int start = yaml.IndexOf("CameraInfo:\n", 0, length);
            int end = yaml.IndexOf("\n\n", start, length - start);

            string CameraInfo = yaml.Substring(start, end - start);

            length = CameraInfo.Length;
            start = CameraInfo.IndexOf(" Groups:\n", 0, length);
            end = length;

            string Cameras = CameraInfo.Substring(start, end - start - 1);
            string[] cameraList = Cameras.Split(new string[] { "\n - " }, StringSplitOptions.RemoveEmptyEntries);
            int groupNum;
            foreach (string camera in cameraList)
            {
                Regex groupNumReg = new Regex("^GroupNum: ([0-9]+)");
                string[] groupNumResult = (from Match match in groupNumReg.Matches(camera) select match.Groups[1].Value).ToArray();
                if (groupNumResult.Length < 1)
                {
                    continue;
                }
                groupNum = Convert.ToInt16(groupNumResult[0]);
                Regex groupNameReg = new Regex("GroupName: ([\\w ]+)");
                string[] groupNameResult = (from Match match in groupNameReg.Matches(camera) select match.Groups[1].Value).ToArray();
                cameras.Add(groupNameResult[0], groupNum);
            }
        }
    }
}
