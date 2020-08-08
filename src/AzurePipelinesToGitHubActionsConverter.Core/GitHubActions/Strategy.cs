using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Strategy
    {
        //strategy:
        //  matrix:
        //    node: [6, 8, 10]
        //steps:
        //  - uses: actions/setup-node@v1
        //    with:
        //      node-version: ${{ matrix.node }}

        //runs-on: ${{ matrix.os }}
        //strategy:
        //  matrix:
        //    os: [ubuntu-16.04, ubuntu-18.04]
        //    node: [6, 8, 10]
        //steps:
        //  - uses: actions/setup-node@v1
        //    with:
        //      node-version: ${{ matrix.node }}

        public Dictionary<string, string[]> matrix { get; set; }
        public string max_parallel { get; set; }
    }
}
