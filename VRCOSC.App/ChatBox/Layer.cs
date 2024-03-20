// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using VRCOSC.App.ChatBox.Clips;

namespace VRCOSC.App.ChatBox;

public class Layer
{
    public ObservableCollection<Clip> Clips { get; } = new();
}
