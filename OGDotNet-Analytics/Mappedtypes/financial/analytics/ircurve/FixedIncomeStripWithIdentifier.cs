//-----------------------------------------------------------------------
// <copyright file="FixedIncomeStripWithIdentifier.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.Financial.Analytics.IRCurve
{
    public class FixedIncomeStripWithIdentifier 
    {
        private readonly StripInstrumentType _instrumentType;
        private readonly Tenor _maturity;
        private readonly Identifier _security;

        public FixedIncomeStripWithIdentifier(StripInstrumentType instrumentType, Tenor maturity, Identifier security)
        {
            _instrumentType = instrumentType;
            _maturity = maturity;
            _security = security;
        }

        public StripInstrumentType InstrumentType
        {
            get { return _instrumentType; }
        }

        public Tenor Maturity
        {
            get { return _maturity; }
        }

        public Identifier Security
        {
            get { return _security; }
        }
    }
}