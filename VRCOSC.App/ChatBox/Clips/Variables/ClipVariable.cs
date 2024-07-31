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
    public string? ModuleID { get; }
    public string VariableID { get; } = null!;

    public string DisplayName => ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.DisplayName.Value;

    public string DisplayNameWithModule
    {
        get
        {
            var moduleName = ModuleID is null ? "Built-In" : ModuleManager.GetInstance().GetModuleOfID(ModuleID).Title;
            return $"{moduleName} - {DisplayName}";
        }
    }

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
        bounceDirection = true;
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

    [ClipVariableOption("only_scroll_when_truncated", "Only Scroll When Truncated", "Should we only scroll when we've needed to truncate?")]
    public bool OnlyScrollWhenTruncated { get; set; }

    private int currentIndex;
    private bool bounceDirection;

    public virtual bool IsDefault() => CaseMode == ClipVariableCaseMode.Default &&
                                       TruncateLength == -1 &&
                                       IncludeEllipses == false &&
                                       ScrollDirection == ClipVariableScrollDirection.Left &&
                                       ScrollSpeed == 0 &&
                                       JoinString == string.Empty &&
                                       OnlyScrollWhenTruncated == false;

    public string GetFormattedValue()
    {
        var variableValue = ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.Value.Value;
        var formattedValue = variableValue is not null ? Format(variableValue) : string.Empty;

        if (string.IsNullOrEmpty(formattedValue)) return string.Empty;

        formattedValue = formattedValue.Trim();

        var willTruncate = TruncateLength >= 0 && formattedValue.Length > TruncateLength;
        var willScroll = !OnlyScrollWhenTruncated || willTruncate;

        if (!willScroll) currentIndex = 0;

        if (!string.IsNullOrEmpty(JoinString) && willScroll && ScrollDirection != ClipVariableScrollDirection.Bounce) formattedValue += JoinString;

        var stringInfo = new StringInfo(formattedValue);

        var position = currentIndex.Modulo(stringInfo.LengthInTextElements);
        formattedValue = cropAndWrapText(stringInfo, position, TruncateLength < 0 ? stringInfo.LengthInTextElements : TruncateLength, ScrollDirection != ClipVariableScrollDirection.Bounce);

        if (IncludeEllipses && willTruncate) formattedValue += "...";

        var finalStringInfo = new StringInfo(formattedValue);

        if (willScroll && ScrollSpeed > 0)
        {
            switch (ScrollDirection)
            {
                case ClipVariableScrollDirection.Right:
                    currentIndex += ScrollSpeed;
                    break;

                case ClipVariableScrollDirection.Left:
                    currentIndex -= ScrollSpeed;
                    break;

                case ClipVariableScrollDirection.Bounce:
                    var localScrollSpeed = ScrollSpeed;
                    var charsLeft = bounceDirection ? finalStringInfo.LengthInTextElements - currentIndex : currentIndex;

                    var willSwitch = false;

                    if (charsLeft < ScrollSpeed)
                    {
                        localScrollSpeed = charsLeft;
                        willSwitch = true;
                    }

                    currentIndex += bounceDirection ? localScrollSpeed : -localScrollSpeed;

                    if (willSwitch) bounceDirection = !bounceDirection;

                    break;
            }
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

    private static string cropAndWrapText(StringInfo text, int position, int maxLength, bool wrap)
    {
        var endPos = Math.Min(position + maxLength, text.LengthInTextElements);
        var subText = text.SubstringByTextElements(position, endPos - position);

        if (wrap)
            return endPos != text.LengthInTextElements ? subText : subText + text.SubstringByTextElements(0, Math.Min(maxLength - subText.Length, position));

        return subText;
    }

    protected abstract string Format(object value);
}

public enum ClipVariableScrollDirection
{
    Left,
    Right,
    Bounce
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
