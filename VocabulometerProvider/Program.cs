using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace VocabulometerProvider
{
    class Program
    {
        public static GazePointData lastGazePoint;
        public static IList<Tuple<double, FixationAnalyzer.Fixation>> fixations = new List<Tuple<double, FixationAnalyzer.Fixation>>();
        public static Mutex locker = new Mutex();

        static void Main(string[] args)
        {
            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();
            FixationAnalyzer analyzer = new FixationAnalyzer();

            gazePointDataStream.GazePoint((gazePointX, gazePointY, timestamp) => {
                Program.lastGazePoint = new GazePointData(gazePointX, gazePointY, timestamp, timestamp);

                FixationAnalyzer.Fixation fixation = analyzer.update(gazePointX, gazePointY, gazePointX, gazePointY);
                if (fixation != null)
                {
                    locker.WaitOne();
                    fixations.Add(new Tuple<double, FixationAnalyzer.Fixation>(timestamp, fixation));
                    locker.ReleaseMutex();

                    Console.WriteLine("New fixation: ({0}, {1})", fixation.x, fixation.y);
                }
            });

            var deviceObserver = host.States.CreateEyeTrackingDeviceStatusObserver();
            
            deviceObserver.WhenChanged(state =>
            {
                if (state.IsValid)
                {
                    switch (state.Value)
                    {
                        case EyeTrackingDeviceStatus.Tracking:
                            Console.WriteLine("Now Tracking");
                            break;

                        case EyeTrackingDeviceStatus.DeviceNotConnected:
                            Console.WriteLine("Device is not connected");
                            break;

                        case EyeTrackingDeviceStatus.Initializing:
                            Console.WriteLine("Initializing Tracking...");
                            break;

                        case EyeTrackingDeviceStatus.InvalidConfiguration:
                            Console.WriteLine("Invalid Configuration reported");
                            break;

                        case EyeTrackingDeviceStatus.Configuring:
                            Console.WriteLine("Tracking configuring...");
                            break;

                        default:
                            Console.WriteLine("Tracking interrupted");
                            break;
                    }
                } else
                {
                    Console.WriteLine("Invalid state!");
                }
                
            });

            string url = @"http://localhost:8080/";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine(string.Format("Server running at {0}", url));
                Console.ReadLine();

                deviceObserver.Dispose();
                host.Dispose();
            }
        }
    }
}
