using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class GitHubActionsRoot
    {
        public string name { get; set; }
        public string[] on { get; set; }
        public Dictionary<string, string> env { get; set; }
        public Dictionary<string, Job> jobs { get; set; }
    }
}

//name: CI

//on: [push]

//jobs:
//  build:

//    runs-on: ubuntu-latest

//    steps:
//    # checkout the repo
//    - uses: actions/checkout @v1

//    # install dependencies, build, and test
//    - name: Build with dotnet
//      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: Publish with dotnet
//      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: publish build artifacts back to GitHub
//      uses: actions/upload-artifact @master
//      with:
//        name: serviceapp
//        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish
