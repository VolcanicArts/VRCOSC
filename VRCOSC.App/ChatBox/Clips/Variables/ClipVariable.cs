// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using Newtonsoft.Json;
using VRCOSC.App.Modules;
using VRCOSC.App.Pages.ChatBox.Options;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips.Variables;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ClipVariable
{
    public string ModuleID { get; } = null!;
    public string VariableID { get; } = null!;

    public string DisplayName => ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.DisplayName.Value;
    public string DisplayNameWithModule => $"{ModuleManager.GetInstance().GetModuleOfID(ModuleID).Title} - {DisplayName}";

    public List<RenderableClipVariableOption> UIOptions
    {
        get
        {
            var optionsList = new List<RenderableClipVariableOption>();

            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ForEach(propertyInfo =>
            {
                if (!propertyInfo.IsDefined(typeof(ClipVariableOptionAttribute))) return;

                if (!propertyInfo.CanRead || !propertyInfo.CanWrite) throw new InvalidOperationException($"Property '{propertyInfo.Name}' must be declared with get/set to be used as a variable option");

                var displayName = propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>()!.DisplayName;
                var description = propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>()!.Description;

                // TODO: Move this inside the ClipVariableOptionAttribute?
                Page? pageInstance = null;

                if (propertyInfo.PropertyType == typeof(bool))
                {
                    pageInstance = new ToggleVariableOptionPage(this, propertyInfo);
                }
                else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(string))
                {
                    pageInstance = new TextBoxVariableOptionPage(this, propertyInfo);
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    pageInstance = new DropdownVariableOptionPage(this, propertyInfo);
                }

                var renderableClipVariableOption = new RenderableClipVariableOption(propertyInfo, displayName, description, pageInstance);
                optionsList.Add(renderableClipVariableOption);
            });

            return optionsList;
        }
    }

    [JsonConstructor]
    internal ClipVariable()
    {
    }

    protected ClipVariable(ClipVariableReference reference)
    {
        ModuleID = reference.ModuleID;
        VariableID = reference.VariableID;
    }

    /// <summary>
    /// Called when the modules start. Good for resetting properties between module restarts
    /// </summary>
    public virtual void Start()
    {
        currentIndex = 0;
    }

    [ClipVariableOption("case_mode", "Case Mode", "Should the final string be made upper or lowercase, or not be changed?")]
    public ClipVariableCaseMode CaseMode { get; set; } = ClipVariableCaseMode.Default;

    [ClipVariableOption("truncate_length", "Truncate Length", "What's the longest length this variable can be?\nSet to -1 for unlimited length")]
    public int TruncateLength { get; set; } = -1;

    [ClipVariableOption("include_ellipses", "Include Ellipses", "When truncating, should we include an ellipses (...) at the end?")]
    public bool IncludeEllipses { get; set; }

    [ClipVariableOption("scroll_direction", "Scroll Direction", "What direction should this variable scroll in?\nSet Scroll Speed to 0 for no scrolling")]
    public ClipVariableScrollDirection ScrollDirection { get; set; } = ClipVariableScrollDirection.Left;

    [ClipVariableOption("scroll_speed", "Scroll Speed", "How fast, in terms of characters per ChatBox update, should this variable scroll?")]
    public int ScrollSpeed { get; set; }

    [ClipVariableOption("join_string", "Join String", "When scrolling, what string should join the ends of the variable?")]
    public string JoinString { get; set; } = string.Empty;

    private int currentIndex;

    public string GetFormattedValue()
    {
        var variableValue = ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.Value.Value;
        var formattedValue = variableValue is not null ? Format(variableValue) : string.Empty;

        if (string.IsNullOrEmpty(formattedValue)) return string.Empty;

        if (!string.IsNullOrEmpty(JoinString)) formattedValue += JoinString;

        var position = currentIndex.Modulo(formattedValue.Length);
        formattedValue = cropAndWrapText(formattedValue, position, TruncateLength == -1 ? int.MaxValue : TruncateLength);

        if (IncludeEllipses)
            formattedValue += "...";

        switch (ScrollDirection)
        {
            case ClipVariableScrollDirection.Right:
                currentIndex += ScrollSpeed;
                break;

            case ClipVariableScrollDirection.Left:
                currentIndex -= ScrollSpeed;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        formattedValue = CaseMode switch
        {
            ClipVariableCaseMode.Default => formattedValue,
            ClipVariableCaseMode.Lower => formattedValue.ToLower(CultureInfo.CurrentCulture),
            ClipVariableCaseMode.Upper => formattedValue.ToUpper(CultureInfo.CurrentCulture),
            _ => throw new ArgumentOutOfRangeException()
        };

        return formattedValue;
    }

    private static string cropAndWrapText(string text, int position, int maxLength)
    {
        var endPos = Math.Min(position + maxLength, text.Length);
        var subText = text.Substring(position, endPos - position);
        return endPos != text.Length ? subText : subText + text[..Math.Min(maxLength - subText.Length, position)];
    }

    protected abstract string Format(object value);
}

public enum ClipVariableScrollDirection
{
    Left,
    Right

    // Bounce
}

public enum ClipVariableCaseMode
{
    Default,
    Lower,
    Upper
}

public class RenderableClipVariableOption
{
    public PropertyInfo PropertyInfo { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Page? PageInstance { get; }

    public RenderableClipVariableOption(PropertyInfo propertyInfo, string displayName, string description, Page? pageInstance)
    {
        PropertyInfo = propertyInfo;
        DisplayName = displayName;
        Description = description;
        PageInstance = pageInstance;
    }
}
