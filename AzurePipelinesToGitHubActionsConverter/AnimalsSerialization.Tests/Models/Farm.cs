using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalSerialization.Tests.Models
{
    public class Farm<TAnimal1, TAnimal2>
    {
        public TAnimal1 Animal1 { get; set; }
        public TAnimal2 Animal2 { get; set; }
    }
}
