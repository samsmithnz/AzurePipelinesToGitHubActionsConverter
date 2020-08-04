using AnimalSerialization.Tests.Conversion;
using AnimalSerialization.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnimalSerialization.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class FarmTests
    {
        public Farm<string, string> AnimalGeneric;
        public Farm<Dog, string> AnimalDog;
        public Farm<string, Cat> AnimalCat;
        public Farm<Dog, Cat> AnimalDogCat;

        public string AnimalGenericJson;
        public string AnimalDogJson;
        public string AnimalCatJson;
        public string AnimalDogCatJson;

        [TestInitialize]
        public void InitializeTests()
        {
            //AnimalGeneric = new Farm
            //{
            //    Animal1 = "dogstring",
            //    Animal2 = "catstring"
            //};
            //AnimalGeneric = new Farm<string, string>
            //{
            //    Animal1 = "dogstring",
            //    Animal2 = "catstring"
            //};
            //AnimalDog = new Farm<Dog, string>
            //{
            //    Animal1 = new Dog(),
            //    Animal2 = "catstring"
            //};
            //AnimalCat = new Farm<string, Cat>
            //{
            //    Animal1 = "dogstring",
            //    Animal2 = new Cat()
            //};
            //AnimalDogCat = new Farm<Dog, Cat>
            //{
            //    Animal1 = new Dog(),
            //    Animal2 = new Cat()
            //};
            AnimalGenericJson = @"
{
  ""animal1"": ""dogstring"",
  ""animal2"": ""catstring""
}
";
            AnimalDogJson = @"
{
  ""animal1"": 
  {
    ""name"": ""Rover"",
    ""legs"": 4,
    ""dogVocab"": null
  },
  ""animal2"": ""catstring""
}
";
            AnimalCatJson = @"
{
  ""animal1"": ""dogstring"",
  ""animal2"": 
  {
    ""name"": ""Kitty"",
    ""numberOfLegs"": 4,
    ""catVocab"": null
  }
    }
";
            AnimalDogCatJson = @"
{ 
  ""animal1"": 
  {
    ""name"": ""Rover"",
    ""legs"": 0,
    ""dogVocab"": null
  },
  ""animal2"": 
  {
    ""name"": ""Kitty"",
    ""numberOfLegs"": 4,
    ""catVocab"": null
  }
}
";
        }

        [TestMethod]
        public void AnimalStringStringTest()
        {
            //Arrange
            FarmConversion conversion = new FarmConversion();
            string json = AnimalGenericJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.AnimalNames.Count);
            Assert.AreEqual("dogstring", response.AnimalNames[0]);
            Assert.AreEqual("catstring", response.AnimalNames[1]);
            Assert.AreEqual(0, response.AnimalLegCount);
        }

        [TestMethod]
        public void AnimalDogStringTest()
        {
            //Arrange
            FarmConversion conversion = new FarmConversion();
            string json = AnimalDogJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.AnimalNames.Count);
            Assert.AreEqual("Rover", response.AnimalNames[0]);
            Assert.AreEqual("catstring", response.AnimalNames[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
        }

        [TestMethod]
        public void AnimalStringCatTest()
        {
            //Arrange
            FarmConversion conversion = new FarmConversion();
            string json = AnimalCatJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.AnimalNames.Count);
            Assert.AreEqual("dogstring", response.AnimalNames[0]);
            Assert.AreEqual("Kitty", response.AnimalNames[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
        }

        [TestMethod]
        public void AnimalDogCatTest()
        {
            //Arrange
            FarmConversion conversion = new FarmConversion();
            string json = AnimalDogCatJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.AnimalNames.Count);
            Assert.AreEqual("Rover", response.AnimalNames[0]);
            Assert.AreEqual("Kitty", response.AnimalNames[1]);
            Assert.AreEqual(8, response.AnimalLegCount);
        }

    }
}