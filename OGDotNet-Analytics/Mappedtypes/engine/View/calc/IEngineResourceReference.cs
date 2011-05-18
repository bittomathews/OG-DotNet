﻿//-----------------------------------------------------------------------
// <copyright file="IEngineResourceReference.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
    public interface IEngineResourceReference<out T> : IDisposable
    {
        T Value { get; }
    }
}
