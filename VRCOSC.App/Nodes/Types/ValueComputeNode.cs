// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueComputeNode<T1> : Node
{
    public ValueOutput<T1> Result;

    protected ValueComputeNode(string resultName = "")
    {
        Result = new ValueOutput<T1>(string.IsNullOrEmpty(resultName) ? "Result" : resultName);
    }

    protected override Task Process(PulseContext c)
    {
        Result.Write(ComputeValue(c), c);
        return Task.CompletedTask;
    }

    protected abstract T1 ComputeValue(PulseContext c);
}

public abstract class ValueComputeNode<T1, T2> : Node
{
    public ValueOutput<T1> A;
    public ValueOutput<T2> B;

    protected ValueComputeNode(string aName = "", string bName = "")
    {
        A = new ValueOutput<T1>(aName);
        B = new ValueOutput<T2>(bName);
    }

    protected override Task Process(PulseContext c)
    {
        var values = ComputeValues(c);
        A.Write(values.Item1, c);
        B.Write(values.Item2, c);
        return Task.CompletedTask;
    }

    protected abstract (T1, T2) ComputeValues(PulseContext c);
}

public abstract class ValueComputeNode<T1, T2, T3> : Node
{
    public ValueOutput<T1> A;
    public ValueOutput<T2> B;
    public ValueOutput<T3> C;

    protected ValueComputeNode(string aName = "", string bName = "", string cName = "")
    {
        A = new ValueOutput<T1>(aName);
        B = new ValueOutput<T2>(bName);
        C = new ValueOutput<T3>(cName);
    }

    protected override Task Process(PulseContext c)
    {
        var values = ComputeValues(c);
        A.Write(values.Item1, c);
        B.Write(values.Item2, c);
        C.Write(values.Item3, c);
        return Task.CompletedTask;
    }

    protected abstract (T1, T2, T3) ComputeValues(PulseContext c);
}

public abstract class ValueComputeNode<T1, T2, T3, T4> : Node
{
    public ValueOutput<T1> A;
    public ValueOutput<T2> B;
    public ValueOutput<T3> C;
    public ValueOutput<T4> D;

    protected ValueComputeNode(string aName = "", string bName = "", string cName = "", string dName = "")
    {
        A = new ValueOutput<T1>(aName);
        B = new ValueOutput<T2>(bName);
        C = new ValueOutput<T3>(cName);
        D = new ValueOutput<T4>(dName);
    }

    protected override Task Process(PulseContext c)
    {
        var values = ComputeValues(c);
        A.Write(values.Item1, c);
        B.Write(values.Item2, c);
        C.Write(values.Item3, c);
        D.Write(values.Item4, c);
        return Task.CompletedTask;
    }

    protected abstract (T1, T2, T3, T4) ComputeValues(PulseContext c);
}