// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Title { get; } = new("New Group");
    public ObservableCollection<Guid> Nodes { get; } = [];
}