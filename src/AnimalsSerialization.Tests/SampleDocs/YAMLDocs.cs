using AnimalSerialization.Tests.Models;

namespace AnimalsSerialization.Tests.SampleDocs
{
    public static class YAMLDocs
    {
        public static string AnimalStringStringYaml = @"
FarmItem1: dogstring
FarmItem2: barnstring
";
        public static string AnimalObjectStringYaml = @"
FarmItem1: 
  Name: Rover
  NumberOfLegs: 4
FarmItem2: barnstring
";
        public static string AnimalStringObjectYaml = @"
FarmItem1: dogstring
FarmItem2: 
  BarnType: New England barn
  Color: Red
  Tools:
  - Hammer
  - Wrench
  - Shovel
";
        public static string AnimalObjectObjectYaml = @"
FarmItem1: 
  Name: Rover
  NumberOfLegs: 0
FarmItem2: 
  BarnType: New England barn
  Color: Red
  Tools:
  - Hammer
  - Wrench
  - Shovel
";
        public static string AnimalDogTractorYaml = @"
FarmItem1: 
  Name: Rover
  NumberOfLegs: 0
FarmItem2: 
  Name: John Deer utility tractor
  Color: Green
  NumberOfWheels: 4
";

    }
}
