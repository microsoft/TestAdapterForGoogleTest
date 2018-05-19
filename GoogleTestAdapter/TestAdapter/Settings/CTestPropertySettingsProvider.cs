// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GoogleTestAdapter.TestAdapter.Settings
{
    [Export(typeof(ISettingsProvider))]
    [SettingsName(GoogleTestConstants.CTestPropertySettingsName)]
    public class CTestPropertySettingsProvider : ISettingsProvider
    {
        public string Name => GoogleTestConstants.CTestPropertySettingsName;

        public CTestPropertySettingsContainer CTestProperySettings { get; set; }

        public void Load(XmlReader reader)
        {
            ValidateArg.NotNull(reader, nameof(reader));

            var schemaSet = new XmlSchemaSet();
            var schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CTestPropertySettings.xsd");
            schemaSet.Add(null, XmlReader.Create(schemaStream));

            var settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
            };

            settings.ValidationEventHandler += (object o, ValidationEventArgs e) => throw e.Exception;

            using (var newReader = XmlReader.Create(reader, settings))
            {
                try
                {
                    if (newReader.Read() && newReader.Name.Equals(this.Name))
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(CTestPropertySettingsContainer));
                        this.CTestProperySettings = deserializer.Deserialize(newReader) as CTestPropertySettingsContainer;
                    }
                }
                catch (InvalidOperationException e) when (e.InnerException is XmlSchemaValidationException)
                {
                    throw new InvalidRunSettingsException(
                        String.Format(Resources.Invalid, GoogleTestConstants.CTestPropertySettingsName),
                        e.InnerException);
                }
            }
        }
    }
}
