// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Nodes.Types;

namespace VRCOSC.App.Nodes;

public interface IRelay : IGraphElement;

public interface IFlowRelay : IRelay;

public interface IValueRelay : IRelay
{
    public Type Type { get; }
}

public class Relay : GraphElement, IRelay;

public class FlowRelay : Relay, IFlowRelay;

public class ValueRelay : Relay, IValueRelay
{
    public Type Type { get; }

    public ValueRelay(Type type)
    {
        Type = type;
    }
}