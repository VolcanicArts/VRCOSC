// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes;

[AttributeUsage(AttributeTargets.Method)]
public class NodeProcessAttribute : Attribute
{
    public NodeProcessAttribute()
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class NodeProcessLoopAttribute : Attribute
{
    public int FinishFlowSlot { get; }
    public int LoopFlowSlot { get; }

    public NodeProcessLoopAttribute(int finishFlowSlot, int loopFlowSlot)
    {
        FinishFlowSlot = finishFlowSlot;
        LoopFlowSlot = loopFlowSlot;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeFlowAttribute : Attribute
{
    public bool IsTrigger { get; }
    public int NumFlowOutputs { get; }

    public NodeFlowAttribute(bool isTrigger = false, int numFlowOutputs = 1)
    {
        IsTrigger = isTrigger;
        NumFlowOutputs = numFlowOutputs;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeValueAttribute : Attribute
{
    public Type[] ValueOutputTypes { get; }

    public NodeValueAttribute(Type[]? valueOutputTypes = null)
    {
        ValueOutputTypes = valueOutputTypes ?? [];
    }
}