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
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules;

public abstract class Module : IComparable<Module>
{
    private GameHost host = null!;
    private AppManager appManager = null!;
    private Scheduler scheduler = null!;
    private TerminalLogger terminal = null!;
    private ModuleDebugLogger moduleDebugLogger = null!;
    private NotificationContainer notifications = null!;
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

    internal string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    internal string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? "PLACEHOLDER";
    internal string LongDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.LongDescription ?? "PLACEHOLDER";
    internal string AuthorName => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.Name ?? "PLACEHOLDER";
    internal string? AuthorUrl => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.Url;
    internal string? AuthorIconUrl => GetType().GetCustomAttribute<ModuleAuthorAttribute>()?.IconUrl;
    internal ModuleType Group => GetType().GetCustomAttribute<ModuleGroupAttribute>()?.Type ?? ModuleType.General;
    internal string? PrefabName => GetType().GetCustomAttribute<ModulePrefabAttribute>()?.Name;
    internal string? PrefabUrl => GetType().GetCustomAttribute<ModulePrefabAttribute>()?.Url;
    internal IEnumerable<string> InfoList => GetType().GetCustomAttributes<ModuleInfoAttribute>().Select(attribute => attribute.Description).ToList();

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    protected virtual bool EnablePersistence => true;

    private string className => GetType().Name;
    internal string SerialisedName => className.ToLowerInvariant();

    private readonly SerialisationManager persistenceSerialisationManager = new();
    private readonly SerialisationManager moduleSerialisationManager = new();

    protected const double FIXED_UPDATE_DELTA = VRChatOscConstants.UPDATE_DELTA_MILLISECONDS;

    internal void InjectDependencies(GameHost host, AppManager appManager, Scheduler scheduler, Storage storage, NotificationContainer notifications)
    {
        this.host = host;
        this.appManager = appManager;
        this.scheduler = scheduler;
        this.notifications = notifications;

        moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, notifications, this));
        persistenceSerialisationManager.RegisterSerialiser(2, new ModulePersistenceSerialiser(storage, notifications, this));
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

    public void Serialise()
    {
        moduleSerialisationManager.Serialise();
    }

    public void Deserialise()
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

    protected void CreateParameter<T>(Enum lookup, ParameterMode mode, string parameterName, string displayName, string description)
        => Parameters.Add(lookup, new ModuleParameter
        {
            Name = displayName,
            Description = description,
            Default = parameterName,
            DependsOn = null,
            Mode = mode,
            ExpectedType = typeof(T)
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
            pushException(e);
        }

        state.Value = ModuleState.Started;

        scheduler.AddDelayed(fixedUpdate, FIXED_UPDATE_DELTA, true);

        GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.GetCustomAttribute<ModuleUpdateAttribute>()?.Mode == ModuleUpdateMode.ChatBox)
            .ForEach(method =>
            {
                scheduler.AddDelayed(() => update(method), appManager.ChatBoxManager.SendDelay.Value, true);
                if (method.GetCustomAttribute<ModuleUpdateAttribute>()!.UpdateImmediately) update(method);
            });

        GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.GetCustomAttribute<ModuleUpdateAttribute>()?.Mode == ModuleUpdateMode.Custom)
            .ForEach(method =>
            {
                scheduler.AddDelayed(() => update(method), method.GetCustomAttribute<ModuleUpdateAttribute>()!.DeltaMilliseconds, true);
                if (method.GetCustomAttribute<ModuleUpdateAttribute>()!.UpdateImmediately) update(method);
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
            pushException(e);
        }

        savePersistentProperties();

        state.Value = ModuleState.Stopped;
    }

    internal void PlayerUpdate()
    {
        try
        {
            OnPlayerUpdate();
        }
        catch (Exception e)
        {
            pushException(e);
        }
    }

    private void update(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            pushException(new Exception($"{className} experienced an exception calling method {method.Name}", e));
        }
    }

    private void fixedUpdate()
    {
        try
        {
            GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttribute<ModuleUpdateAttribute>()?.Mode == ModuleUpdateMode.Fixed)
                .ForEach(method => method.Invoke(this, null));
        }
        catch (Exception e)
        {
            pushException(e);
        }
    }

    private void avatarChange()
    {
        try
        {
            OnAvatarChange();
        }
        catch (Exception e)
        {
            pushException(e);
        }
    }

    protected virtual void OnModuleStart() { }
    protected virtual void OnModuleStop() { }
    protected virtual void OnAvatarChange() { }
    protected virtual void OnPlayerUpdate() { }

    protected virtual void OnAnyParameterReceived(AvatarParameter parameter) { }
    protected virtual void OnModuleParameterReceived(AvatarParameter parameter) { }

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
    /// Sends a <paramref name="value"/> to an address denoted by <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to send</param>
    protected void SendParameter<T>(Enum lookup, T value) where T : struct
    {
        if (!Parameters.ContainsKey(lookup)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} has not been defined");

        var data = Parameters[lookup];
        if (!data.Mode.HasFlagFast(ParameterMode.Write)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} is a read-only parameter and therefore can't be sent!");

        scheduler.Add(() => oscClient.SendValue(data.ParameterAddress, value));
    }

    internal void OnParameterReceived(VRChatOscData data)
    {
        if (data.IsAvatarChangeEvent)
        {
            avatarChange();
            return;
        }

        if (!data.IsAvatarParameter) return;

        try
        {
            OnAnyParameterReceived(new AvatarParameter(null, data.ParameterName, data.ParameterValue));
        }
        catch (Exception e)
        {
            pushException(e);
        }

        var parameterName = Parameters.Values.FirstOrDefault(moduleParameter => parameterNameRegex[moduleParameter.ParameterName].IsMatch(data.ParameterName))?.ParameterName;
        if (parameterName is null) return;

        if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = Parameters[lookup];

        if (!parameterData.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!data.IsValueType(parameterData.ExpectedType))
        {
            Log($@"Cannot accept input parameter. `{lookup}` expects type `{parameterData.ExpectedType}` but received type `{data.ParameterValue.GetType()}`");
            return;
        }

        try
        {
            OnModuleParameterReceived(new AvatarParameter(parameterData, lookup, data.ParameterName, data.ParameterValue));
        }
        catch (Exception e)
        {
            pushException(e);
        }
    }

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

    private void pushException(Exception e)
    {
        notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
        Logger.Error(e, $"{className} experienced an exception");
    }

    internal enum ModuleState
    {
        Starting,
        Started,
        Stopping,
        Stopped
    }

    public enum ModuleType
    {
        Health,
        Accessibility,
        OpenVR,
        Integrations,
        General,
        NSFW
    }

    public int CompareTo(Module? other)
    {
        if (other is null) return 1;
        if (Group > other.Group) return 1;
        if (Group < other.Group) return -1;

        return string.CompareOrdinal(Title, other.Title);
    }
}
