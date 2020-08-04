using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalSerialization.Tests.Models
{
    public class Cat
    {
        public string Name { get; set; }
        public int NumberOfLegs { get; set; }
        public List<string> CatVocab { get; set; }
    }
}
