using GitHubActionsDotNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzurePipelinesToGitHubActionsConverterCore = AzurePipelinesToGitHubActionsConverter.Core;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class GitHubSerializationTests
    {

        [TestMethod]
        public void GitHubDeserializationTest()
        {
            //Arrange
            string yaml = @"
on:
  push:
    branches:
    - main
jobs:
  build:
    runs-on: ubuntu-latest
    name: Build 1
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world!""
      shell: powershell
";

            //Act
            GitHubActionsRoot gitHubAction = AzurePipelinesToGitHubActionsConverter.Core.Serialization.GitHubActionsSerialization.Deserialize(yaml);

            //Assert
            Assert.AreNotEqual(null, gitHubAction);
            Assert.AreEqual(null, gitHubAction.env); //environment variables are null

            //Test for messages and name
            Assert.AreEqual(0, gitHubAction.messages.Count);
            Assert.AreEqual(null, gitHubAction.name);

            //Test the trigger
            Assert.AreNotEqual(null, gitHubAction.on);
            Assert.AreEqual(null, gitHubAction.on.pull_request);
            Assert.AreNotEqual(null, gitHubAction.on.push);
            Assert.AreEqual(1, gitHubAction.on.push.branches.Length);
            Assert.AreEqual("main", gitHubAction.on.push.branches[0]);
            Assert.AreEqual(null, gitHubAction.on.push.branches_ignore);
            Assert.AreEqual(null, gitHubAction.on.push.paths);
            Assert.AreEqual(null, gitHubAction.on.push.paths_ignore);
            Assert.AreEqual(null, gitHubAction.on.push.tags);
            Assert.AreEqual(null, gitHubAction.on.push.tags_ignore);
            Assert.AreEqual(null, gitHubAction.on.schedule);

            //Test that jobs exist
            Assert.AreNotEqual(null, gitHubAction.jobs);
            Assert.AreEqual(1, gitHubAction.jobs.Count);
            Assert.AreEqual(true, gitHubAction.jobs.ContainsKey("build"));
            gitHubAction.jobs.TryGetValue("build", out Job gitHubJob);
            Assert.AreEqual(null, gitHubJob._if);
            Assert.AreEqual("Build 1", gitHubJob.name);
            Assert.AreEqual("windows-latest", gitHubJob.runs_on);

            //Test that steps exist
            Assert.AreNotEqual(null, gitHubJob.steps);
            Assert.AreEqual(2, gitHubJob.steps.Length);
        }


        [TestMethod]
        public void GitHubActionYamlToGenericObjectTest()
        {
            //Arrange
            string yaml = @"
on:
  schedule:
  - cron: ""0 0 * * *""
";

            //Act
            object yamlObject = AzurePipelinesToGitHubActionsConverterCore.Serialization.YamlSerialization.DeserializeYaml<object>(yaml);

            //Assert
            Assert.AreNotEqual(null, yamlObject);
        }
    }
}
