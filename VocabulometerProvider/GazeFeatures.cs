using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VocabulometerProvider
{
    public class GazeFeatures
    {
        public static string nameOfTheParticipant;

        public static List<Gaze> listFixations;
        public static List<Saccade> listSaccades;
        
        public static string[] participants = new string[] { "clement", "david", "florian",
                "juliette", "juliette-bis", "lea", "lea-bis", "mathilde",
                "pierre", "william", "samuel", "ag"};

        float fixationDurationMean; //=>difficulty ?
        float fixationsNumberVariance;
        int fixationsNumber;

        float saccadeLengthMedian; //outliers would not affect with median
        float saccadeLenghtVariance;
        float saccadeVelocityXMean;
        float saccadeVelocityYMean;

        List<float> angleSaccade; //amplitude

        public static void describeAll()
        {
            //string[] participant = new string[] { "lea" };

            for (int i = 0; i < participants.Length; i++)
            {
                Console.WriteLine("\n-------------------"+participants[i]+"-----------------");
                string filePath = Path.GetFullPath("data/" + participants[i] + ".json");
                getInfos(@filePath);
            }

            Console.WriteLine("\n\n");
        }

        public static void getConfusionMatrix()
        {
            List<Gaze> fixaR = new List<Gaze>(); //list of fixations when reading
            List<Gaze> fixaNR = new List<Gaze>(); //list of fixations when not reading

            int nbRightR_TOT = 0; int nbRightNR_TOT = 0;
            int nbFalseR_TOT = 0; int nbFalseNR_TOT = 0;
            int nbFixaR_TOT = 0; int nbFixaNR_TOT = 0;
            
            string[] participant = new string[] { "clement","david","florian","juliette","juliette-bis","lea","lea-bis","mathilde","pierre","samuel","william" };

            for (int i = 0; i < participant.Length; i++)
            {
                Console.WriteLine("\n\n-----------EXPERIENCE n° " + participant[i] + "---------------");
                string filePath = Path.GetFullPath("data/" + participant[i] + ".json");
                List<Gaze> listFixaFromFile = (List<Gaze>)FileManager.getDataFromFile(@filePath);

                List<Gaze> predictedList = Gaze.copyListGaze(listFixaFromFile);
                predictedList = SequenceOfGaze.getPredictedList(predictedList);

               // Console.WriteLine("___________________INITIAL :____________________");
               // Gaze.printListGaze(listFixaFromFile);

                int nbRightR = 0; int nbRightNR = 0;
                int nbFalseR = 0; int nbFalseNR = 0;
                int nbFixaR = 0; int nbFixaNR = 0;
                int beginning = SequenceOfGaze.lengthOfTheContext;
                int end = predictedList.Count() - beginning;


                for (int j = beginning; j < end; j++)
                {
                    Gaze g = listFixaFromFile[j];
                    Gaze gP = predictedList[j];

                    if (g.idFixation == gP.idFixation)
                    {
                        if (g.isReading)
                        {
                            nbFixaR++;
                            if (gP.isReading) { nbRightR++; }
                            else { nbFalseR++; 
                                //Console.WriteLine("Erreur : "+gP.idFixation); 
                                //gP.printGaze(); 
                            }
                        }  else {
                            nbFixaNR++;
                            if (!gP.isReading) { nbRightNR++; }
                            else
                            {
                                nbFalseNR++; 
                                //Console.WriteLine("Erreur : " + gP.idFixation); 
                                //gP.printGaze(); 
                            }
                        }
                    }
                }

               // Console.WriteLine("_______________PREDICTION: ___________________");
               // Gaze.printListGaze(predictedList);

                printMatrixResults(nbFixaNR, nbFixaR, nbRightR, nbFalseR, nbRightNR, nbFalseNR);

                nbFixaR_TOT += nbFixaR;
                nbRightR_TOT += nbRightR;
                nbFalseR_TOT += nbFalseR;
                nbFixaNR_TOT += nbFixaNR;
                nbRightNR_TOT += nbRightNR;
                nbFalseNR_TOT += nbFalseNR; 
            }

            //Console.WriteLine("Moyenne à ne pas dépasser : " + SequenceOfGaze.maxAngleReading);

            Console.WriteLine("\n----------------------TOTAL----------------------------");
            printMatrixResults(nbFixaNR_TOT, nbFixaR_TOT, nbRightR_TOT, nbFalseR_TOT, nbRightNR_TOT, nbFalseNR_TOT);
            
            Console.WriteLine("\n\n");
        }

        public static void printMatrixResults(int nbFixaNR, int nbFixaR, int nbRightR, int nbFalseR, int nbRightNR, int nbFalseNR)
        {
            Console.WriteLine("\nTotal fixations: " + (nbFixaNR + nbFixaR));
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Nb fixations reading : " + nbFixaR);
            Console.WriteLine("Nb fixations right : " + nbRightR);
            Console.WriteLine("Nb fixations false : " + nbFalseR);
            Console.WriteLine("Accuracy : " + ((nbRightR * 100) / nbFixaR) + "%");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Nb fixations not read : " + nbFixaNR);
            Console.WriteLine("Nb fixations right : " + nbRightNR);
            Console.WriteLine("Nb fixations false : " + nbFalseNR);
            Console.WriteLine("Accuracy : " + ((nbRightNR * 100) / nbFixaNR) + "%");
        }



        //Just for one recording / one file
        public static void getInfos(string pathOfTheData)
        {
            List<Gaze> listFixa = (List<Gaze>)FileManager.getDataFromFile(pathOfTheData);
            int listSize = listFixa.Count;

            Gaze firstGaze = listFixa[0];
            Gaze lastGaze = listFixa[listSize - 1];
            float totalDuration = (lastGaze.timestamp - firstGaze.timestamp) / 60000;

            float meanDurationR = 0; //reading
            float meanDurationNR = 0; //no reading

            float maxDurationR = 0; float maxDurationNR = 0;
            float minDurationR = 100000000; float minDurationNR = 100000000;
            float meanAngleR = 0; float meanAngleNR = 0;
            float minAngleR = 0; float minAngleNR = 0;
            float maxAngleR = 0; float maxAngleNR = 0;
            float meanSizeSaccadeR = 0; float meanSizeSaccadeNR = 0;
            int nbR = 0; int nbNR = 0;

            for (int i = 0; i < listFixa.Count; i++)
            {
                Gaze g = listFixa[i];

                if (g.isReading)
                {
                    if (g.duration > maxDurationR) { maxDurationR = g.duration; }
                    if (g.duration < minDurationR) { minDurationR = g.duration; }
                    meanDurationR += g.duration;
                    if ((i > 0) && (i < (listSize - 1)))
                    {
                        float angle = Saccade.angleOriented(listFixa[i - 1], g, listFixa[i + 1]);

                        /*Console.WriteLine("__________________________________________"); listFixa[i - 1].printGaze();
                        Console.WriteLine("__________________________________________"); g.printGaze();
                        Console.WriteLine("__________________________________________"); listFixa[i + 1].printGaze();*/

                        if (angle > maxAngleR) { maxAngleR = angle; }
                        if (angle < minAngleR) { minAngleR = angle; }

                        meanSizeSaccadeR += Saccade.distance(listFixa[i - 1], g);
                        meanAngleR += angle;
                    }
                    nbR++;
                }
                else
                {
                    if (g.duration > maxDurationNR) { maxDurationNR = g.duration; }
                    if (g.duration < minDurationNR) { minDurationNR = g.duration; }
                    meanDurationNR += g.duration;
                    if ((i > 0) && (i < (listSize - 1)))
                    {
                        float angle = Saccade.angleOriented(listFixa[i - 1], g, listFixa[i + 1]);

                        if (angle > maxAngleNR) { maxAngleNR = angle; }
                        if (angle < minAngleNR) { minAngleNR = angle; }

                        meanSizeSaccadeNR += Saccade.distance(listFixa[i - 1], g);
                        meanAngleNR += angle;
                    }
                    nbNR++;
                }

            }

            meanDurationR /= nbR;
            meanDurationNR /= nbNR;
            meanAngleR /= nbR - 2;
            meanAngleNR /= nbNR - 2;
            meanSizeSaccadeR /= nbR - 1;
            meanSizeSaccadeNR /= nbNR - 1;

            Console.WriteLine("Duration of the record (min): \t" + totalDuration);
            Console.WriteLine("Number of fixations : \t\t" + listSize);
            //Console.WriteLine("Mini duration : " + minDuration);
            //Console.WriteLine("Max duration  : " + maxDuration);

            Console.WriteLine("\nREADING PART :");
            Console.WriteLine("\tNumber of fixation : \t\t" + nbR);
            Console.WriteLine("\tFixation duration mean (ms): \t" + meanDurationR);
            Console.WriteLine("\tmin : \t\t\t\t" + minDurationR + "\n\tmax : \t\t\t\t" + maxDurationR);
            Console.WriteLine("\tAngle mean : \t\t\t" + meanAngleR);
            Console.WriteLine("\tmin : \t\t\t\t" + minAngleR + "\n\tmax : \t\t\t\t" + maxAngleR);
            Console.WriteLine("\tMean size saccade  : \t\t" + meanSizeSaccadeR);

            Console.WriteLine("\nNOT READING PART :");
            Console.WriteLine("\tNumber of fixation : \t\t" + nbNR);
            Console.WriteLine("\tFixation duration mean (ms): \t" + meanDurationNR);
            Console.WriteLine("\tmin : \t\t\t\t" + minDurationNR + "\n\tmax : \t\t\t\t" + maxDurationNR);
            Console.WriteLine("\tAngle mean : \t\t\t" + meanAngleNR);
            Console.WriteLine("\tmin : \t\t\t\t" + minAngleNR + "\n\tmax : \t\t\t\t" + maxAngleNR);
            Console.WriteLine("\tMean size saccade  : \t\t" + meanSizeSaccadeNR);
        }
    }
}
