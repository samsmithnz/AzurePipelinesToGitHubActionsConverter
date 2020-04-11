﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines.Repositories._ref")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:AzurePipelinesToGitHubActionsConverter.Core.GitHubActions.Job._if")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~P:AzurePipelinesToGitHubActionsConverter.Core.GitHubActions.Step._if")]
[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>", Scope = "member", Target = "~M:AzurePipelinesToGitHubActionsConverter.Core.Conversion.ConditionsProcessing.TranslateConditions(System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "<Pending>", Scope = "member", Target = "~M:AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization.AzurePipelinesSerialization`2.CleanYamlBeforeDeserialization(System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "<Pending>", Scope = "member", Target = "~M:AzurePipelinesToGitHubActionsConverter.Core.Conversion.ConditionsProcessing.SplitContents(System.String)~System.Collections.Generic.List{System.String}")]
//https://stackoverflow.com/questions/11359652/suppressmessage-for-a-whole-namespace
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "AzurePipelinesToGitHubActionsConverter.Core")]
