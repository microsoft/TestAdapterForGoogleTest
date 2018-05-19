﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using GoogleTestAdapter.Settings;
using System.Collections.Generic;

namespace GoogleTestAdapter.TestAdapter.Settings
{
    public class CTestPropertySettings : ICTestPropertySettings
    {
        public IDictionary<string, string> Environment { get; private set; }

        public string WorkingDirectory { get; private set; }

        public CTestPropertySettings(CTestPropertySettingsContainer.TestProperties test)
        {
            var environment = new Dictionary<string, string>();
            foreach (var envVar in test.Environment)
            {
                environment.Add(envVar.Name, envVar.Value);
            }

            this.Environment = environment;
            this.WorkingDirectory = test.WorkingDirectory;
        }
    }
}
