using AnimalSerialization.Tests.Models;

namespace AnimalsSerialization.Tests
{
    public class JSONDocs
    {
        public string AnimalGenericJson;
        public string AnimalDogJson;
        public string AnimalBarnJson;
        public string AnimalDogBarnJson;
        public string AnimalDogTractorJson;
        public JSONDocs()
        {
            AnimalGenericJson = @"
{
  ""FarmItem1"": ""dogstring"",
  ""FarmItem2"": ""barnstring""
}
";
            AnimalDogJson = @"
{
  ""FarmItem1"": 
  {
    ""Name"": ""Rover"",
    ""NumberOfLegs"": 4,
    ""DogVocab"": null
  },
  ""FarmItem2"": ""barnstring""
}
";
            AnimalBarnJson = @"
{
  ""FarmItem1"": ""dogstring"",
  ""FarmItem2"": 
  {
    ""BarnType"": ""New England barn"",
    ""Color"": ""Red"",
    ""Equipment"": null
  }
}
";
            AnimalDogBarnJson = @"
{ 
  ""FarmItem1"": 
  {
    ""Name"": ""Rover"",
    ""NumberOfLegs"": 0,
    ""DogVocab"": null
  },
  ""FarmItem2"": 
  {
    ""BarnType"": ""New England barn"",
    ""Color"": ""Red"",
    ""Equipment"": null
  }
}
";
            AnimalDogTractorJson = @"
{ 
  ""FarmItem1"": 
  {
    ""Name"": ""Rover"",
    ""NumberOfLegs"": 0,
    ""DogVocab"": null
  },
  ""FarmItem2"": 
  {
    ""name"": ""John Deer utility tractor"",
    ""Color"": ""Green"",
    ""NumberOfWheels"": 4
  }
}
";

        }
    }
}
