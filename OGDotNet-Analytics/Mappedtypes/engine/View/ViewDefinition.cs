﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Mappedtypes.engine.view
{
    public class ViewDefinition
    {
        private string _name;
        private readonly UniqueIdentifier _portfolioIdentifier;
        private readonly UserPrincipal _user;

        private readonly ResultModelDefinition _resultModelDefinition;

        private readonly long? _minDeltaCalcPeriod;
        private readonly long? _maxDeltaCalcPeriod;

        private readonly long? _minFullCalcPeriod;
        private readonly long? _maxFullCalcPeriod;
        private readonly Currency _defaultCurrency;

        private readonly Dictionary<string, ViewCalculationConfiguration> _calculationConfigurationsByName;


        public ViewDefinition(string name, ResultModelDefinition resultModelDefinition = null, UniqueIdentifier portfolioIdentifier = null, UserPrincipal user = null, Currency defaultCurrency = null, long? minDeltaCalcPeriod = null, long? maxDeltaCalcPeriod = null, long? minFullCalcPeriod = null, long? maxFullCalcPeriod = null, Dictionary<string, ViewCalculationConfiguration> calculationConfigurationsByName = null)
        {
            _name = name;
            _portfolioIdentifier = portfolioIdentifier;
            _user = user ?? UserPrincipal.DefaultUser;
            _resultModelDefinition = resultModelDefinition ?? new ResultModelDefinition();
            _defaultCurrency = defaultCurrency;
            _minDeltaCalcPeriod = minDeltaCalcPeriod;
            _maxDeltaCalcPeriod = maxDeltaCalcPeriod;
            _minFullCalcPeriod = minFullCalcPeriod;
            _maxFullCalcPeriod = maxFullCalcPeriod;
            _calculationConfigurationsByName = calculationConfigurationsByName ?? new Dictionary<string, ViewCalculationConfiguration>();
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public UniqueIdentifier PortfolioIdentifier
        {
            get { return _portfolioIdentifier; }
        }

        public UserPrincipal User
        {
            get { return _user; }
        }

        public ResultModelDefinition ResultModelDefinition
        {
            get { return _resultModelDefinition; }
        }

        public Currency DefaultCurrency
        {
            get { return _defaultCurrency; }
        }

        public long? MinDeltaCalcPeriod
        {
            get { return _minDeltaCalcPeriod; }
        }

        public long? MaxDeltaCalcPeriod
        {
            get { return _maxDeltaCalcPeriod; }
        }

        public long? MinFullCalcPeriod
        {
            get { return _minFullCalcPeriod; }
        }

        public long? MaxFullCalcPeriod
        {
            get { return _maxFullCalcPeriod; }
        }

        public Dictionary<string, ViewCalculationConfiguration> CalculationConfigurationsByName
        {
            get { return _calculationConfigurationsByName; }
        }

        public static ViewDefinition FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            var resultModelDefinition = deserializer.FromField<ResultModelDefinition>(ffc.GetByName("resultModelDefinition"));
            var portfolioIdentifier =ffc.GetAllByName("identifier").Any()  ? UniqueIdentifier.Parse(ffc.GetValue<String>("identifier")) : null;
            var user = deserializer.FromField<UserPrincipal>(ffc.GetByName("user"));

            var currency = ffc.GetByName("currency")==null ? null : Currency.Create(ffc.GetValue<string>("currency"));

            
            var minDeltaCalcPeriod = GetNullableLongField(ffc, "minDeltaCalcPeriod");
            var maxDeltaCalcPeriod = GetNullableLongField(ffc, "maxDeltaCalcPeriod");

            var minFullCalcPeriod = GetNullableLongField(ffc, "fullDeltaCalcPeriod");
            var maxFullCalcPeriod = GetNullableLongField(ffc, "maxFullCalcPeriod");



            var calculationConfigurationsByName = ffc.GetAllByName("calculationConfiguration")
                                                    .Select(deserializer.FromField<ViewCalculationConfiguration>)
                                                    .ToDictionary(vcc => vcc.Name);

            return new ViewDefinition(name, resultModelDefinition, portfolioIdentifier, user, currency, minDeltaCalcPeriod, maxDeltaCalcPeriod, minFullCalcPeriod, maxFullCalcPeriod, calculationConfigurationsByName);
        }

        /// <summary>
        /// TODO a nice nullable deserializer
        /// </summary>
        private static long? GetNullableLongField(IFudgeFieldContainer ffc, string name)
        {
            var field = ffc.GetByName(name);
            
            if (field == null) return null;

            long longValue = ffc.GetValue<long>(name);
            return (long?) longValue;
        }
        private static void WriteNullableLongField(IAppendingFudgeFieldContainer message, string name, long? value)
        {
            if (value.HasValue)
            {message.Add(name,value.Value);}
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer message, IFudgeSerializer s)
        {
            message.Add("name",Name);
            s.WriteInline(message,"identifier", PortfolioIdentifier);
            s.WriteInline(message, "user", User);
            s.WriteInline(message, "resultModelDefinition", ResultModelDefinition);

            if (DefaultCurrency != null)
            {
                message.Add("currency",DefaultCurrency.ISOCode);
            }

            WriteNullableLongField(message, "minDeltaCalcPeriod", MinDeltaCalcPeriod);
            WriteNullableLongField(message, "maxDeltaCalcPeriod", MaxDeltaCalcPeriod);
            WriteNullableLongField(message, "fullDeltaCalcPeriod", MinFullCalcPeriod);
            WriteNullableLongField(message, "maxFullCalcPeriod", MinFullCalcPeriod);


            foreach (var calcConfig in CalculationConfigurationsByName.Values)
            {
                FudgeMsg calcConfigMsg = s.Context.NewMessage();
                calcConfigMsg.Add("name", calcConfig.Name);
                foreach (var securityTypeRequirements in calcConfig.PortfolioRequirementsBySecurityType)
                {
                    FudgeMsg securityTypeRequirementsMsg = new FudgeMsg(s.Context);
                    securityTypeRequirementsMsg.Add("securityType", securityTypeRequirements.Key);
                    foreach (var requirement in securityTypeRequirements.Value.Properties)
                    {
                        foreach (var var in requirement.Value)
                        {
                            securityTypeRequirementsMsg.Add("portfolioRequirement", var);    
                        }
                        
                        // TODO put the value constraints into the message if they're specified
                    }

                    calcConfigMsg.Add("portfolioRequirementsBySecurityType", securityTypeRequirementsMsg);
                }
                foreach (var specificRequirement in calcConfig.SpecificRequirements)
                {
                    var sReqMsg = new FudgeSerializer(s.Context).SerializeToMsg(specificRequirement);
                    calcConfigMsg.Add("specificRequirement", sReqMsg);
                    
                }

                s.WriteInline(calcConfigMsg, "defaultProperties", calcConfig.DefaultProperties);
                
                //TODO delta defn
                calcConfigMsg.Add("deltaDefinition", new FudgeMsg());
                
                message.Add("calculationConfiguration",calcConfigMsg);
            }
            
        }

    }
}