using Markdig.Helpers;

namespace VRCOSC.Game.Modules;

public class ModuleParametersManager
{
    public OrderedList<ModuleOscParameter> Parameters;

    public ModuleParametersManager()
    {
        Parameters = new OrderedList<ModuleOscParameter>();
    }

    public ModuleParametersManager(OrderedList<ModuleOscParameter> initialParameters)
    {
        Parameters = initialParameters;
    }
}
