using Markdig.Helpers;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules;

public abstract class Module : Container
{
    public virtual string Title => "Unknown";
    public virtual string Description => "Unknown description";
    public virtual OrderedList<ModuleSetting> Settings => new();
    public virtual OrderedList<ModuleOscParameter> Parameters => new();

    public abstract void Start();
    public abstract void Stop();
}
