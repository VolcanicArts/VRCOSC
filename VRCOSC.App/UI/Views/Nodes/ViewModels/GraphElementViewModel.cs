// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public abstract partial class GraphElementViewModel : ObservableObject
{
    [ObservableProperty]
    private int zIndex;

    public FrameworkElement Control { get; set; } = null!;
}

public abstract partial class GridGraphElementViewModel : GraphElementViewModel
{
    [ObservableProperty]
    private Point position;

    public Point SnapOffset { get; set; }

    public virtual void SetPosition(Point newPosition)
    {
        Position = newPosition;
    }
}