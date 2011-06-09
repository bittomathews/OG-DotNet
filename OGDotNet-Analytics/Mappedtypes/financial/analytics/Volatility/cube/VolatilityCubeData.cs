﻿//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeData.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.cube
{
    public class VolatilityCubeData
    {
        private readonly Dictionary<VolatilityPoint, double> _dataPoints;

        public VolatilityCubeData(Dictionary<VolatilityPoint, double> dataPoints)
        {
            _dataPoints = dataPoints;
        }

        public Dictionary<VolatilityPoint, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public static VolatilityCubeData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            if (ffc.GetMessage("dataPoints").Any())
            {
                throw new NotImplementedException();
            }
            return new VolatilityCubeData(new Dictionary<VolatilityPoint, double>());
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
