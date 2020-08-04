using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalSerialization.Tests.Conversion
{
    public class FarmResponse
    {
        public FarmResponse()
        {
            Items = new List<string>();
        }

        public List<string> Items { get; set; }
        public int BuildingCount { get; set; }
        public int AnimalLegCount { get; set; }
    }
}
