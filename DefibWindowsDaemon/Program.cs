using Defib.Service;
using System;
using System.Threading;

namespace Defib
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize statics
            Database.Initialize();
            Context.Initialize();
            ScriptDelegator.Initialize();
            Heart.Initialize();

            // Load required data from database
            Database.LoadHeartbeats();
            Database.LoadUsers();

            if (Context.Heartbeats.Count > 0)
            {
                Heart.Recalculate = true;
            }

            // Start threads
            Thread generateBatchesThread = new Thread(Heart.GenerateBatches);
            Thread executeBatchesThread = new Thread(Heart.ExecuteBatches);

            bool shouldRun = true;

            while (shouldRun)
            {
                string input = Console.ReadLine();

                if (input == "exit")
                {
                    shouldRun = false;
                }
            }
        }
    }
}
