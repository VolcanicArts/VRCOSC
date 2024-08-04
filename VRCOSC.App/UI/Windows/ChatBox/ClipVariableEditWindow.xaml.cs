// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.ChatBox.Clips.Variables;

namespace VRCOSC.App.UI.Windows.ChatBox;

public partial class ClipVariableEditWindow
{
    public ClipVariable ClipVariable { get; }

    public ClipVariableEditWindow(ClipVariable clipVariable)
    {
        InitializeComponent();

        DataContext = clipVariable;
        ClipVariable = clipVariable;

        Title = $"Editing {clipVariable.DisplayNameWithModule}";
    }
}
