// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public class NodeViewModel : GridGraphElementViewModel
{
    public NodeViewModel(INode node)
    {
        Node = node;
        base.SetPosition(new Point(node.Metadata.Position.X, node.Metadata.Position.Y));
    }

    public INode Node { get; }

    public string GenericTypesToString => string.Join(", ", Node.Metadata.Shared.GenericTypes.Select(arg => arg.GetFriendlyName()));

    public FrameworkElement SnappingControl { get; set; } = null!;

    public List<FrameworkElement>[] FlowInputControls { get; set; } = [];
    public List<FrameworkElement>[] FlowOutputControls { get; set; } = [];
    public List<FrameworkElement>[] ValueInputControls { get; set; } = [];
    public List<FrameworkElement>[] ValueOutputControls { get; set; } = [];

    public override void SetPosition(Point newPosition)
    {
        Node.Metadata.Position = new Vector2((float)newPosition.X, (float)newPosition.Y);
        base.SetPosition(newPosition);
    }
}