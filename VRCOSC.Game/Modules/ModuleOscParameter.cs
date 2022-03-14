namespace VRCOSC.Game.Modules;

public class ModuleOscParameter
{
    public string Reference { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string Address { get; set; }

    public ModuleOscParameter(string reference, string displayName, string description, string initialAddress)
    {
        Reference = reference;
        DisplayName = displayName;
        Description = description;
        Address = initialAddress;
    }
}
