﻿using Defib.Entity;
using System.Collections.Generic;

namespace Defib
{
    public class Batch
    {
        public int Execute;
        public Dictionary<int, Heartbeat> Entries;
    }
}
