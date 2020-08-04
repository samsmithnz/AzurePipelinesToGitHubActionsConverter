using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalSerialization.Tests.Conversion
{
    public class FarmResponse
    {
        public FarmResponse()
        {
            AnimalNames = new List<string>();
        }

        public List<string> AnimalNames { get; set; }
        public int AnimalLegCount { get; set; }
    }
}
