// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Actions;

/// <summary>
/// Represents an action that has progress associated with it
/// </summary>
public abstract class ProgressAction
{
    public virtual string Title => string.Empty;

    public Action? OnComplete;

    public Action<float>? OnProgressChanged;

    public virtual bool UseProgressBar => false;

    /// <summary>
    /// Executes this <see cref="ProgressAction"/>
    /// </summary>
    public async Task Execute()
    {
        await Perform();
        OnComplete?.Invoke();
    }

    protected abstract Task Perform();
}