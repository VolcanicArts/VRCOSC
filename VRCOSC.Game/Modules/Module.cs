// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Modules.Persistence;
using VRCOSC.Game.Modules.Serialisation.V1;
using VRCOSC.Game.Modules.World;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Serialisation;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class Module : IComparable<Module>
{
    private GameHost host = null!;
    private AppManager appManager = null!;
    private Scheduler scheduler = null!;
    private TerminalLogger terminal = null!;
    private ModuleDebugLogger moduleDebugLogger = null!;
    private VRChatOscClient oscClient => appManager.OSCClient;
    private readonly Bindable<ModuleState> state = new(ModuleState.Stopped);

    protected Player Player => appManager.VRChat.Player;
    protected OVRClient OVRClient => appManager.OVRClient;
    protected ChatBoxManager ChatBoxManager => appManager.ChatBoxManager;
    protected AvatarConfig? AvatarConfig => appManager.VRChat.AvatarConfig;

    internal readonly BindableBool Enabled = new();
    internal readonly Dictionary<string, ModuleAttribute> Settings = new();
    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();
    internal readonly Dictionary<ModulePersistentAttribute, PropertyInfo> PersistentProperies = new();
    internal readonly List<MethodInfo> ChatBoxUpdateMethods = new();

    internal string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    internal string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? "PLACEHOLDER";
    internal string LongDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.LongDescription ?? "PLACEHOLDER";
    internal string AuthorName => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.Name ?? "PLACEHOLDER";
    internal string? AuthorUrl => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.Url;
    internal string? AuthorIconUrl => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.IconUrl;
    internal ModuleType Group => GetType().GetCustomAttribute<ModuleGroupAttribute>()?.Type ?? ModuleType.General;
    internal string? PrefabName => GetType().GetCustomAttribute<ModulePrefabAttribute>()?.Name;
    internal string? PrefabUrl => GetType().GetCustomAttribute<ModulePrefabAttribute>()?.Url;
    internal IEnumerable<(string, string?)> InfoList => GetType().GetCustomAttributes<ModuleInfoAttribute>().Select(attribute => (attribute.Description, attribute.Url)).ToList();

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    protected virtual bool EnablePersistence => true;

    private string className => GetType().Name;
    internal string SerialisedName => className.ToLowerInvariant();
    internal string? LegacySerialisedName => GetType().GetCustomAttribute<ModuleLegacyAttribute>()?.LegacySerialisedName?.ToLowerInvariant();

    private readonly SerialisationManager persistenceSerialisationManager = new();
    private readonly SerialisationManager moduleSerialisationManager = new();

    internal void InjectDependencies(GameHost host, AppManager appManager, Scheduler scheduler, Storage storage)
    {
        this.host = host;
        this.appManager = appManager;
        this.scheduler = scheduler;

        moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, this));
        persistenceSerialisationManager.RegisterSerialiser(2, new ModulePersistenceSerialiser(storage, this));
    }

    internal void Load()
    {
        terminal = new TerminalLogger(Title);
        moduleDebugLogger = new ModuleDebugLogger(Title);

        CreateAttributes();
        Settings.Values.ForEach(setting => setting.Setup());
        Parameters.Values.ForEach(parameter => parameter.Setup());

        state.ValueChanged += _ => Log(state.Value.ToString());

        Deserialise();
        cachePersistentProperties();
    }

    #region Serialisation

    internal void Serialise()
    {
        moduleSerialisationManager.Serialise();
    }

    internal void Deserialise()
    {
        moduleSerialisationManager.Deserialise();
    }

    #endregion

    #region Persistence

    internal bool TryGetPersistentProperty(string key, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = PersistentProperies.SingleOrDefault(property => property.Key.LegacySerialisedName == key || property.Key.SerialisedName == key).Value;
        return property is not null;
    }

    private void cachePersistentProperties()
    {
        GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ForEach(info =>
        {
            var isDefined = info.IsDefined(typeof(ModulePersistentAttribute));
            if (!isDefined) return;

            if (!info.CanRead || !info.CanWrite) throw new InvalidOperationException($"Property '{info.Name}' must be declared with get/set to have persistence");

            PersistentProperies.Add(info.GetCustomAttribute<ModulePersistentAttribute>()!, info);
        });
    }

    private void loadPersistentProperties()
    {
        if (!PersistentProperies.Any() || !EnablePersistence) return;

        persistenceSerialisationManager.Deserialise();
    }

    private void savePersistentProperties()
    {
        if (!PersistentProperies.Any() || !EnablePersistence) return;

        persistenceSerialisationManager.Serialise();
    }

    #endregion

    #region Attributes

    protected virtual void CreateAttributes() { }

    protected void CreateSetting(Enum lookup, ModuleAttribute attribute) => Settings.Add(lookup.ToLookup(), attribute);

    protected void CreateSetting(Enum lookup, string displayName, string description, bool defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleBoolAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleIntAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleFloatAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonCallback, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringWithButtonAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            ButtonText = buttonText,
            ButtonCallback = buttonCallback
        });

    protected void CreateSetting<T>(Enum lookup, string displayName, string description, T defaultValue, Func<bool>? dependsOn = null) where T : Enum
        => Settings.Add(lookup.ToLookup(), new ModuleEnumAttribute<T>
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, IEnumerable<string> defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringListAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue.Select(v => new Bindable<string>(v)).ToList(),
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, List<MutableKeyValuePair> defaultValue, string keyPlaceholder, string valuePlaceholder, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new MutableKeyValuePairListAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            KeyPlaceholder = keyPlaceholder,
            ValuePlaceholder = valuePlaceholder
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, int minValue, int maxValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleIntRangeAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            Min = minValue,
            Max = maxValue
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, float minValue, float maxValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleFloatRangeAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            Min = minValue,
            Max = maxValue
        });

    internal bool TryGetSetting(string lookup, [NotNullWhen(true)] out ModuleAttribute? attribute)
    {
        attribute = Settings.SingleOrDefault(pair => pair.Key == lookup).Value;
        return attribute is not null;
    }

    internal bool TryGetParameter(string lookup, [NotNullWhen(true)] out ModuleAttribute? attribute)
    {
        attribute = Parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup).Value;
        return attribute is not null;
    }

    #endregion

    #region Events

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = parameterName.Replace(@"/", @"\/").Replace(@"*", @"(\S*)");
        return new Regex(pattern);
    }

    internal void Start()
    {
        state.Value = ModuleState.Starting;

        parameterNameEnum.Clear();
        Parameters.ForEach(pair => parameterNameEnum.Add(pair.Value.ParameterName, pair.Key));

        parameterNameRegex.Clear();
        Parameters.ForEach(pair => parameterNameRegex.Add(pair.Value.ParameterName, parameterToRegex(pair.Value.ParameterName)));

        loadPersistentProperties();

        try
        {
            OnModuleStart();
        }
        catch (Exception e)
        {
            PushException(e);
        }

        state.Value = ModuleState.Started;

        initialiseUpdateAttributes(GetType());
    }

    private void initialiseUpdateAttributes(Type? type)
    {
        if (type is null) return;

        initialiseUpdateAttributes(type.BaseType);

        type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ForEach(method =>
            {
                var updateAttribute = method.GetCustomAttribute<ModuleUpdateAttribute>();
                if (updateAttribute is null) return;

                if (updateAttribute.Mode == ModuleUpdateMode.ChatBox && GetType().IsSubclassOf(typeof(WorldModule)))
                    throw new InvalidOperationException($"Cannot have update mode {ModuleUpdateMode.ChatBox} on a world module");

                switch (updateAttribute.Mode)
                {
                    case ModuleUpdateMode.Custom:
                        scheduler.AddDelayed(() => UpdateMethod(method), updateAttribute.DeltaMilliseconds, true);
                        if (updateAttribute.UpdateImmediately) UpdateMethod(method);
                        break;

                    case ModuleUpdateMode.ChatBox:
                        ChatBoxUpdateMethods.Add(method);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
    }

    internal void Stop()
    {
        state.Value = ModuleState.Stopping;

        try
        {
            OnModuleStop();
        }
        catch (Exception e)
        {
            PushException(e);
        }

        savePersistentProperties();

        state.Value = ModuleState.Stopped;
    }

    protected void UpdateMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            PushException(new Exception($"{className} experienced an exception calling method {method.Name}", e));
        }
    }

    protected virtual void OnModuleStart() { }
    protected virtual void OnModuleStop() { }

    #endregion

    #region Settings

    protected List<T> GetSettingList<T>(Enum lookup)
    {
        var setting = Settings[lookup.ToLookup()];

        if (setting.GetType().IsSubclassOf(typeof(ModuleAttributePrimitiveList<T>)))
        {
            if (setting is ModuleAttributeList<Bindable<T>> settingList)
            {
                return settingList.Attribute.Select(bindable => bindable.Value).ToList();
            }
        }
        else
        {
            if (setting is ModuleAttributeList<T> settingList)
            {
                return settingList.Attribute.ToList();
            }
        }

        throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type List<'{typeof(T)}>'");
    }

    protected T GetSetting<T>(Enum lookup)
    {
        var setting = Settings[lookup.ToLookup()];

        if (!setting.IsValueType<T>())
            throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(T)}'");

        return ((ModuleAttribute<T>)setting).Value;
    }

    #endregion

    #region Parameters

    /// <summary>
    /// Allows for sending a parameter that hasn't been registed. Only use this when absolutely necessary
    /// </summary>
    protected void SendParameter<T>(string parameterName, T value) where T : struct
    {
        scheduler.Add(() => oscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/{parameterName}", value));
    }

    /// <summary>
    /// Sends a <paramref name="value"/> to an address denoted by <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to send</param>
    protected void SendParameter<T>(Enum lookup, T value) where T : struct
    {
        if (!Parameters.ContainsKey(lookup)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} has not been defined");

        var data = Parameters[lookup];
        if (!data.Mode.HasFlagFast(ParameterMode.Write)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} is a read-only parameter and therefore can't be sent!");
        if (data.ExpectedType != typeof(T)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} expects type {data.ExpectedType.ToReadableName()} but you tried to send {typeof(T).ToReadableName()}");

        scheduler.Add(() => oscClient.SendValue(data.ParameterAddress, value), false);
    }

    internal virtual void OnParameterReceived(VRChatOscMessage message)
    {
        var receivedParameter = new ReceivedParameter(message.ParameterName, message.ParameterValue);

        try
        {
            OnAnyParameterReceived(receivedParameter);
        }
        catch (Exception e)
        {
            PushException(e);
        }

        var parameterName = Parameters.Values.FirstOrDefault(moduleParameter => parameterNameRegex[moduleParameter.ParameterName].IsMatch(receivedParameter.Name))?.ParameterName;
        if (parameterName is null) return;

        if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = Parameters[lookup];

        if (!parameterData.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!receivedParameter.IsValueType(parameterData.ExpectedType))
        {
            Log($@"Cannot accept input parameter. `{lookup}` expects type `{parameterData.ExpectedType}` but received type `{receivedParameter.Value.GetType()}`");
            return;
        }

        var registeredParameter = new RegisteredParameter(receivedParameter, lookup, parameterData);

        try
        {
            ModuleParameterReceived(registeredParameter);
        }
        catch (Exception e)
        {
            PushException(e);
        }
    }

    protected virtual void OnAnyParameterReceived(ReceivedParameter parameter) { }
    private protected abstract void ModuleParameterReceived(RegisteredParameter parameter);

    #endregion

    #region Extensions

    /// <summary>
    /// Logs to the terminal in the run screen
    /// </summary>
    protected void Log(string message) => terminal.Log(message);

    /// <summary>
    /// Logs to a special module-debug file when the --module-debug flag is passed on startup
    /// </summary>
    protected void LogDebug(string message) => moduleDebugLogger.Log(message);

    /// <summary>
    /// Opens a given <paramref name="url"/> in the user's chosen browser
    /// </summary>
    protected void OpenUrlExternally(string url) => host.OpenUrlExternally(url);

    /// <summary>
    /// Maps a value <paramref name="source"/> from a source range to a destination range
    /// </summary>
    protected static float Map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));

    #endregion

    #region Internal

    protected internal void PushException(Exception e)
    {
        Notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
        Logger.Error(e, $"{className} experienced an exception");
    }

    public int CompareTo(Module? other)
    {
        if (other is null) return 1;
        if (Group > other.Group) return 1;
        if (Group < other.Group) return -1;

        return string.CompareOrdinal(Title, other.Title);
    }

    #endregion
}
