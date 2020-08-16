using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalSerialization.Tests.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Barn
    {
        public string BarnType { get; set; }
        public string Color { get; set; }
        public List<string> Tools { get; set; }
    }
}
