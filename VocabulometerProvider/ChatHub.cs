using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;

namespace VocabulometerProvider
{
    public class ChatHub : Hub
    {
        public static Boolean sendFixation = false;

        public void UpdateRequest()
        {
            if (Program.lastGazePoint != null)
            {
                Clients.Caller.onGazePoint(Program.lastGazePoint.X, Program.lastGazePoint.Y);
            }

            if (sendFixation)
            {
                Program.locker.WaitOne();

                // Get the list of all previous sequences (list of fixations)
                List<List<Gaze>> sequences = SequenceOfGaze.getSequences();
                foreach (List<Gaze> seq in sequences)
                {
                    // Get a gaze labbelised with the decision tree
                    Gaze g = MyDecisionTree.getPredictionGaze(seq);

                    // Send to the client the gaze
                    // Clients.Caller.onFixation(g.gazeX, g.gazeY, g.idFixation, g.isReading); 
                    
                    Clients.Caller.onFixation(g.gazeX, g.gazeY, g.isReading);
                }

                sendFixation = false;
                Program.locker.ReleaseMutex();
            }
        }
        
        // The client call this function to record the gaze in a file
        public void RecordGazeJson(bool recordOn)
        {
            Console.WriteLine("RECORD IN JSON : "+recordOn);
            Program.saveData = recordOn;
            if (!recordOn)
            {
                var dataToSave = Program.listFixationsToSave;
                FileManager.saveDataInFile(dataToSave);
            } 
        }

    }

}
