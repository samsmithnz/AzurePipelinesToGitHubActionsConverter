using AnimalSerialization.Tests.Conversion;
using AnimalsSerialization.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnimalSerialization.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class AlexFarmTests
    {
        [TestMethod]
        public void AlexAnimalStringStringTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            JSONDocs docs = new JSONDocs();
            string json = docs.AnimalGenericJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("dogstring", response.Items[0]);
            Assert.AreEqual("barnstring", response.Items[1]);
            Assert.AreEqual(0, response.AnimalLegCount);
            Assert.AreEqual(0, response.BuildingCount);
        }

        [TestMethod]
        public void AlexAnimalDogStringTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            JSONDocs docs = new JSONDocs();
            string json = docs.AnimalDogJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("Rover", response.Items[0]);
            Assert.AreEqual("barnstring", response.Items[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
            Assert.AreEqual(0, response.BuildingCount);
        }

        [TestMethod]
        public void AlexAnimalStringBarnTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            JSONDocs docs = new JSONDocs();
            string json = docs.AnimalBarnJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("dogstring", response.Items[0]);
            Assert.AreEqual("New England barn", response.Items[1]);
            Assert.AreEqual(0, response.AnimalLegCount);
            Assert.AreEqual(1, response.BuildingCount);
        }

        [TestMethod]
        public void AlexAnimalDogBarnTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            JSONDocs docs = new JSONDocs();
            string json = docs.AnimalDogBarnJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("Rover", response.Items[0]);
            Assert.AreEqual("New England barn", response.Items[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
            Assert.AreEqual(1, response.BuildingCount);
        }

        //This test is meant to fail, the code is not expecting the Barn/dog combination
        [TestMethod]
        public void AlexInvalidAnimalDogTractorTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            JSONDocs docs = new JSONDocs();
            string json = docs.AnimalDogTractorJson;

            //Act
            FarmResponse response = conversion.ConvertFarm(json);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("Rover", response.Items[0]);
            Assert.AreEqual(null, response.Items[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
            Assert.AreEqual(1, response.BuildingCount);
        }

    }
}