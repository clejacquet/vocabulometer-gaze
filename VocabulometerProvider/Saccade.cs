using System;

namespace VocabulometerProvider
{
    public class Saccade
    {
        float size;
        Gaze begin;
        Gaze end;

        public Saccade(Gaze g1, Gaze g2)
        {
            begin = g1;
            end = g2;
            size = distance(g1, g2);
        }
        
        public void printSaccade()
        {
            Console.WriteLine("-------------SACCADE-------------");
            Console.WriteLine("Begin fixation : ");
            begin.printGaze();
            Console.WriteLine("End fixation : ");
            end.printGaze();
            Console.WriteLine("Size : " + size);
            Console.WriteLine("----------------------------------");
        }

        // Return the length of the saccade between the two points took in parameter
        public static float distance(Gaze g1, Gaze g2) {
            return (float)(Math.Sqrt(Math.Pow((g2.gazeX - g1.gazeX), 2) + Math.Pow((g2.gazeY - g1.gazeY), 2)));
        }
        
        // Return the angle between the three points took in parameter ( 0 to 360 degree)
        public static float angleOriented(Gaze g1, Gaze g2, Gaze g3)
        {
            float d1 = Saccade.distance(g1, g2);
            float d2 = Saccade.distance(g2, g3);
            float d3 = Saccade.distance(g1, g3);
            float res = ((d1 * d1 + d2 * d2 - d3 * d3) / (2 * d1 * d2));
            float angle = (float)(Math.Acos(res));
            if (res < (-1)) { angle = (float)(Math.Acos(-1)); }
            if (res > 1) { angle = (float)(Math.Acos(1)); }
            angle *= (float)(180.0 / Math.PI); // to convert the angle in degre

            float a = (g2.gazeY - g1.gazeY) / (g2.gazeX - g1.gazeX);
            float b = g1.gazeY - a*g1.gazeX;
            float yD = a * g3.gazeX + b;
            bool aboveLine = g3.gazeY < yD;
            if (aboveLine) { angle = 360 - angle; }

            return angle;
        }
        
    }
}
