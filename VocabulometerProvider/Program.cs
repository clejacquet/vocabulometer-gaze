using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace VocabulometerProvider
{
    class Program
    {
        public static GazePointData lastGazePoint;
        public static Mutex locker = new Mutex();
        public static List<Gaze> listFixations = new List<Gaze>();
        public static List<Gaze> listGazes = new List<Gaze>();
        public static List<Gaze> listFixationsToSave = new List<Gaze>();
        public static bool saveData = false; // save fixation in json file or not
        public static bool reading = false; // if the fixation was a reading fixation or not
        public static int nbFixations = 0;
        private static short seqLength = 1000; // ms

        static void Main(string[] args)
        {
            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();
            
            Boolean debSeq = true;
            double debSeqTime = Int16.MaxValue;
             
            MyDecisionTree.initialize(); // initialize the decision tree
            
            gazePointDataStream.GazePoint((gazePointX, gazePointY, timestamp) => {
                Program.lastGazePoint = new GazePointData(gazePointX, gazePointY, timestamp, timestamp);
                //Console.WriteLine(gazePointX + "-" + gazePointY);

                if (debSeq) //if it's the beginning of a new sequence
                {
                    debSeqTime = timestamp; //set the time of the beginning of the sequence
                    debSeq = false; 
                }
                if (timestamp - debSeqTime > seqLength) //if it's the end of the sequence
                {
                    locker.WaitOne();
                    debSeq = true;
                    listFixations = Gaze.fixationBusher2008(listGazes); //get the fixations from the list of gazes

                    foreach (Gaze g in listFixations) {
                        nbFixations++;
                        g.idFixation = nbFixations; //set a id for each fixations
                    }

                    if (saveData) { //if we must save the data
                        foreach (Gaze g in listFixations) {
                            g.isReading = reading;
                            listFixationsToSave.Add(g); //then add to the list which will be saved
                        }
                    }

                    ChatHub.sendFixation = true; //send the fixations to the web client
                    listGazes.Clear(); //reset de list of gazes
                    locker.ReleaseMutex();
                }

                Gaze gaze = new Gaze()  {
                    gazeX = (float) gazePointX,
                    gazeY = (float) gazePointY,
                    timestamp = (float) timestamp
                };
                listGazes.Add(gaze);

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

            Console.ReadLine();
        }
    }
}
