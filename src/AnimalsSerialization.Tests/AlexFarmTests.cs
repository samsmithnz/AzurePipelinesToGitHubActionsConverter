using AnimalSerialization.Tests.Conversion;
using AnimalsSerialization.Tests.SampleDocs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            string yaml = YAMLDocs.AnimalStringStringYaml;

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
            string yaml = YAMLDocs.AnimalObjectStringYaml;

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
            string yaml = YAMLDocs.AnimalStringObjectYaml;

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
            string yaml = YAMLDocs.AnimalObjectObjectYaml;

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
        //Cancel that second point, it does now, as I removed the error handling from the last deserializing statement
        [TestMethod]
        public void AlexInvalidAnimalDogTractorTest()
        {
            //Arrange
            FarmConversionAlex conversion = new FarmConversionAlex();
            string yaml = YAMLDocs.AnimalDogTractorYaml;
            FarmResponse response = null;

            //Act
            try
            {
                response = conversion.ConvertFarm(yaml);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
                //string expectedMessage = "(Line: 6, Col: 3, Idx: 64) - (Line: 6, Col: 3, Idx: 64): Exception during deserialization";
                //Assert.AreEqual(expectedMessage, ex.Message);
                Assert.IsTrue(ex.Message.IndexOf("Exception during deserialization") >= 0);
            }

            //Assert
            Assert.IsNull(response);
        }

    }
}