// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public partial class GroupViewModel : GridGraphElementViewModel
{
    public override Guid Id => Group.Id;

    public NodeGroup Group { get; }

    [ObservableProperty]
    private bool editing;

    [ObservableProperty]
    private double width;

    [ObservableProperty]
    private double height;

    public GroupViewModel(NodeGroup group)
    {
        Group = group;
    }
}