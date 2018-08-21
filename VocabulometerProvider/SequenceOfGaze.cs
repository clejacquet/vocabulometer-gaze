using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VocabulometerProvider
{
    // Here are all function about a sequence of gaze and its analyze
    public class SequenceOfGaze
    {
        public static float maxAngleReading = 20 * (float) Math.PI / 180; 
        public static float maxLengthReading = 110;
        static List<Gaze> oldSequence = new List<Gaze>(); // list to analyze : read or not ? 
        public static int lengthOfTheContext = 2; // we will analyze a sequence with a context of the 2 previous and the 2 next fixations
        static int lengthSequenceToAnalyze = lengthOfTheContext*2 +1; // the total length of our sequence which will be analazed 
        
        public static List<List<Gaze>> getSequences()
        {
            List<List<Gaze>> mySequences = new List<List<Gaze>>();
            List<Gaze> listFixa = Program.listFixations;
            bool notEmpty = listFixa.Any();
            if (notEmpty) { //check if the list is not empty
                if (oldSequence.Any()) {
                    oldSequence = addNewValues(oldSequence, listFixa);  //add the new fixations to the list to analyze
                } else {
                    oldSequence = Program.listFixations; //if the list doesn't exist add all the fixations
                }
                    
                int lengthOldSequence = oldSequence.Count();

                while (lengthOldSequence >= lengthSequenceToAnalyze)
                { //if we have enougth fixations to do a sequence
                    List<Gaze> sequence = new List<Gaze>();
                    for (int i = 0; i < lengthSequenceToAnalyze; i++) { sequence.Add(oldSequence[i]); } //a sequence with X fixations
                    oldSequence.RemoveAt(0);  // we remove 1 element to deplace the window
                    lengthOldSequence = oldSequence.Count(); //update the length of the oldSequence
                    mySequences.Add(sequence);
                }
            }

            return mySequences;
        }

        //Return a list of sequences of gaze to analyze from a list took in parameter
        public static List<List<Gaze>> getSequencesFromList(List<Gaze> l)
        {
            var mySequences = new List<List<Gaze>>();
            
            for(int i = 0; i<l.Count; i += lengthSequenceToAnalyze)
            {
                if(lengthSequenceToAnalyze <= (l.Count - i))
                {
                    mySequences.Add(l.GetRange(i, lengthSequenceToAnalyze));
                }
            }

            return mySequences;
        }

        //Return a list of sequence of gaze. Each Sequence is labbeled True OR False. 
        //The sequence length is lengthSequenceToAnalyze. 
        //A sequence is composed of consecutive Gaze with the SAME LABBEL.
        //The Gaze which don't correspond to this aren't added to the sequence. 
        public static List<List<Gaze>> getSequencesLabbeled(List<Gaze> l)
        {
            List<List<Gaze>> sequences = new List<List<Gaze>>();
            int i = 0;

            while (i < l.Count)
            {
                int idSeq = i;
                int indicator = i + lengthSequenceToAnalyze;
                bool sameLabbel = true;
                bool firstLabbel = l[i].isReading;

                while ( (i < indicator) && (sameLabbel) && (i < l.Count) )
                {
                    sameLabbel = (firstLabbel == l[i].isReading);
                    i++;
                }
                
                if (!sameLabbel) { i--; } 
                else {
                    int j = i-lengthSequenceToAnalyze;
                    if (i < l.Count) { sequences.Add(l.GetRange(j, lengthSequenceToAnalyze)); }
                }
            }
            
            return sequences;
        }
        
        public static List<Gaze> getPredictedList(List<Gaze> listFromFile)
        {
            List<Gaze> predictedList = new List<Gaze>();
            List<Gaze> sequence = new List<Gaze>();
            int counter = 0; int idSeq = 0;
            foreach (Gaze g in listFromFile)
            {
                predictedList.Add(g); // 0 to listOfAllGaze.Count()
                sequence.Add(g); // 0 to lengthSequenceToAnalyze

                if (counter < (lengthSequenceToAnalyze-1)) {
                    counter++;
                } else {
                    while (sequence.Count() >= lengthSequenceToAnalyze)
                    {
                        bool predictionOfSeq = analyzeSequence(sequence);
                        int idToLabbel = lengthOfTheContext + idSeq;
                        predictedList[idToLabbel].isReading = predictionOfSeq;
                        sequence.RemoveAt(0);

                        idSeq ++;
                    }
                    counter = sequence.Count();
                }
            }

            return predictedList;
        }
 
        //Update the old sequence with the new
        public static List<Gaze> addNewValues(List<Gaze> oldSeq, List<Gaze> list)
        {
            foreach (Gaze g in list)
            {
                bool gazeAlreadyAdded = false;
                int i = 0;
                while (i < oldSeq.Count() && !gazeAlreadyAdded)
                {
                    if (g.idFixation == oldSeq[i].idFixation) { gazeAlreadyAdded = true; }
                    i++;
                }
                if (!gazeAlreadyAdded) { oldSeq = addNewGazeToList(oldSeq, g); }
            }
            return oldSeq;
        }

        //Add a gaze to a list in the right order of its id fixation
        public static List<Gaze> addNewGazeToList(List<Gaze> list, Gaze gToAdd){
            int i = 0;
            Gaze g = list[i];
            int max = list.Count();
            while ( (g.idFixation < gToAdd.idFixation) && (i < list.Count())  ){
                g = list[i]; i++; 
            }
            list.Insert(i,gToAdd);
            return list;
        }
        
        //Analyze if the sequence took in parameter is a reading sequence or not
        //Return true if it is a reading sequence and false in other cases
        public static bool analyzeSequence(List<Gaze> sequence)
        {
            bool reading = false;

            //TEST WITH THE MEAN OF ANGLE
            bool meanAngleSucceed = testMeanAngle(sequence);
            //TEST WITH THE LENGTH OF THE SACCADES
            bool meanLengthSucceed = testMeanLength(sequence);
            
            if (meanAngleSucceed && meanLengthSucceed) { reading = true; }

            return reading;
        }

        private static bool testMeanAngle(List<Gaze> sequence)
        {
            bool res = false;
            int lengthSeq = sequence.Count();
            int i = 0;
            if (lengthSeq > 3) //we need at least 4 points
            {
                while ((i < lengthSeq) && !res)
                {
                    List<Gaze> seq2 = new List<Gaze>(sequence);
                    seq2.RemoveAt(i); //test to find any outlier

                    if (!res)
                    {
                        float meanAngleSeq = getMeanAngleOrientedSequence(seq2);
                        res = (meanAngleSeq < maxAngleReading);
                    }

                    i++;
                }
            }
            return res;
        }

        private static bool testMeanLength(List<Gaze> seq)
        {
            float mean = getMeanLengthSaccade(seq);
            bool res = (mean < maxLengthReading);
            return res;
        }

        //Calculate the mean of the length of the saccades in a list of fixations
        public static float getMeanLengthSaccade(List<Gaze> seq)
        {
            int nbSaccades = seq.Count() - 1;
            float mean = 0;
            if (nbSaccades > 0)
            {
                float maxLength = 0;
                for (int i = 0; i < nbSaccades; i++)
                {
                    float lengthSaccade = Saccade.distance(seq[i], seq[i + 1]);
                    mean += lengthSaccade;
                    if (maxLength < lengthSaccade) { maxLength = lengthSaccade; }
                }

                mean -= maxLength; // we remove a potential outlier
                mean /= nbSaccades - 1;
            }
            return mean;
        }
        
        //Calculate the mean of angle in a list of Gaze
        public static float getMeanAngleOrientedSequence(List<Gaze> seq)
        {
            int max = seq.Count() - 2;
            float mean = 0;
            for (int i = 0; i < max; i++) {
                float angle = (Saccade.angleOriented(seq[i], seq[i + 1], seq[i + 2])) ;

                angle *= (float)Math.PI / 180;

                float res = angle % (float)Math.PI;

                float demiPi = (float)Math.PI / 2;
                if ((demiPi < res) && (res < Math.PI)) { res -= (float)Math.PI; }

                res = Math.Abs(res);

                //Console.WriteLine(seq[i].idFixation + "-" + seq[i + 1].idFixation + "-" + seq[i + 2].idFixation + " : " + (angle * 180 / ((float)Math.PI) ) + " / " + angle + "\tres: " + res + "\tmod: " + (angle % (float)Math.PI)); 

                mean += res;
            }
            mean /= max;

            return mean;
        }
        

        public static float getMedianAngle(List<Gaze> list)
        {
            int size = list.Count;
            List<float> listAngle = new List<float>(); 
            int max = size - 2;
            for (int i = 0; i < max; i++)
            {
                float angle = (Saccade.angleOriented(list[i], list[i + 1], list[i + 2]));
                angle *= (float)Math.PI / 180;
                float res = angle % (float)Math.PI;
                float demiPi = (float)Math.PI / 2;
                if ((demiPi < res) && (res < Math.PI)) { res -= (float)Math.PI; }
                res = Math.Abs(res);
                listAngle.Add(res);
            }

            listAngle.Sort();
            int middle = (listAngle.Count) / 2;

            return listAngle[middle];
        }

        public static float getMedianLengthSaccade(List<Gaze> list)
        {
            int nbSaccades = list.Count() - 1;
            List<float> listSaccades = new List<float>();
            if (nbSaccades > 0)
            {
                for (int i = 0; i < nbSaccades; i++)
                {
                    float lengthSaccade = Saccade.distance(list[i], list[i + 1]);
                    listSaccades.Add(lengthSaccade);
                }
            }
            int middle = listSaccades.Count / 2;
            return listSaccades[middle];
        }
    }
}
