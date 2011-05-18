﻿//-----------------------------------------------------------------------
// <copyright file="IViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.View.compilation;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
    public interface IViewCycle : IUniqueIdentifiable
    {
        // TODO ICompiledViewDefinition GetCompiledViewDefinition();
        ViewComputationResultModel GetResultModel();
        ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery);
    }
}