using Microsoft.AspNet.SignalR;
using System;
using System.Windows.Forms;

namespace VocabulometerProvider
{
    public class ChatHub : Hub
    {
        public void UpdateRequest()
        {
            if (Program.lastGazePoint != null)
            {
                Clients.Caller.onGazePoint(Program.lastGazePoint.X, Program.lastGazePoint.Y);
            }

            Program.locker.WaitOne();
            foreach (Tuple<double, FixationAnalyzer.Fixation> t in Program.fixations)
            {
                Clients.Caller.onFixation(t.Item2.x, t.Item2.y);
            }
            Program.fixations.Clear();
            Program.locker.ReleaseMutex();
        }
    }
}
