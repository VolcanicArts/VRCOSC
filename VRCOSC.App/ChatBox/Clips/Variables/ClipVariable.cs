// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using Newtonsoft.Json;
using VRCOSC.App.Modules;
using VRCOSC.App.UI.Views.ChatBox.Variables;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips.Variables;

public abstract class ClipVariable
{
    public string? ModuleID { get; private set; }
    public string VariableID { get; private set; } = null!;

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

                UserControl? viewInstance = null;

                if (propertyInfo.PropertyType == typeof(bool))
                {
                    viewInstance = new ToggleVariableOptionView(this, propertyInfo);
                }
                else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(string))
                {
                    viewInstance = new TextBoxVariableOptionView(this, propertyInfo);
                }
                else if (propertyInfo.PropertyType.IsEnum)
                {
                    viewInstance = new DropdownVariableOptionView(this, propertyInfo);
                }

                var renderableClipVariableOption = new RenderableClipVariableOption(propertyInfo, displayName, description, viewInstance);
                optionsList.Add(renderableClipVariableOption);
            });

            return optionsList;
        }
    }

    [JsonConstructor]
    protected ClipVariable()
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
        previousScrollDirection = ScrollDirection;
        previousOnlyScrollWhenTruncated = OnlyScrollWhenTruncated;
    }

    public virtual ClipVariable Clone()
    {
        var clone = (ClipVariable)Activator.CreateInstance(GetType())!;

        clone.ModuleID = ModuleID;
        clone.VariableID = VariableID;
        clone.CaseMode = CaseMode;
        clone.TruncateLength = TruncateLength;
        clone.IncludeEllipses = IncludeEllipses;
        clone.ScrollDirection = ScrollDirection;
        clone.ScrollSpeed = ScrollSpeed;
        clone.JoinString = JoinString;
        clone.OnlyScrollWhenTruncated = OnlyScrollWhenTruncated;

        return clone;
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
    private ClipVariableScrollDirection previousScrollDirection;
    private bool previousOnlyScrollWhenTruncated;

    public virtual bool IsDefault() => CaseMode == ClipVariableCaseMode.Default &&
                                       TruncateLength == -1 &&
                                       IncludeEllipses == false &&
                                       ScrollDirection == ClipVariableScrollDirection.Left &&
                                       ScrollSpeed == 0 &&
                                       JoinString == string.Empty &&
                                       OnlyScrollWhenTruncated == false;

    // I hate all of this code. For the love of god redo this properly at some point so that it doesn't SUCK

    public string GetFormattedValue()
    {
        if (previousScrollDirection != ClipVariableScrollDirection.Bounce && ScrollDirection == ClipVariableScrollDirection.Bounce)
            currentIndex = 0;

        previousScrollDirection = ScrollDirection;

        if (OnlyScrollWhenTruncated != previousOnlyScrollWhenTruncated)
        {
            currentIndex = 0;
            previousOnlyScrollWhenTruncated = OnlyScrollWhenTruncated;
        }

        var variableValue = ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.Value.Value;
        var formattedValue = variableValue is not null ? Format(variableValue) : string.Empty;
        if (string.IsNullOrEmpty(formattedValue)) return string.Empty;

        var formattedValueInfo = new StringInfo(formattedValue);

        var willTruncate = TruncateLength >= 0 && formattedValueInfo.LengthInTextElements > TruncateLength;
        var willScroll = (!OnlyScrollWhenTruncated || willTruncate) && ScrollSpeed > 0;
        var addEllipses = IncludeEllipses && !willScroll && willTruncate;

        if (!willScroll) currentIndex = 0;

        var stringInfo = new StringInfo(formattedValue);

        if (willScroll && ScrollDirection == ClipVariableScrollDirection.Bounce && OnlyScrollWhenTruncated)
        {
            var charsLeft = bounceDirection ? stringInfo.LengthInTextElements - currentIndex - TruncateLength : currentIndex;

            if (charsLeft == 0)
                bounceDirection = !bounceDirection;

            var localScrollSpeed = ScrollSpeed;
            localScrollSpeed = charsLeft != 0 ? Math.Min(localScrollSpeed, charsLeft) : localScrollSpeed;

            formattedValue = stringInfo.SubstringByTextElements(currentIndex, TruncateLength);
            currentIndex += bounceDirection ? localScrollSpeed : -localScrollSpeed;
        }
        else
        {
            if (willScroll && ScrollDirection != ClipVariableScrollDirection.Bounce)
            {
                if (!string.IsNullOrEmpty(JoinString)) formattedValue += JoinString;

                switch (ScrollDirection)
                {
                    case ClipVariableScrollDirection.Right:
                        currentIndex += ScrollSpeed;
                        break;

                    case ClipVariableScrollDirection.Left:
                        currentIndex -= ScrollSpeed;
                        break;
                }
            }

            var localTruncateLength = TruncateLength;

            if (addEllipses && stringInfo.LengthInTextElements > 3)
                formattedValue = $"{stringInfo.SubstringByTextElements(0, localTruncateLength - 3)}...";

            var localStringInfo = new StringInfo(formattedValue);
            var position = currentIndex.Modulo(localStringInfo.LengthInTextElements);
            formattedValue = cropAndWrapText(localStringInfo, position, localTruncateLength < 0 ? localStringInfo.LengthInTextElements : localTruncateLength);
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

    private static string cropAndWrapText(StringInfo text, int position, int maxLength)
    {
        var endPos = Math.Min(position + maxLength, text.LengthInTextElements);
        var subText = text.SubstringByTextElements(position, endPos - position);
        var subTextInfo = new StringInfo(subText);
        return endPos != text.LengthInTextElements ? subText : subText + text.SubstringByTextElements(0, Math.Min(maxLength - subTextInfo.LengthInTextElements, position));
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
    public UserControl? ViewInstance { get; }

    public RenderableClipVariableOption(PropertyInfo propertyInfo, string displayName, string description, UserControl? viewInstance)
    {
        PropertyInfo = propertyInfo;
        DisplayName = displayName;
        Description = description;
        ViewInstance = viewInstance;
    }
}
