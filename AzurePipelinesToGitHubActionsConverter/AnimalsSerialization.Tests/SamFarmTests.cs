using AnimalSerialization.Tests.Conversion;
using AnimalsSerialization.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnimalSerialization.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class SamFarmTests
    {
        [TestMethod]
        public void SamAnimalStringStringTest()
        {
            //Arrange
            FarmConversionSam conversion = new FarmConversionSam();
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
        public void SamAnimalDogStringTest()
        {
            //Arrange
            FarmConversionSam conversion = new FarmConversionSam();
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
        public void SamAnimalStringBarnTest()
        {
            //Arrange
            FarmConversionSam conversion = new FarmConversionSam();
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
        public void SamAnimalDogBarnTest()
        {
            //Arrange
            FarmConversionSam conversion = new FarmConversionSam();
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

        //This test is meant to fail, the code is not expecting the Barn/dog combination. 
        //By a quirk of the branching, it will jump into the last Dog/Barn combination, but as the barn object only matches color, most of the object will be null
        [TestMethod]
        public void SamInvalidAnimalDogTractorTest()
        {
            //Arrange
            FarmConversionSam conversion = new FarmConversionSam();
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