// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Flow;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Views.Nodes.ViewModels;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.Templates;

public class GraphElementDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate CastNode { get; set; }
    public required DataTemplate CollapsedNode { get; set; }
    public required DataTemplate HoistedInputNode { get; set; }
    public required DataTemplate NoInlineNode { get; set; }
    public required DataTemplate RegularNode { get; set; }
    public required DataTemplate SourceNode { get; set; }
    public required DataTemplate DriveNode { get; set; }
    public required DataTemplate ButtonNode { get; set; }
    public required DataTemplate SwitchNode { get; set; }
    public required DataTemplate DisplayNode { get; set; }
    public required DataTemplate PassthroughDisplayNode { get; set; }
    public required DataTemplate FlowDisplayNode { get; set; }
    public required DataTemplate ColorDisplayNode { get; set; }
    public required DataTemplate RelayNode { get; set; }
    public required DataTemplate CheckBoxValueNode { get; set; }
    public required DataTemplate TextBoxValueNode { get; set; }
    public required DataTemplate TextBoxNumericValueNode { get; set; }
    public required DataTemplate ComboBoxValueNode { get; set; }
    public required DataTemplate KeybindValueNode { get; set; }
    public required DataTemplate DateTimeValueNode { get; set; }
    public required DataTemplate TimeSpanValueNode { get; set; }
    public required DataTemplate ColorValueNode { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is NodeViewModel nodeViewModel)
        {
            var node = nodeViewModel.Node;
            var type = node.GetType();
            var isGeneric = node.Metadata.Shared.TypeGenerics.Length != 0;

            if (isGeneric)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(SwitchNode<>))
                    return SwitchNode;

                if (genericType == typeof(ValueNode<>))
                {
                    var instanceType = node.Metadata.Shared.TypeGenerics[0];
                    instanceType = Nullable.GetUnderlyingType(instanceType) ?? instanceType;

                    if (NodeConstants.NUMERIC_TYPES.Contains(instanceType))
                        return TextBoxNumericValueNode;

                    if (NodeConstants.TEXTBOX_TYPES.Contains(instanceType))
                        return TextBoxValueNode;

                    if (instanceType.IsEnum) return ComboBoxValueNode;
                    if (instanceType == typeof(bool)) return CheckBoxValueNode;
                    if (instanceType == typeof(Keybind)) return KeybindValueNode;
                    if (instanceType == typeof(DateTime)) return DateTimeValueNode;
                    if (instanceType == typeof(TimeSpan)) return TimeSpanValueNode;
                    if (instanceType == typeof(Color)) return ColorValueNode;
                    if (instanceType == typeof(ColorHSL)) return ColorValueNode;
                }

                if (genericType == typeof(DisplayNode<>))
                {
                    var instanceType = node.Metadata.Shared.TypeGenerics[0];
                    if (instanceType == typeof(Color)) return ColorDisplayNode;
                    if (instanceType == typeof(ColorHSL)) return ColorDisplayNode;

                    return DisplayNode;
                }

                if (genericType == typeof(PassthroughDisplayNode<>)) return PassthroughDisplayNode;
                if (genericType == typeof(RelayNode<>)) return RelayNode;
                if (genericType == typeof(CastNode<,>)) return CastNode;
            }

            if (type == typeof(FlowDisplayNode))
                return FlowDisplayNode;

            if (type == typeof(ButtonNode))
                return ButtonNode;

            if (node.Metadata.Shared.IsCollapsed)
                return CollapsedNode;

            if (node.Metadata.Shared.IsSourceNode)
                return SourceNode;

            if (node.Metadata.Shared.IsDriveNode)
                return DriveNode;

            var valueInputs = node.Metadata.Elements[ConnectionPoint.ValueInput];
            var canBeNoInline = false;

            if (node.Metadata.Shared.IsValueInput && node.Metadata.Shared.IsValueOutput && node.Metadata.Shared.Elements[ConnectionPoint.ValueOutput].All(e => !e.IsList))
            {
                canBeNoInline = true;

                for (var i = 0; i < node.Metadata.Shared.ValueOutputCount; i++)
                {
                    if (i < node.Metadata.Shared.ValueInputCount)
                    {
                        var metadata = valueInputs[i];

                        if (metadata.Shared.IsInlineable && metadata.Shared.Modes.HasFlag(InputModes.Inline))
                        {
                            canBeNoInline = false;
                            break;
                        }
                    }
                }
            }

            if (node.Metadata.Shared.HasAllInlineOnly)
                return HoistedInputNode;

            if (canBeNoInline)
                return NoInlineNode;

            return RegularNode;
        }

        return null;
    }
}