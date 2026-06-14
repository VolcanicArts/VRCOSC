// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Files;

public abstract class HandleFilePathTransformAsyncNode<T>(string outputName = "") : TryValueTransformAsyncNode<string?, T>("Path", outputName)
{
    protected override async Task<Result<T>> TryTransformValueAsync(string? path, IPulseContext c)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return Result<T>.Fail();

        return await HandlePathAsync(path, c);
    }

    protected abstract Task<T> HandlePathAsync(string path, IPulseContext c);
}

public abstract class SimpleHandleFilePathTransformAsyncNode<T>(Func<string, Task<T>> func, string outputName = "") : HandleFilePathTransformAsyncNode<T>(outputName)
{
    protected override Task<T> HandlePathAsync(string path, IPulseContext c) => func(path);
}

public abstract class HandleFilePathTransformNode<T>(string outputName = "") : HandleFilePathTransformAsyncNode<T>(outputName)
{
    protected override Task<T> HandlePathAsync(string path, IPulseContext c) => Task.FromResult(HandlePath(path, c));

    protected abstract T HandlePath(string path, IPulseContext c);
}

public abstract class SimpleHandleFilePathTransformNode<T>(Func<string, T> func, string outputName = "") : HandleFilePathTransformNode<T>(outputName)
{
    protected override T HandlePath(string path, IPulseContext c) => func(path);
}

public abstract class TryHandleFilePathActionAsyncNode(string pathInputName = "") : TryActionAsyncNode
{
    public ValueInput<string?> Path = new(string.IsNullOrEmpty(pathInputName) ? "Path" : pathInputName);

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        var path = Path.Read(c);

        if (!IsPathValid(path)) return false;

        return await TryHandlePathAsync(path, c);
    }

    protected virtual bool IsPathValid([NotNullWhen(true)] string? path) => !string.IsNullOrEmpty(path);

    protected abstract Task<bool> TryHandlePathAsync(string path, IPulseContext c);
}

public abstract class TryHandleFilePathActionNode(string pathInputName = "") : TryHandleFilePathActionAsyncNode(pathInputName)
{
    protected override Task<bool> TryHandlePathAsync(string path, IPulseContext c) => Task.FromResult(TryHandlePath(path, c));

    protected abstract bool TryHandlePath(string path, IPulseContext c);
}