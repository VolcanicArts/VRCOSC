// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueSourceNode<T1> : ValueComputeNode<T1>, IActiveUpdateNode
{
    public virtual int UpdateOffset => 0;
    private readonly GlobalStore<T1> prevValue = new();

    private readonly Func<T1> _func;

    protected ValueSourceNode(Func<T1> func, string resultName = "") : base(resultName) => _func = func;

    protected override T1 ComputeValue(PulseContext c) => _func();

    public Task<bool> OnUpdate(PulseContext c)
    {
        var value = _func();

        if (EqualityComparer<T1>.Default.Equals(value, prevValue.Read(c))) return Task.FromResult(false);

        prevValue.Write(value, c);
        return Task.FromResult(true);
    }
}