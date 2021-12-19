using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class FolderOfYAMLFilesTests
    {

        [TestMethod]
        public void ProcessFolderOfYAMLTest()
        {
            //Arrange
            //Files downloaded from repo at: https://github.com/microsoft/azure-pipelines-yaml
            string sourceFolder = Path.Combine(Directory.GetCurrentDirectory(), "yamlFiles");
            string[] files = Directory.GetFiles(sourceFolder);
            Conversion conversion = new Conversion();
            List<string> comments = new List<string>();

            //Act            
            foreach (string path in files) //convert every YML file in the folder
            {
                try
                {
                    //Open the file
                    string yaml = File.ReadAllText(path);
                    //Process the yaml string
                    ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);
                    //Add any comments to a string list list 
                    comments.AddRange(gitHubOutput.comments);
                }
                catch (Exception ex)
                {
                    Assert.AreEqual("", "File: " + Path.GetFileName(path) + ", Exception: " + ex.ToString());
                }
            }

            //Assert
            //TODO: Solve roadblocks with the "FilesToIgnore"
            //Check if any errors were detected
            Assert.AreEqual(null, comments.FirstOrDefault(s => s.Contains("#Error:")));
            //Check that the remaining comments equals what we expect
            Assert.AreEqual(33, comments.Count);
        }

    }
}