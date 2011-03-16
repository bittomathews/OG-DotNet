﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.currency;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class CurrencyMatrixSourcingFunctionTests : TestWithContextBase
    {
        [Fact]
        public void CanGetIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, Currency.Create("USD"), Currency.Create("USD"));
            Assert.Equal(1.0,conversionRate);
        }
        

        [Fact]
        public void CanGetNonIdentity()
        {
            CurrencyMatrixSourcingFunction currencyMatrixSourcingFunction = GetFunction();
            var source = Currency.Create("USD");
            var target = Currency.Create("GBP");

            var conversionRate = currencyMatrixSourcingFunction.GetConversionRate(GetValue, source, target);
            Assert.NotEqual(1.0, conversionRate);
            var reciprocal = currencyMatrixSourcingFunction.GetConversionRate(GetValue, target, source);
            Assert.NotEqual(1.0, reciprocal);

            Assert.InRange(conversionRate, 0.99 / reciprocal, 1.01 / reciprocal );
        }

        private CurrencyMatrixSourcingFunction GetFunction()
        {
            var currencyMatrix = Context.CurrencyMatrixSource.GetCurrencyMatrix("BloombergLiveData");
            return new CurrencyMatrixSourcingFunction(currencyMatrix);
        }

        private object GetValue(ValueRequirement req)
        {
            if (req.TargetSpecification.Type != ComputationTargetType.Primitive)
                throw new NotImplementedException();
            var cancellationTokenSource = new CancellationTokenSource();

            using (var remoteClient = Context.CreateUserClient())
            {
                var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());

                var viewCalculationConfiguration = new ViewCalculationConfiguration("Default", new List<ValueRequirement> { req }, new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
                viewDefinition.CalculationConfigurationsByName.Add("Default", viewCalculationConfiguration);
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));

                try
                {
                    var remoteView = Context.ViewProcessor.GetView(viewDefinition.Name);
                    remoteView.Init();
                    var remoteViewClient = remoteView.CreateClient();
                    foreach (var viewComputationResultModel in remoteViewClient.GetResults(cancellationTokenSource.Token))
                    {
                        foreach (var val in viewComputationResultModel.AllResults)
                        {
                            Debug.Assert(val.CalculationConfiguration == "Default");
                            Debug.Assert(req.IsSatisfiedBy(val.ComputedValue.Specification));
                            return val.ComputedValue.Value;
                        }
                    }
                }
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                }
            }
            throw new NotImplementedException();
        }
    }
}