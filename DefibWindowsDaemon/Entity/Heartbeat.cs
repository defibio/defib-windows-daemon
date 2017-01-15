using System;

namespace Defib.Entity
{
    public class Heartbeat : ICloneable
    {
        public int Id;
        public string Name;
        public string Key;
        public int Interval;
        public int NextBeat;
        public int LastBeat;
        public string Script;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}