// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodePreset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Preset");
    public List<SerialisableNode> Nodes { get; set; } = [];
    public List<SerialisableConnection> Connections { get; set; } = [];
    public List<SerialisableNodeGroup> Groups { get; set; } = [];
    public List<SerialisableGraphVariable> Variables { get; set; } = [];

    private readonly SerialisationManager serialiser;

    public NodePreset()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodePresetSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Load(string importPath = "")
    {
        if (string.IsNullOrEmpty(importPath))
            serialiser.Deserialise();
        else
            serialiser.Deserialise(false, importPath);

        Name.Subscribe(_ => serialiser.Serialise());
    }

    public void Serialise()
    {
        serialiser.Serialise();
    }
}