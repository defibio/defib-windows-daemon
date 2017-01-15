using Defib.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Defib.Service
{
    public static class Heart
    {
        public static Dictionary<int, Batch> Batches;
        public static Dictionary<int, Heartbeat> Processed;
        public static Dictionary<int, bool> Executing;
        public static bool Recalculate;

        public static void Initialize()
        {
            Batches = new Dictionary<int, Batch>();
            Processed = new Dictionary<int, Heartbeat>();
            Executing = new Dictionary<int, bool>();
        }

        public static void SendHeartbeat(string key)
        {
            WebRequest httpRequest = WebRequest.Create("https://defib.io/heartbeat/receiver/" + key);
            WebResponse httpResponse = httpRequest.GetResponse();

            Stream httpStream = httpResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(httpStream);
            string responseFromDefib = streamReader.ReadToEnd();

            streamReader.Close();
            httpStream.Close();
            httpResponse.Close();
        }

        // THREAD
        public static void GenerateBatches()
        {
            while (true)
            {
                // Recalculate all heartbeats
                if (Recalculate)
                {
                    Recalculate = false;
                    Batches.Clear();
                    Processed.Clear();

                    int currentTimestamp = Utils.GetCurrentTimestamp();

                    foreach (System.Collections.Generic.KeyValuePair<int, Heartbeat> pairs in Context.Heartbeats)
                    {
                        if (Processed.ContainsKey(pairs.Value.Id))
                        {
                            continue;
                        }

                        Heartbeat heartbeat = pairs.Value;

                        // Possible downtime?
                        if (heartbeat.NextBeat != -1 && currentTimestamp > heartbeat.NextBeat)
                        {
                            if (Batches.ContainsKey(currentTimestamp))
                            {
                                Batches[currentTimestamp].Entries.Add(heartbeat.Id, heartbeat);
                            }
                            else
                            {
                                Batches.Add(currentTimestamp, new Batch());
                                Batches[currentTimestamp].Execute = currentTimestamp;
                                Batches[currentTimestamp].Entries.Add(heartbeat.Id, heartbeat);
                            }

                            heartbeat.NextBeat = currentTimestamp;
                            Context.Heartbeats[pairs.Key] = heartbeat;
                            Processed.Add(heartbeat.Id, heartbeat);
                        }
                        else if (heartbeat.NextBeat == -1)
                        {
                            // This heartbeat has never been executed before
                            int nextBeat = currentTimestamp + heartbeat.Interval;
                            heartbeat.NextBeat = nextBeat;

                            if (Batches.ContainsKey(nextBeat))
                            {
                                Batches[nextBeat].Entries.Add(heartbeat.Id, heartbeat);
                            }
                            else
                            {
                                Batches.Add(nextBeat, new Batch());
                                Batches[nextBeat].Execute = nextBeat;
                                Batches[nextBeat].Entries.Add(heartbeat.Id, heartbeat);
                            }

                            heartbeat.NextBeat = nextBeat;
                            Context.Heartbeats[pairs.Key] = heartbeat;
                            Processed.Add(heartbeat.Id, heartbeat);
                        }
                        else
                        {
                            // This heartbeat has everything going for itself
                            if (Batches.ContainsKey(heartbeat.NextBeat))
                            {
                                Batches[heartbeat.NextBeat].Entries.Add(heartbeat.Id, heartbeat);
                            }
                            else
                            {
                                Batches.Add(heartbeat.NextBeat, new Batch());
                                Batches[heartbeat.NextBeat].Execute = heartbeat.NextBeat;
                                Batches[heartbeat.NextBeat].Entries.Add(heartbeat.Id, heartbeat);
                            }

                            Context.Heartbeats[pairs.Key] = heartbeat;
                            Processed.Add(heartbeat.Id, heartbeat);
                        }
                    }
                }
                else
                {
                    // We are not recalculating, so just go your own way *random fleetwood mac*
                    foreach (System.Collections.Generic.KeyValuePair<int, Heartbeat> pairs in Context.Heartbeats)
                    {
                        if (Processed.ContainsKey(pairs.Value.Id))
                        {
                            continue;
                        }

                        Heartbeat heartbeat = pairs.Value;

                        if (Batches.ContainsKey(heartbeat.NextBeat))
                        {
                            Batches[heartbeat.NextBeat].Entries.Add(heartbeat.Id, heartbeat);
                        }
                        else
                        {
                            Batches.Add(heartbeat.NextBeat, new Batch());
                            Batches[heartbeat.NextBeat].Execute = heartbeat.NextBeat;
                            Batches[heartbeat.NextBeat].Entries.Add(heartbeat.Id, heartbeat);
                        }

                        Context.Heartbeats[pairs.Key] = heartbeat;
                        Processed.Add(heartbeat.Id, heartbeat);
                        Executing.Remove(heartbeat.Id);
                    }
                }

                Thread.Sleep(1000 / 128);
            }
        }

        // THREAD
        public static void ExecuteBatches()
        {
            while (true)
            {
                int currentTimestamp = Utils.GetCurrentTimestamp();

                if (Executing.ContainsKey(currentTimestamp))
                {
                    continue;
                }

                Executing.Add(currentTimestamp, true);

                if (Batches.ContainsKey(currentTimestamp))
                {
                    Batch currentBatch = Batches[currentTimestamp];

                    foreach (System.Collections.Generic.KeyValuePair<int, Heartbeat> pairs in currentBatch.Entries)
                    {
                        Heartbeat currentBeat = pairs.Value;
                        Context.LuaEngine["result"] = 0;

                        // Execute LUA script
                        Context.LuaEngine.DoFile("scripts/" + currentBeat.Script);

                        // Send heartbeat if result is 1
                        if (int.Parse(Context.LuaEngine["result"].ToString()) == 1)
                        {
                            Console.WriteLine(currentBeat.Name + " met interval " + currentBeat.Interval + " op " + currentBeat.NextBeat + " (" + currentTimestamp + ")");
                            //SendHeartbeat(currentBeat.Key);
                        }

                        currentBeat.NextBeat = currentTimestamp + currentBeat.Interval;
                        Processed.Remove(currentBeat.Id);
                        Context.Heartbeats[currentBeat.Id] = currentBeat;
                    }
                }

                Thread.Sleep(1000 / 128);
            }
        }
    }
}
