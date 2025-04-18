﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace MyBddProject.Tests.Features
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Content Filtering by Streaming Service")]
    [NUnit.Framework.FixtureLifeCycleAttribute(NUnit.Framework.LifeCycle.InstancePerTestCase)]
    public partial class ContentFilteringByStreamingServiceFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Content Filtering by Streaming Service", "  In order to view only desired content\r\n  As a user on the content browsing page" +
                "\r\n  I want to filter content by streaming services", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "StreamingServiceFilter.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
            {
                await testRunner.OnFeatureEndAsync();
            }
            if ((testRunner.FeatureContext == null))
            {
                await testRunner.OnFeatureStartAsync(featureInfo);
            }
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
            global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Filtering content by a single streaming service")]
        public async System.Threading.Tasks.Task FilteringContentByASingleStreamingService()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Filtering content by a single streaming service", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 6
  this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 7
    await testRunner.GivenAsync("the user is on the content browsing page", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 8
    await testRunner.AndAsync("the list of available streaming services is displayed", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 9
    await testRunner.WhenAsync("the user selects a specific streaming service \"Netflix\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 10
    await testRunner.ThenAsync("the content list is updated to show only items available on \"Netflix\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Filtering content by multiple streaming services")]
        public async System.Threading.Tasks.Task FilteringContentByMultipleStreamingServices()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Filtering content by multiple streaming services", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 12
  this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 13
    await testRunner.GivenAsync("the user is on the content browsing page", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 14
    await testRunner.AndAsync("the list of available streaming services is displayed", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 15
    await testRunner.WhenAsync("the user selects streaming services \"Netflix\" and \"Hulu\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 16
    await testRunner.ThenAsync("the content list is updated to show items available on either \"Netflix\" or \"Hulu\"" +
                        "", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Clearing applied streaming service filters")]
        public async System.Threading.Tasks.Task ClearingAppliedStreamingServiceFilters()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Clearing applied streaming service filters", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 18
  this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 19
    await testRunner.GivenAsync("the user has one or more streaming service filters applied", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 20
    await testRunner.WhenAsync("the user clicks the \"Clear Filters\" button", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 21
    await testRunner.ThenAsync("all streaming service filters are removed", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 22
    await testRunner.AndAsync("the content list returns to the default unfiltered view", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Filter response time performance")]
        public async System.Threading.Tasks.Task FilterResponseTimePerformance()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Filter response time performance", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 24
  this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 25
    await testRunner.GivenAsync("the user is on the content browsing page", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 26
    await testRunner.AndAsync("the user has one or more streaming service filters applied", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 27
    await testRunner.WhenAsync("the user selects a streaming service filter", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 28
    await testRunner.ThenAsync("the content list should update within 2 seconds", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
