// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using GoogleTestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GoogleTestAdapter.TestAdapter.Settings
{
    [XmlRoot(GoogleTestConstants.CTestPropertySettingsName)]
    public class CTestPropertySettingsContainer : TestRunSettings, ICTestPropertySettingsContainer
    {
        public class EnvVar
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class TestProperties
        {
            public string Name { get; set; }
            public string Command { get; set; }
            public List<EnvVar> Environment { get; set; }
            public string WorkingDirectory { get; set; }
        }

        private IDictionary<string, ICTestPropertySettings> _tests;

        public CTestPropertySettingsContainer()
            : base(GoogleTestConstants.CTestPropertySettingsName)
        {
        }

        public List<TestProperties> Tests { get; set; }

        public override XmlElement ToXml()
        {
            var document = new XmlDocument();
            using (XmlWriter writer = document.CreateNavigator().AppendChild())
            {
                new XmlSerializer(typeof(RunSettingsContainer))
                    .Serialize(writer, this);
            }
            return document.DocumentElement;
        }

        public bool TryGetSettings(string testName, out ICTestPropertySettings settings)
        {
            EnsureTestPropertiesMap();
            return _tests.TryGetValue(testName, out settings);
        }

        private void EnsureTestPropertiesMap()
        {
            if (_tests != null)
            {
                return;
            }

            _tests = new Dictionary<string, ICTestPropertySettings>();
            if (this.Tests != null)
            {
                foreach (var t in this.Tests)
                {
                    var propertySettings = new CTestPropertySettings(t);
                    _tests.Add(t.Name, propertySettings);
                }
            }
        }
    }
}
