// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public class ModuleSettingsManager
{
    [JsonProperty("string_settings")]
    public SerialisableBindableDictionary<string, string> StringSettings { get; } = new();

    [JsonProperty("int_settings")]
    public SerialisableBindableDictionary<string, int> IntSettings { get; } = new();

    [JsonProperty("bool_settings")]
    public SerialisableBindableDictionary<string, bool> BoolSettings { get; } = new();

    [JsonProperty("enum_settings")]
    // key : (enumName : enumValue)
    public SerialisableBindableDictionary<string, KeyValuePair<string, int>> EnumSettings { get; } = new();

    public void SetStringSetting(string key, string value)
    {
        if (!StringSettings.TryAdd(key, value))
            StringSettings[key] = value;
    }

    public void SetIntSetting(string key, int value)
    {
        if (!IntSettings.TryAdd(key, value))
            IntSettings[key] = value;
    }

    public void SetBoolSetting(string key, bool value)
    {
        if (!BoolSettings.TryAdd(key, value))
            BoolSettings[key] = value;
    }

    public void SetEnumSetting<T>(string key, T value) where T : Enum
    {
        var keyValuePair = new KeyValuePair<string, int>(typeof(T).Name, Convert.ToInt32(value));

        if (!EnumSettings.TryAdd(key, keyValuePair))
            EnumSettings[key] = keyValuePair;
    }

    public void CopyDataFrom(ModuleSettingsManager dataToCopy)
    {
        dataToCopy.StringSettings.ForEach(pair =>
        {
            if (StringSettings.ContainsKey(pair.Key))
                StringSettings[pair.Key] = pair.Value;
        });
        dataToCopy.IntSettings.ForEach(pair =>
        {
            if (IntSettings.ContainsKey(pair.Key))
                IntSettings[pair.Key] = pair.Value;
        });
        dataToCopy.BoolSettings.ForEach(pair =>
        {
            if (BoolSettings.ContainsKey(pair.Key))
                BoolSettings[pair.Key] = pair.Value;
        });
        dataToCopy.EnumSettings.ForEach(pair =>
        {
            try
            {
                TypeUtils.GetTypeByName(pair.Value.Key);
            }
            catch (InvalidOperationException)
            {
                // enum no longer exists due to an alteration to its module but is saved in the config
                // we ignore it to let it be removed
                return;
            }

            if (EnumSettings.ContainsKey(pair.Key))
                EnumSettings[pair.Key] = pair.Value;
        });
    }

    public int GetTotalSettingKeys()
    {
        return StringSettings.Count + IntSettings.Count + BoolSettings.Count + EnumSettings.Count;
    }
}
