// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules.Serialisation.Legacy;

public class LegacyModuleSerialiser
{
    private const string directory_name = "modules";
    private readonly Storage storage;

    public LegacyModuleSerialiser(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory(directory_name);
    }

    public void Deserialise(Module module)
    {
        using (var stream = storage.GetStream(module.FileName))
        {
            if (stream is not null)
            {
                using var reader = new StreamReader(stream);

                while (reader.ReadLine() is { } line)
                {
                    switch (line)
                    {
                        case "#InternalSettings":
                            performInternalSettingsLoad(reader, module);
                            break;

                        case "#Settings":
                            performSettingsLoad(reader, module);
                            break;

                        case "#Parameters":
                            performParametersLoad(reader, module);
                            break;
                    }
                }
            }
        }

        Serialise(module);
    }

    private static void performInternalSettingsLoad(TextReader reader, Module module)
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
                    module.Enabled.Value = bool.Parse(value);
                    break;
            }
        }
    }

    private static void performSettingsLoad(TextReader reader, Module module)
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

            if (!module.Settings.ContainsKey(lookup)) continue;

            var setting = module.Settings[lookup];

            var readableTypeName = setting.Attribute.Value.GetType().ToReadableName().ToLowerInvariant();
            if (!readableTypeName.Equals(typeStr)) continue;

            switch (typeStr)
            {
                case "enum":
                    var typeAndValue = value.Split(new[] { '#' }, 2);
                    var enumName = typeAndValue[0].Split('+')[1];
                    var enumType = enumNameToType(enumName);
                    if (enumType is not null) setting.Attribute.Value = Enum.ToObject(enumType, int.Parse(typeAndValue[1]));
                    break;

                case "string":
                    setting.Attribute.Value = value;
                    break;

                case "int":
                    setting.Attribute.Value = int.Parse(value);
                    break;

                case "float":
                    setting.Attribute.Value = float.Parse(value);
                    break;

                case "bool":
                    setting.Attribute.Value = bool.Parse(value);
                    break;

                default:
                    Logger.Log($"Unknown type found in file: {typeStr}");
                    break;
            }
        }
    }

    private static void performParametersLoad(TextReader reader, Module module)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplit = line.Split(new[] { '=' }, 2);
            var lookup = lineSplit[0];
            var value = lineSplit[1];

            if (!module.ParametersLookup.ContainsKey(lookup)) continue;

            var parameter = module.Parameters[module.ParametersLookup[lookup]];
            parameter.Attribute.Value = value;
        }
    }

    private static Type? enumNameToType(string enumName)
    {
        Type? returnType = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                assembly.GetTypes().ForEach(type =>
                {
                    if (!type.IsSubclassOf(typeof(Enum))) return;

                    if (type.Name.Equals(enumName, StringComparison.Ordinal)) returnType = type;
                });
            }
            catch { }
        }

        return returnType;
    }

    public void Serialise(Module module)
    {
        using var stream = storage.CreateFileSafely(module.FileName);
        using var writer = new StreamWriter(stream);

        performInternalSettingsSave(writer, module);
        performSettingsSave(writer, module);
        performParametersSave(writer, module);
    }

    private static void performInternalSettingsSave(TextWriter writer, Module module)
    {
        writer.WriteLine(@"#InternalSettings");
        writer.WriteLine(@"{0}={1}", "enabled", module.Enabled.Value.ToString());
        writer.WriteLine(@"#End");
    }

    private static void performSettingsSave(TextWriter writer, Module module)
    {
        var areAllDefault = module.Settings.All(pair => pair.Value.Attribute.IsDefault);
        if (areAllDefault) return;

        writer.WriteLine(@"#Settings");

        foreach (var (lookup, moduleAttributeData) in module.Settings)
        {
            if (moduleAttributeData.Attribute.IsDefault) continue;

            var value = moduleAttributeData.Attribute.Value;
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
        }

        writer.WriteLine(@"#End");
    }

    private static void performParametersSave(TextWriter writer, Module module)
    {
        var areAllDefault = module.Parameters.All(pair => pair.Value.Attribute.IsDefault);
        if (areAllDefault) return;

        writer.WriteLine(@"#Parameters");

        foreach (var (lookup, parameterAttribute) in module.Parameters)
        {
            if (parameterAttribute.Attribute.IsDefault) continue;

            var value = parameterAttribute.Attribute.Value;
            writer.WriteLine(@"{0}={1}", lookup.ToLookup(), value);
        }

        writer.WriteLine(@"#End");
    }
}
