using AnimalSerialization.Tests.Conversion;
using AnimalsSerialization.Tests.SampleDocs;
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
            string yaml = YAMLDocs.AnimalGenericYaml;

            //Act
            FarmResponse response = conversion.ConvertFarm(yaml);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("dogstring", response.Items[0]);
            Assert.AreEqual("barnstring", response.Items[1]);
            Assert.AreEqual(0, response.AnimalLegCount);
            Assert.AreEqual(0, response.BuildingCount);
            Assert.AreEqual(0, response.BarnTools.Count);
        }

        [TestMethod]
        public void AlexAnimalDogStringTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            string yaml = YAMLDocs.AnimalDogYaml;

            //Act
            FarmResponse response = conversion.ConvertFarm(yaml);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("Rover", response.Items[0]);
            Assert.AreEqual("barnstring", response.Items[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
            Assert.AreEqual(0, response.BuildingCount);
            Assert.AreEqual(0, response.BarnTools.Count);
        }

        [TestMethod]
        public void AlexAnimalStringBarnTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            string yaml = YAMLDocs.AnimalBarnYaml;

            //Act
            FarmResponse response = conversion.ConvertFarm(yaml);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("dogstring", response.Items[0]);
            Assert.AreEqual("New England barn", response.Items[1]);
            Assert.AreEqual(0, response.AnimalLegCount);
            Assert.AreEqual(1, response.BuildingCount);
            Assert.AreEqual(3, response.BarnTools.Count);
        }

        [TestMethod]
        public void AlexAnimalDogBarnTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            string yaml = YAMLDocs.AnimalDogBarnYaml;

            //Act
            FarmResponse response = conversion.ConvertFarm(yaml);

            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Items.Count);
            Assert.AreEqual("Rover", response.Items[0]);
            Assert.AreEqual("New England barn", response.Items[1]);
            Assert.AreEqual(4, response.AnimalLegCount);
            Assert.AreEqual(1, response.BuildingCount);
            Assert.AreEqual(3, response.BarnTools.Count);
        }

        //This test is meant to fail, the code is not expecting the Barn/dog combination
        //Note that this solution does not currently return feedback to indiciate failure
        [TestMethod]
        public void AlexInvalidAnimalDogTractorTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            string yaml = YAMLDocs.AnimalDogTractorYaml;

            //Act
            FarmResponse response = conversion.ConvertFarm(yaml);

            //Assert
            Assert.IsNull(response);
        }

    }
}