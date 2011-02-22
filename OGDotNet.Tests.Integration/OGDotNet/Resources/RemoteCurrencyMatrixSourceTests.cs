﻿using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.financial.currency;
using Xunit;
using Currency = OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteCurrencyMatrixSourceTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            Assert.NotNull(Context.CurrencyMatrixSource);
        }

        [Fact]
        public void CanGetNonExistantMatrix()
        {
            var source = Context.CurrencyMatrixSource;
            Assert.Null(source.GetCurrencyMatrix("NonExistant"+Guid.NewGuid()));
        }

        [Fact]
        public void CanGetBloombergMatrix()
        {
            var source = Context.CurrencyMatrixSource;
            var currencyMatrix = source.GetCurrencyMatrix("BloombergLiveData");
            Assert.NotNull(currencyMatrix);

            Assert.True(currencyMatrix.SourceCurrencies.Contains(Currency.Create("USD")));
            Assert.True(currencyMatrix.SourceCurrencies.Contains(Currency.Create("GBP")));
            Assert.True(currencyMatrix.TargetCurrencies.Contains(Currency.Create("USD")));
            Assert.True(currencyMatrix.TargetCurrencies.Contains(Currency.Create("GBP")));

            var valueTypes = new HashSet<Type>();
            var matchedTargets = new HashSet<Currency>();
            foreach (var sourceCurrency in currencyMatrix.SourceCurrencies)
            {
                var foundTargets = currencyMatrix.TargetCurrencies.Where(targetCurrency => currencyMatrix.GetConversion(sourceCurrency, targetCurrency) != null).ToList();
                Assert.NotEmpty(foundTargets);
                foreach (var foundTarget in foundTargets)
                {
                    matchedTargets.Add(foundTarget);

                    var fwd = currencyMatrix.GetConversion(sourceCurrency,foundTarget);
                    var bwd = currencyMatrix.GetConversion(foundTarget, sourceCurrency);

                    Assert.Equal(bwd, fwd.GetReciprocal());
                    Assert.Equal(fwd, bwd.GetReciprocal());
                    valueTypes.Add(fwd.GetType());
                }

                Assert.Contains(Currency.Create("USD"), foundTargets);
                Assert.Contains(sourceCurrency, foundTargets);
                var identityConversion = currencyMatrix.GetConversion(sourceCurrency, sourceCurrency);
                Assert.True(identityConversion is CurrencyMatrixValue.CurrencyMatrixFixed);
                Assert.Equal(1.0,((CurrencyMatrixValue.CurrencyMatrixFixed)identityConversion).FixedValue);
            }

            foreach (var targetCurrency in currencyMatrix.TargetCurrencies)
            {
                Assert.Contains(targetCurrency,matchedTargets);
            }
        }
    }
}
