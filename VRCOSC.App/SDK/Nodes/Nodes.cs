// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Modules;

namespace VRCOSC.App.SDK.Nodes;

public abstract class ModuleNode<T> : Node where T : Module
{
    public T Module => (T)ModuleManager.GetInstance().GetModuleInstanceFromType(typeof(T));
    protected override bool ShouldProcess(PulseContext c) => ModuleManager.GetInstance().IsModuleRunning(Module.ID);
}