using System;
using System.Collections.Generic;
using System.Linq;

namespace VocabulometerProvider
{
    public class Gaze
    {
        public float gazeX; // px
        public float gazeY; // px
        public float timestamp; // ms
        public int idLine;
        public float duration;
        public float squaredVelocity;
        public bool isReading; // true if the gaze is record while reading a text
        public int idFixation; // the id of the fixation

        public void printGaze() {
            Console.WriteLine("X=" + gazeX + "\tY=" + gazeY + "\tisReading : "+isReading+"\tid : "+idFixation); }

        public static void printListGaze(List<Gaze> l){
            foreach (Gaze g in l) { g.printGaze(); }  }

        public static Gaze copyGaze (Gaze g)
        {
            Gaze gaze = new Gaze()
            {
                gazeX = g.gazeX,
                gazeY = g.gazeY,
                timestamp = g.timestamp,
                idLine = g.idLine,
                duration = g.duration,
                squaredVelocity = g.squaredVelocity,
                isReading = g.isReading,
                idFixation = g.idFixation
            };
            return gaze;
        }

        public static List<Gaze> copyListGaze(List<Gaze> l)
        {
            List<Gaze> copyL = new List<Gaze>();
            foreach (Gaze g in l)
            {
                copyL.Add(copyGaze(g));
            }
            return copyL;
        }

        
        public static List<Gaze> fixationBusher2008(List<Gaze> gazes, int msFixation = 100, int smallSquareSize = 30, int bigSquareSize = 50, int consecutiveFails = 4)
        {
            //First compute how many fixations we need in order to have 100ms (which is the minimum time for a fixation)

            List<Gaze> fixations = new List<Gaze>();
            for (int i = 0; i < gazes.Count; i++)
            {
                int fails = 0;
                //Let's take enought gazes for making 100ms
                float time = 0;
                List<Gaze> fixationCandidates = new List<Gaze>();
                int counter = 0;
                while (time < msFixation && i + counter < gazes.Count)
                {
                    fixationCandidates.Add(gazes[i + counter]);
                    time = fixationCandidates[fixationCandidates.Count - 1].timestamp - fixationCandidates[0].timestamp; //the timestamp of the last - the timestamp of the first
                    counter++;
                }

                //If not enough gazes are gathered (at the end of the file)
                if (time < msFixation)
                    break;

                //If they are contained in a square
                if (insideSquare(fixationCandidates, smallSquareSize))
                {
                    //This gazes are considered as a fixation
                    //While next gazes are include in 50 * 50 square we add them to the fixation
                    int j = i + fixationCandidates.Count;
                    while (j < gazes.Count)
                    {
                        fixationCandidates.Add(gazes[j]);
                        //If the gaze is not in  the square, we remove it from the fixation and stop the process after 4 consecutive fails
                        if (!insideSquare(fixationCandidates, bigSquareSize))
                        {
                            fixationCandidates.RemoveAt(fixationCandidates.Count - 1);
                            fails++;
                            if (fails >= consecutiveFails)
                                break;
                        }
                        else
                        {
                            fails = 0;
                        }
                        j++;
                    }
                    fixations.Add(centroid(fixationCandidates));
                    i += fixationCandidates.Count - 1;
                }
            }
            return fixations;
        }

        public static bool insideSquare(List<Gaze> gazes, int sizeSquare)
        {
            var minX = gazes.Min(p => p.gazeX);
            var minY = gazes.Min(p => p.gazeY);
            var maxX = gazes.Max(p => p.gazeX);
            var maxY = gazes.Max(p => p.gazeY);

            if (maxX - minX < sizeSquare && maxY - minY < sizeSquare)
                return true;
            else
                return false;
        }

        public static Gaze centroid(List<Gaze> gazes)
        {
            Gaze g = gazes.First();
            g.duration = gazes.Last().timestamp - g.timestamp;
            for (int i = 1; i < gazes.Count; i++)
            {
                g.gazeX += gazes[i].gazeX;
                g.gazeY += gazes[i].gazeY;
            }
            g.gazeX /= gazes.Count;
            g.gazeY /= gazes.Count;
            return g;
        }
    }
}
