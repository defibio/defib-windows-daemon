using Defib.Service;
using System;
using System.Threading;
using Microsoft.Owin.Hosting;
using Defib.Entity;
using System.Collections.Generic;

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

            // Initialize API
            WebApp.Start<Api>("http://localhost:2700");

            // Start threads
            Thread generateBatchesThread = new Thread(Heart.GenerateBatches);
            generateBatchesThread.Start();
            Thread executeBatchesThread = new Thread(Heart.ExecuteBatches);
            executeBatchesThread.Start();

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
