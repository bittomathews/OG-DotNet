﻿//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycleTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine.depgraph;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Model.Resources;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewCycleTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void CanGetCycle()
        {
            WithViewCycle(
           delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
           {
               Assert.NotNull(cycle.UniqueId);
               var resultModel = cycle.GetResultModel();
               Assert.NotNull(resultModel);

               var computedValue = resultModel.AllResults.First().ComputedValue;
               var valueSpec = computedValue.Specification;

               var nonEmptyResponse = cycle.QueryComputationCaches(new ComputationCacheQuery("Default", valueSpec));

               Assert.NotNull(nonEmptyResponse);

               var results = nonEmptyResponse.Results;
               Assert.NotEmpty(results);
               Assert.Equal(1, results.Count());
               Assert.Equal(computedValue.Specification, results.Single().First);
               Assert.Equal(computedValue.Value, results.Single().Second);

               Assert.NotNull(cycle.GetViewProcessId());
               Assert.Equal(ViewCycleState.Executed, cycle.GetState());
               var duration = cycle.GetDurationNanos();
               Assert.InRange(duration, 10, long.MaxValue);
           });
        }

        [Xunit.Extensions.Fact]
        public void CantDoStupidCacheQuery()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                Assert.Throws<ArgumentException>(() => cycle.QueryComputationCaches(new ComputationCacheQuery("Default")));
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetCycleById()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                using (
                    var refById = client.CreateCycleReference(cycle.UniqueId))
                {
                    Assert.Equal(refById.Value.UniqueId, cycle.UniqueId);
                }
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetViewDefintion()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                Assert.NotNull(compiledViewDefinition.ViewDefinition);
                Assert.NotEmpty(compiledViewDefinition.CompiledCalculationConfigurations);
                Assert.Equal(compiled.CompiledViewDefinition.CompiledCalculationConfigurations.Keys, compiledViewDefinition.CompiledCalculationConfigurations.Keys);

                Assert.Equal(compiled.CompiledViewDefinition.EarliestValidity, compiledViewDefinition.EarliestValidity);
                Assert.Equal(compiled.CompiledViewDefinition.LatestValidity, compiledViewDefinition.LatestValidity);

                Assert.NotEmpty(compiledViewDefinition.LiveDataRequirements);
                Assert.Equal(compiled.CompiledViewDefinition.LiveDataRequirements.Count, compiledViewDefinition.LiveDataRequirements.Count);

                Assert.NotNull(compiledViewDefinition.Portfolio);
                Assert.Equal(compiled.CompiledViewDefinition.Portfolio.UniqueId, compiledViewDefinition.Portfolio.UniqueId);
            });
        }

        [Xunit.Extensions.Fact]
        public void CanGetGraphs()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                var compiledViewDefinition = cycle.GetCompiledViewDefinition();
                var resultModel = cycle.GetResultModel();
                foreach (var kvp in compiledViewDefinition.ViewDefinition.CalculationConfigurationsByName)
                {
                    var viewCalculationConfiguration = kvp.Key;

                    var vreToTest = resultModel.AllResults.OrderBy(r=>r.GetHashCode()).First(r => r.CalculationConfiguration == viewCalculationConfiguration);
                    var specToTest = vreToTest.ComputedValue.Specification;

                    var dependencyGraphExplorer = compiledViewDefinition.GetDependencyGraphExplorer(viewCalculationConfiguration);
                    Assert.NotNull(dependencyGraphExplorer);
                    var subgraph = dependencyGraphExplorer.GetSubgraphProducing(specToTest);
                    Assert.NotNull(subgraph);

                    Assert.Equal(viewCalculationConfiguration, subgraph.CalculationConfigurationName);
                    Assert.NotEmpty(subgraph.DependencyNodes);
                    Assert.True(subgraph.DependencyNodes.Any(n => Produces(n, specToTest)));
                    
                    foreach (var node in subgraph.DependencyNodes)
                    {
                        Assert.NotEmpty(node.OutputValues);
                    }

                    var lastNode = subgraph.DependencyNodes.Single(n => Produces(n, specToTest));
                    Assert.True(lastNode.TerminalOutputValues.Contains(specToTest));
                    
                    //Check the graph is connected
                    Assert.Equal(FollowInputs(lastNode).Count, subgraph.DependencyNodes.Count);

                    WriteToDot(subgraph, viewCalculationConfiguration+".dot");
                }
            });
        }

        private static void WriteToDot(IDependencyGraph subgraph, string dotFileName)
        {
            using (var streamWriter = new StreamWriter(dotFileName))
            {
                streamWriter.WriteLine("digraph graphname {");
                var dependencyNodes = subgraph.DependencyNodes.ToList();
                Dictionary<DependencyNode, int> map = dependencyNodes.Select((n, i) => Tuple.Create(n, i)).ToDictionary(t => t.Item1, t => t.Item2);
                for (int index = 0; index < dependencyNodes.Count; index++)
                {
                    var dependencyNode = dependencyNodes[index];
                    streamWriter.WriteLine(string.Format("{0} [label=\"{1}\"];", index, dependencyNode.Target.UniqueId));
                }

                for (int index = 0; index < dependencyNodes.Count; index++)
                {
                    var dependencyNode = dependencyNodes[index];
                    foreach (var inputNode in dependencyNode.InputNodes)
                    {
                        streamWriter.WriteLine(string.Format("{0} -> {1};", map[inputNode], index));
                    }
                }
                streamWriter.WriteLine("}");
            }
        }

        private ISet<DependencyNode> FollowInputs(DependencyNode dependencyNode)
        {
            var set = new HashSet<DependencyNode>();
            FollowInputs(set, dependencyNode);
            return set;
        }

        private void FollowInputs(HashSet<DependencyNode> nodes, DependencyNode dependencyNode)
        {
            if (nodes.Contains(dependencyNode))
                return;
            nodes.Add(dependencyNode);
            foreach (var inputNode in dependencyNode.InputNodes)
            {
                FollowInputs(nodes, inputNode);
            }
        }

        private static bool Produces(DependencyNode n, ValueSpecification specToTest)
        {
            var targetMatches = n.Target.UniqueId == specToTest.TargetSpecification.Uid && n.Target.Type == specToTest.TargetSpecification.Type;
            return targetMatches && n.OutputValues.Any(s => s.Equals(specToTest));
        }

        [Xunit.Extensions.Fact]
        public void CycleStaysAlive()
        {
            WithViewCycle(
            delegate(ViewDefinitionCompiledArgs compiled, IViewCycle cycle, RemoteViewClient client)
            {
                Thread.Sleep(10000);
                Assert.Equal(ViewCycleState.Executed, cycle.GetState());
            });
        }

        private static void WithViewCycle(Action<ViewDefinitionCompiledArgs, IViewCycle, RemoteViewClient> action)
        {
            using (var executedMre = new ManualResetEventSlim(false))
            using (var remoteViewClient = Context.ViewProcessor.CreateClient())
            {
                ViewDefinitionCompiledArgs compiled = null;

                var listener = new EventViewResultListener();
                listener.ProcessCompleted += delegate { executedMre.Set(); };
                listener.ViewDefinitionCompiled += delegate(object sender, ViewDefinitionCompiledArgs e) { compiled = e; };

                remoteViewClient.SetResultListener(listener);
                remoteViewClient.SetViewCycleAccessSupported(true);
                remoteViewClient.AttachToViewProcess("Equity Option Test View 1", ExecutionOptions.SingleCycle);
                Assert.Null(remoteViewClient.CreateLatestCycleReference());

                executedMre.Wait(TimeSpan.FromMinutes(1));
                Assert.NotNull(compiled);

                using (var engineResourceReference = remoteViewClient.CreateLatestCycleReference())
                {
                    action(compiled, engineResourceReference.Value, remoteViewClient);
                }
            }
        }
    }
}