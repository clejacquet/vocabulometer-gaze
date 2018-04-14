using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocabulometerProvider
{
    public class FixationAnalyzer
    {
        public class Fixation
        {
            public double x;
            public double y;
            public double var_x;
            public double var_y;

            public Fixation(double x, double y, double var_x, double var_y)
            {
                this.x = x;
                this.y = y;
                this.var_x = var_x;
                this.var_y = var_y;
            }
        }

        private double mean_x = 0;
        private double mean_y = 0;

        private double var1 = 0;
        private double var2 = 0;

        private long clearBufferTime;

        private List<double> bufferX = new List<double>();
        private List<double> bufferY = new List<double>();

        // threshold for the sum of var1 and var2 to determine if var1+var2 > VARIANCE_THRESHOLD then we have a saccade
        private double VARIANCE_THRESHOLD = 50.0f;
        private long FIXATION_MIN_DURATION = 100; // minimum duration of a fixation in milliseconds

        // Detects eyes blinking
        // Returns :
        // 0 : no blinking
        // 1 : both eyes blinking
        // 2 : left eye blinking
        // 3 : right eye blinking
        private static int eyeBlink(double leftX, double leftY, double rightX, double rightY)
        {
            if ((leftX == 0.0f || leftY == 0.0f) || (rightX == 0.0f || rightY == 0.0f))
            {

                if ((leftX == 0.0f || leftY == 0.0f) && (rightX == 0.0f || rightY == 0.0f))
                {
                    return 1;
                }
                if (leftX == 0.0f || leftY == 0.0f)
                {
                    return 2;
                }
                return 3;
            }
            return 0;
        }

        public Fixation update(double leftX, double leftY, double rightX, double rightY)
        {
            var blinkValue = eyeBlink(leftX, leftY, rightX, rightY);

            // Filtering eyes blinking
            if (blinkValue != 1)
            { // no both eyes blinking (in this case, we do nothing)
                if (blinkValue != 0)
                {// no blinking

                    if (blinkValue == 2)
                    {// left eye
                        leftX = rightX;
                        leftY = rightY;
                    }
                    if (blinkValue == 3)
                    {// right eye
                        rightX = leftX;
                        rightY = leftY;
                    }
                }
            }
            return this.addNewCoordinates(leftX, leftY, rightX, rightY);
        }

        private Fixation addNewCoordinates(double leftX, double leftY, double rightX, double rightY)
        {
            Fixation result = null;
            var old_mean_x = this.mean_x;
            var old_mean_y = this.mean_y;

            var old_var1 = this.var1;
            var old_var2 = this.var2;

            this.updateArrays(leftX, leftY, rightX, rightY);
            this.updateCursor();

            if (this.var1 + this.var2 > this.VARIANCE_THRESHOLD)
            {
                long t = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                if (t - this.clearBufferTime > this.FIXATION_MIN_DURATION)
                {
                    result = new Fixation(old_mean_x, old_mean_y, old_var1, old_var2);
                }

                this.clearBufferTime = t;

                this.bufferX.Clear();
                this.bufferY.Clear();

                this.updateArrays(leftX, leftY, rightX, rightY);
            }

            return result;
        }

        // Averaging the last "CURSOR_MEAN_BUFFER_SIZE" values received
        private void updateCursor()
        {
            var bufferX2 = this.bufferX.Select(x => x * x);
            var bufferY2 = this.bufferY.Select(y => y * y);

            var Ex = this.bufferX.Average();
            var Ex2 = bufferX2.Average();
            var Ey = this.bufferY.Average();
            var Ey2 = bufferY2.Average();

            var COVxx = Ex2 - Ex * Ex;
            var COVyy = Ey2 - Ey * Ey;

            this.mean_x = Ex;
            this.mean_y = Ey;

            this.var1 = Math.Sqrt(COVxx);
            this.var2 = Math.Sqrt(COVyy);
        }

        // Keeping only the last "CURSOR_MEAN_BUFFER_SIZE" values
        private void updateArrays(double leftX, double leftY, double rightX, double rightY)
        {
            this.bufferX.Add((leftX + rightX) / 2.0f);
            this.bufferY.Add((leftY + rightY) / 2.0f);
        }
    }
}
