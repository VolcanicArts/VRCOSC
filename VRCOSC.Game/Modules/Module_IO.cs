// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules;

public partial class Module
{
    #region Loading

    private void performLoad()
    {
        using (var stream = Storage.GetStream(FileName))
        {
            if (stream is not null)
            {
                using var reader = new StreamReader(stream);

                while (reader.ReadLine() is { } line)
                {
                    switch (line)
                    {
                        case "#InternalSettings":
                            performInternalSettingsLoad(reader);
                            break;

                        case "#Settings":
                            performSettingsLoad(reader);
                            break;
                    }
                }
            }
        }

        executeAfterLoad();
    }

    private void performInternalSettingsLoad(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplit = line.Split(new[] { '=' }, 2);
            var lookup = lineSplit[0];
            var value = lineSplit[1];

            switch (lookup)
            {
                case "enabled":
                    Enabled.Value = bool.Parse(value);
                    break;
            }
        }
    }

    private void performSettingsLoad(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplitLookupValue = line.Split(new[] { '=' }, 2);
            var lookupType = lineSplitLookupValue[0].Split(new[] { ':' }, 2);
            var value = lineSplitLookupValue[1];

            var lookupStr = lookupType[0];
            var typeStr = lookupType[1];

            var lookup = lookupStr;
            if (lookupStr.Contains('#')) lookup = lookupStr.Split(new[] { '#' }, 2)[0];

            if (!Settings.ContainsKey(lookup)) continue;

            var setting = Settings[lookup];

            switch (setting)
            {
                case ModuleAttributeSingle settingSingle:
                {
                    var readableTypeName = settingSingle.Attribute.Value.GetType().ToReadableName().ToLowerInvariant();
                    if (!readableTypeName.Equals(typeStr)) continue;

                    switch (typeStr)
                    {
                        case "enum":
                            var typeAndValue = value.Split(new[] { '#' }, 2);
                            var enumName = typeAndValue[0].Split('+')[1];
                            var enumType = enumNameToType(enumName);
                            if (enumType is not null) settingSingle.Attribute.Value = Enum.ToObject(enumType, int.Parse(typeAndValue[1]));
                            break;

                        case "string":
                            settingSingle.Attribute.Value = value;
                            break;

                        case "int":
                            settingSingle.Attribute.Value = int.Parse(value);
                            break;

                        case "float":
                            settingSingle.Attribute.Value = float.Parse(value);
                            break;

                        case "bool":
                            settingSingle.Attribute.Value = bool.Parse(value);
                            break;

                        default:
                            Logger.Log($"Unknown type found in file: {typeStr}");
                            break;
                    }

                    break;
                }

                case ModuleAttributeList settingList:
                {
                    if (value == "EMPTY" && settingList.CanBeEmpty)
                    {
                        settingList.AttributeList.Clear();
                        continue;
                    }

                    var readableTypeName = settingList.AttributeList.First().Value.GetType().ToReadableName().ToLowerInvariant();
                    if (!readableTypeName.Equals(typeStr)) continue;

                    var index = int.Parse(lookupStr.Split(new[] { '#' }, 2)[1]);

                    switch (typeStr)
                    {
                        case "string":
                            settingList.AddAt(index, new Bindable<object>(value));
                            break;

                        case "int":
                            settingList.AddAt(index, new Bindable<object>(int.Parse(value)));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(value), value, $"Unknown type found in file for {nameof(ModuleAttributeList)}: {value.GetType()}");
                    }

                    break;
                }
            }
        }
    }

    private void executeAfterLoad()
    {
        performSave();
        Enabled.BindValueChanged(_ => performSave());
    }

    private static Type? enumNameToType(string enumName)
    {
        Type? returnType = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            assembly.GetTypes().ForEach(type =>
            {
                if (type.Name.Equals(enumName, StringComparison.Ordinal)) returnType = type;
            });
        }

        return returnType;
    }

    #endregion

    #region Saving

    public void Save()
    {
        performSave();
    }

    private void performSave()
    {
        using var stream = Storage.CreateFileSafely(FileName);
        using var writer = new StreamWriter(stream);

        performInternalSettingsSave(writer);
        performSettingsSave(writer);
    }

    private void performInternalSettingsSave(TextWriter writer)
    {
        writer.WriteLine(@"#InternalSettings");
        writer.WriteLine(@"{0}={1}", "enabled", Enabled.Value.ToString());
        writer.WriteLine(@"#End");
    }

    private void performSettingsSave(TextWriter writer)
    {
        var areAllDefault = Settings.All(pair => pair.Value.IsDefault());
        if (areAllDefault) return;

        writer.WriteLine(@"#Settings");

        foreach (var (lookup, moduleAttributeData) in Settings)
        {
            if (moduleAttributeData.IsDefault()) continue;

            switch (moduleAttributeData)
            {
                case ModuleAttributeSingle moduleAttributeSingle:
                {
                    var value = moduleAttributeSingle.Attribute.Value;
                    var valueType = value.GetType();
                    var readableTypeName = valueType.ToReadableName().ToLowerInvariant();

                    if (valueType.IsSubclassOf(typeof(Enum)))
                    {
                        var enumClass = valueType.FullName;
                        writer.WriteLine(@"{0}:{1}={2}#{3}", lookup, readableTypeName, enumClass, (int)value);
                    }
                    else
                    {
                        writer.WriteLine(@"{0}:{1}={2}", lookup, readableTypeName, value);
                    }

                    break;
                }

                case ModuleAttributeList moduleAttributeList:
                {
                    var values = moduleAttributeList.AttributeList.ToList();

                    if (!values.Any())
                    {
                        writer.WriteLine(@"{0}:EMPTY=EMPTY", lookup);
                    }
                    else
                    {
                        var valueType = values.First().Value.GetType();
                        var readableTypeName = valueType.ToReadableName().ToLowerInvariant();

                        for (int i = 0; i < values.Count; i++)
                        {
                            writer.WriteLine(@"{0}#{1}:{2}={3}", lookup, i, readableTypeName, values[i].Value);
                        }
                    }

                    break;
                }
            }
        }

        writer.WriteLine(@"#End");
    }

    #endregion
}
