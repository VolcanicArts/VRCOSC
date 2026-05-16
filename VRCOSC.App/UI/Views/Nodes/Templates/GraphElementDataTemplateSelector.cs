// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Flow;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Views.Nodes.ViewModels;

namespace VRCOSC.App.UI.Views.Nodes.Templates;

public class GraphElementDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate CastNode { get; set; }
    public required DataTemplate CollapsedNode { get; set; }
    public required DataTemplate RegularNode { get; set; }
    public required DataTemplate SourceNode { get; set; }
    public required DataTemplate CallNode { get; set; }
    public required DataTemplate DisplayNode { get; set; }
    public required DataTemplate PassthroughDisplayNode { get; set; }
    public required DataTemplate RelayNode { get; set; }
    public required DataTemplate CheckBoxValueNode { get; set; }
    public required DataTemplate TextBoxValueNode { get; set; }
    public required DataTemplate ComboBoxValueNode { get; set; }
    public required DataTemplate KeybindValueNode { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is NodeViewModel nodeViewModel)
        {
            var node = nodeViewModel.Node;
            var type = node.GetType();
            var isGeneric = type.IsGenericType;

            if (isGeneric)
            {
                var genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(ValueNode<>))
                {
                    var instanceType = type.GetGenericArguments()[0];

                    if (instanceType == typeof(bool)) return CheckBoxValueNode;

                    if (instanceType == typeof(string) || instanceType == typeof(int) || instanceType == typeof(long)
                        || instanceType == typeof(float) || instanceType == typeof(double) || instanceType == typeof(short))
                        return TextBoxValueNode;

                    if (instanceType.IsEnum) return ComboBoxValueNode;

                    if (instanceType == typeof(Keybind))
                        return KeybindValueNode;
                }

                if (genericType == typeof(DisplayNode<>)) return DisplayNode;
                if (genericType == typeof(PassthroughDisplayNode<>)) return PassthroughDisplayNode;
                if (genericType == typeof(RelayNode<>)) return RelayNode;
                if (genericType == typeof(CastNode<,>)) return CastNode;
            }

            if (type == typeof(ButtonNode))
                return CallNode;

            if (node.Metadata.Shared.IsCollapsed)
                return CollapsedNode;

            if (node.Metadata.IsSourceNode)
                return SourceNode;

            return RegularNode;
        }

        return null;
    }
}