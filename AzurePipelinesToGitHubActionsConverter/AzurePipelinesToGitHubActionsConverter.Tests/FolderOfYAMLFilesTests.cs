using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class FolderOfYAMLFilesTests
    {

        [TestMethod]
        public async Task ProcessFolderOfYAMLTest()
        {
            //Arrange
            //Files downloaded from repo at: https://github.com/microsoft/azure-pipelines-yaml
            var sourceFolder = Path.Combine(Directory.GetCurrentDirectory(), "yamlFiles");
            string[] files = Directory.GetFiles(sourceFolder);
            Conversion conversion = new Conversion();
            List<string> comments = new List<string>();

            //Act            
            foreach (string file in files) //convert every YML file in the folder
            { 
                try
                {
                    //Open the file
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string yaml = await sr.ReadToEndAsync();
                        //Process the yaml string
                        ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);
                        //Add any comments to a string list list 
                        comments.AddRange(gitHubOutput.comments);
                    }
                }
                catch (Exception ex)
                {
                    Assert.AreEqual("", "File: " + file + ", Exception: " + ex.ToString());
                }
            }

            //Assert
            //TODO: Solve roadblocks with the "FilesToIgnore"
            //Check if any errors were detected
            Assert.AreEqual(null, comments.FirstOrDefault(s => s.Contains("Error!")));
            //Check that the remaining comments equals what we expect
            Assert.AreEqual(17, comments.Count);
        }

    }
}