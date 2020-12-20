using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class ContainerTests
    {
        [TestMethod]
        public void ContainerServicesTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
resources:
  containers:
  - container: my_container
    image: buildpack-deps:focal
  - container: nginx
    image: nginx


pool:
  vmImage: 'ubuntu-20.04'

container: my_container
services:
  nginx: nginx

steps:
- script: |
    curl nginx
  displayName: Show that nginx is running";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#TODO: Container conversion not yet done, we need help!: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39
jobs:
  build:
    runs-on: ubuntu-20.04
    container:
      image: buildpack-deps:focal
    steps:
    - uses: actions/checkout@v2
    - name: Show that nginx is running
      run: curl nginx
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}
