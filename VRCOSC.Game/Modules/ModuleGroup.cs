// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules;

public class ModuleGroup : Container<ModuleContainer>
{
    public readonly ModuleType Type;

    public ModuleGroup(ModuleType type)
    {
        Type = type;
    }

    public void Start()
    {
        this.ForEach(child => child.Start());
    }

    public void Stop()
    {
        this.ForEach(child => child.Stop());
    }
}
