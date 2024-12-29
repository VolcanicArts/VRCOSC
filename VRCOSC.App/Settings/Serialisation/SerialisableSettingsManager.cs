﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Settings.Serialisation;

public class SerialisableSettingsManager : SerialisableVersion
{
    [JsonProperty("settings")]
    public Dictionary<string, object> Settings = new();

    [JsonProperty("metadata")]
    public Dictionary<string, object> Metadata = new();

    [JsonConstructor]
    public SerialisableSettingsManager()
    {
    }

    public SerialisableSettingsManager(SettingsManager settingsManager)
    {
        Version = 1;

        settingsManager.Settings.ForEach(pair => Settings.Add(pair.Key.ToString(), pair.Value.GetValue()));
        settingsManager.Metadata.ForEach(pair => Metadata.Add(pair.Key.ToString(), pair.Value.GetValue()));
    }
}