// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueSourceNode<T>(string resultName = "") : ValueComputeNode<T>(resultName), IContinuousNode
{
    public virtual int UpdateOffset => 0;
}

public abstract class SimpleValueSourceNode<T>(Func<T> func, string resultName = "") : ValueSourceNode<T>(resultName)
{
    protected override T ComputeValue(IPulseContext c) => func();
}