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
            var moduleName = ModuleID switch
            {
                null => "Built-In",
                "internal.pulse" => "Pulse",
                _ => ModuleManager.GetInstance().GetModuleOfID(ModuleID).Title
            };

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
                else if (propertyInfo.PropertyType == typeof(List<string>))
                {
                    viewInstance = new ListVariableOptionView(this, propertyInfo);
                }
                else if (propertyInfo.PropertyType == typeof(DateTimeOffset))
                {
                    viewInstance = new DateTimeVariableOptionView(this, propertyInfo);
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

    public virtual void OnDeserialised()
    {
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

    public virtual bool IsDefault() => CaseMode == ClipVariableCaseMode.Default &&
                                       TruncateLength == -1 &&
                                       !IncludeEllipses &&
                                       ScrollDirection == ClipVariableScrollDirection.Left &&
                                       ScrollSpeed == 0 &&
                                       JoinString == string.Empty &&
                                       !OnlyScrollWhenTruncated;

    private int currentIndex;
    private bool bounceDirection;
    private ClipVariableScrollDirection previousScrollDirection;
    private bool previousOnlyScrollWhenTruncated;

    public string GetFormattedValue()
    {
        updateState();

        var variableValue = ChatBoxManager.GetInstance().GetVariable(ModuleID, VariableID)!.Value.Value;
        var value = variableValue is not null ? Format(variableValue) : string.Empty;

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var text = new StringInfo(value);
        var isTruncated = TruncateLength >= 0 && text.LengthInTextElements > TruncateLength;
        var isScrolling = ScrollSpeed > 0 && (!OnlyScrollWhenTruncated || isTruncated);

        if (!isScrolling)
        {
            currentIndex = 0;
            return applyCase(truncate(text, IncludeEllipses && isTruncated));
        }

        var result = ScrollDirection switch
        {
            ClipVariableScrollDirection.Bounce when OnlyScrollWhenTruncated => scrollBounce(text),
            ClipVariableScrollDirection.Left => scrollLoop(text, -ScrollSpeed),
            ClipVariableScrollDirection.Right => scrollLoop(text, ScrollSpeed),
            _ => value
        };

        return applyCase(result);
    }

    private void updateState()
    {
        var enteredBounce = previousScrollDirection != ClipVariableScrollDirection.Bounce && ScrollDirection == ClipVariableScrollDirection.Bounce;
        var changedScrollWhenTruncated = OnlyScrollWhenTruncated != previousOnlyScrollWhenTruncated;

        if (enteredBounce || changedScrollWhenTruncated)
        {
            currentIndex = 0;
            bounceDirection = true;
        }

        previousScrollDirection = ScrollDirection;
        previousOnlyScrollWhenTruncated = OnlyScrollWhenTruncated;
    }

    private string scrollBounce(StringInfo text)
    {
        var maxIndex = text.LengthInTextElements - TruncateLength;
        currentIndex = int.Clamp(currentIndex, 0, maxIndex);

        var result = text.SubstringByTextElements(currentIndex, TruncateLength);
        var distanceToEdge = bounceDirection ? maxIndex - currentIndex : currentIndex;

        var movement = int.Min(ScrollSpeed, distanceToEdge);
        currentIndex += bounceDirection ? movement : -movement;

        if (currentIndex == 0 || currentIndex == maxIndex)
            bounceDirection = !bounceDirection;

        return result;
    }

    private string scrollLoop(StringInfo text, int offset)
    {
        var value = text.String;

        if (!string.IsNullOrEmpty(JoinString))
            value += JoinString;

        var scrollingText = new StringInfo(value);

        currentIndex += offset;
        currentIndex = currentIndex.Modulo(scrollingText.LengthInTextElements);

        var maxLength = TruncateLength < 0 ? scrollingText.LengthInTextElements : TruncateLength;
        return cropAndWrap(scrollingText, currentIndex, maxLength);
    }

    private string truncate(StringInfo text, bool addEllipses)
    {
        if (TruncateLength < 0 || text.LengthInTextElements <= TruncateLength)
            return text.String;

        if (!addEllipses || TruncateLength <= 1)
            return text.SubstringByTextElements(0, TruncateLength);

        return text.SubstringByTextElements(0, TruncateLength - 1) + '…';
    }

    private static string cropAndWrap(StringInfo text, int position, int maxLength)
    {
        if (maxLength <= 0 || text.LengthInTextElements == 0)
            return string.Empty;

        position = position.Modulo(text.LengthInTextElements);

        var end = int.Min(position + maxLength, text.LengthInTextElements);
        var result = text.SubstringByTextElements(position, end - position);

        if (end != text.LengthInTextElements) return result;

        var remainingLength = maxLength - (end - position);

        if (remainingLength > 0)
            result += text.SubstringByTextElements(0, int.Min(remainingLength, position));

        return result;
    }

    private string applyCase(string value) => CaseMode switch
    {
        ClipVariableCaseMode.Default => value,
        ClipVariableCaseMode.Lower => value.ToLower(CultureInfo.CurrentCulture),
        ClipVariableCaseMode.Upper => value.ToUpper(CultureInfo.CurrentCulture),
        ClipVariableCaseMode.Title => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value),
        _ => throw new ArgumentOutOfRangeException(nameof(CaseMode), CaseMode, $"{nameof(CaseMode)} is out of range for {nameof(ClipVariableCaseMode)}")
    };

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
    Upper,
    Title
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