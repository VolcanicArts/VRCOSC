// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Numerics;
using VRCOSC.App.Nodes.Serialisation;
using VRCOSC.App.Nodes.Serialisation.V1;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodePreset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Observable<string> Name { get; } = new("New Preset");
    public SerialisableNodeGraphBase Structure { get; set; } = new();

    private readonly SerialisationManager serialiser;

    public NodePreset()
    {
        serialiser = new SerialisationManager();
        serialiser.RegisterSerialiser(1, new NodePresetSerialiserV1(AppManager.GetInstance().Storage, this));
        serialiser.RegisterSerialiser(2, new NodePresetSerialiser(AppManager.GetInstance().Storage, this));
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

    public IEnumerable<Guid> SpawnTo(NodeGraph targetGraph, Vector2 offset) => NodeGraphBaseHelper.Deserialise(Structure, targetGraph, true, offset);
}