using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using System;
using System.Collections.Generic;
using System.IO;

namespace VocabulometerProvider
{
    internal class MyDecisionTree
    {
        // A structure for the data for inputs and outputs for the decision tree
        public struct DataForm
        {
            public double[][] inp; // We will give a table of each features in inputs
            public int[] outp; // In outputs we will have a table of 1 or 0 (reading or not reading)

            public DataForm(double[][] i, int[] o)
            {
                inp = i;
                outp = o;
            }
        }

        public static DecisionTree tree;

        // Initialize the decision tree to use it later
        internal static void initialize()
        {
            // All the participants to train the decision tree
            string[] participants = new string[] { "equilibree/clem", "equilibree/clement", "equilibree/david", "equilibree/florian","equilibree/juliette",
                "equilibree/juliette-bis", "equilibree/lea", "equilibree/lea-bis", "equilibree/mathilde", "equilibree/samuel", "equilibree/william" };

            // My data (inputs and outputs for the tree) from the previous participants
            DataForm dataTraining = getDataFromParticipants(participants);

            // Inputs and outputs for the training
            double[][] inputs = dataTraining.inp;
            int[] outputs = dataTraining.outp;

            // We  use the C4.5 for learning:
            C45Learning teacher = new C45Learning();

            // Finally induce the tree from the data:
            tree = teacher.Learn(inputs, outputs);
        }
        
        // Make a prediction of reading or not for a fixation in the list took in parameter
        // Return the gaze with the predicted labbel of reading
        public static Gaze getPredictionGaze(List<Gaze> list)
        {
            // All the features needed to predict the reading 
            double meanAngle = SequenceOfGaze.getMeanAngleOrientedSequence(list);
            double medianAngle = SequenceOfGaze.getMedianAngle(list);
            double meanLength = SequenceOfGaze.getMeanLengthSaccade(list);
            double medianLength = SequenceOfGaze.getMedianLengthSaccade(list);
            double[] features = { meanAngle, medianAngle, meanLength, medianLength };
            double[][] inputs = { features };

            // Get the estimated class labels
            int[] predicted = tree.Decide(inputs);
            
            // We take the Gaze in the middle of the list
            int middle = (list.Count / 2) + 1;
            Gaze g = list[middle];

            // Labbelize the Gaze as reading or not according the response of the decision tree
            if (predicted[0]==0) { g.isReading = false; }
            else { g.isReading = true; }

            return g;
        }

        // Return the data in DataForm from each file of the participants took in participants
        private static DataForm getDataFromParticipants(string[] participants)
        {
            List<List<Gaze>> allData = new List<List<Gaze>>();

            // Make a list of all the data in the files
            for (int i = 0; i < participants.Length; i++)
            {
                string filePath = Path.GetFullPath("data/" + participants[i] + ".json");
                allData.Add((List<Gaze>)FileManager.getDataFromFile(@filePath));
            }

            List<double[]> inputsList = new List<double[]>();
            List<int> outputsList = new List<int>();
            
            // Sort all of the data to transform it into the inputs of our decision tree
            foreach (List<Gaze> listGazes in allData)
            {
                List<List<Gaze>> sequences = SequenceOfGaze.getSequencesLabbeled(listGazes);

                foreach (List<Gaze> seq in sequences)
                {
                    double meanAngle = SequenceOfGaze.getMeanAngleOrientedSequence(seq);
                    double medianAngle = SequenceOfGaze.getMedianAngle(seq);
                    double meanLength = SequenceOfGaze.getMeanLengthSaccade(seq);
                    double medianLength = SequenceOfGaze.getMedianLengthSaccade(seq);
                    double[] features = { meanAngle, medianAngle, meanLength, medianLength };
                    int isRead = Convert.ToInt32(seq[0].isReading);
                    inputsList.Add(features);
                    outputsList.Add(isRead);
                }
            }

            // The decision tree take table in parameter (not list)
            double[][] inputs = inputsList.ToArray();
            int[] outputs = outputsList.ToArray();
            
            // The final form of the data
            DataForm res = new DataForm(inputs, outputs);

            return res;
        }
        
        // Make a confusion matrix from the differences between the table took in parameter
        private static void compareTab(int[] oupt, int[] predicted)
        {
            double size = oupt.Length;
            double nbRight = 0;
            double nbRightR = 0; //number of right labbelisation as reading
            double nbFalseR = 0; //number of false labbelisation as reading
            double nbRightNR = 0; //number of right labbelisation as no reading
            double nbFalseNR = 0; //number of false labbelisation as no reading
            if (size == predicted.Length)
            {
                for (int i = 0; i < size; i++)
                {
                    if (oupt[i] == 1)
                    {
                        if (predicted[i] == 1) { nbRight++; nbRightR++; }
                        else { nbFalseR++; } // Console.WriteLine("Erreur R : "+i+"\tpredicted : "+predicted[i]); }
                    }
                    else
                    {
                        if (predicted[i] == 1) { nbFalseNR++; } // Console.WriteLine("Erreur NR : " + i + "\tpredicted : " + predicted[i]); }
                        else { nbRight++; nbRightNR++; }
                    }
                }
            }
            double accuracy = nbRight / size;
            double nbReading = (nbRightR + nbFalseR);
            double nbNoReading = (nbRightNR + nbFalseNR);
            Console.WriteLine("-----------Labbeled as reading---------------");
            Console.WriteLine("Total labbelised : " + nbReading);
            Console.WriteLine("Right labbeled : " + (nbRightR / nbReading));
            Console.WriteLine("False labbeled : " + (nbFalseR / nbReading));
            Console.WriteLine("----------Labbeled as no reading-------------");
            Console.WriteLine("Total labbelised : " + nbNoReading);
            Console.WriteLine("Right labbeled : " + (nbRightNR / nbNoReading));
            Console.WriteLine("False labbeled : " + (nbFalseNR / nbNoReading));
            Console.WriteLine("------------------TOTAL---------------------");
            Console.WriteLine("Accuracy : " + accuracy);
        }

        // Print the table took in parameter
        public static void printTab<T>(T[] tab)
        {
            foreach (T nb in tab)
            {
                Console.WriteLine(nb);
            }
        }

        // Print the 2D table took in parameter
        public static void printTab2D<T>(T[][] inputs)
        {
            foreach (T[] tab in inputs)
            {
                printTab(tab);
                Console.WriteLine("");
            }
        }
        
    }
}