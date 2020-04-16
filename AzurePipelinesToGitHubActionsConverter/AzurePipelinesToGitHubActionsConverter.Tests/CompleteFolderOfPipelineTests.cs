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
    [TestClass]
    public class CompleteFolderOfPipelineTests
    {

        [TestMethod]
        public async Task ProcessFolderOfYAMLTest()
        {
            //Arrange
            //Files downloaded from repo at: https://github.com/microsoft/azure-pipelines-yaml
            string sourceFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\yamlFiles";
            string[] files = Directory.GetFiles(sourceFolder);
            Conversion conversion = new Conversion();
            List<string> comments = new List<string>();

            //Act
            //convert every file in the folder
            foreach (string file in files)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string yaml = await sr.ReadToEndAsync();
                        ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);
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
            Assert.AreEqual(null, comments.FirstOrDefault(s => s.Contains("Error!")));
            Assert.AreEqual(15, comments.Count);
        }

    }
}