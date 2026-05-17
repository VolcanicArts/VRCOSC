// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public partial class CommentViewModel : GridGraphElementViewModel
{
    public IComment Comment { get; }

    [ObservableProperty]
    private bool editing;

    public CommentViewModel(IComment comment)
    {
        Comment = comment;
        Position = comment.Position.Value.AsPoint;
    }

    public override void SetPosition(Point newPosition)
    {
        base.SetPosition(newPosition);
        Comment.Position.Value = newPosition.AsVector;
    }
}