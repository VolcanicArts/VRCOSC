using VRCOSC.Game.Modules;

namespace TemplateModule
{
    [ModuleTitle("Template")]
    [ModuleDescription("Template description")]
    [ModuleAuthor("Template Author")]
    [ModuleGroup(ModuleType.General)]
    public partial class TemplateModule : AvatarModule
    {
        protected override void CreateAttributes()
        {
            CreateSetting(TemplateSetting.ExampleSetting, "Example Setting", "An example setting", string.Empty);
            CreateParameter<bool>(TemplateParameter.ExampleParameter, ParameterMode.ReadWrite, "ExampleParameterName", "Example Parameter Display Name", "This is an example parameter");
        }

        protected override void OnModuleStart()
        {
        }

        [ModuleUpdate(ModuleUpdateMode.Custom)]
        private void moduleUpdate()
        {
        }

        protected override void OnModuleStop()
        {
        }

        private enum TemplateSetting
        {
            ExampleSetting
        }

        private enum TemplateParameter
        {
            ExampleParameter
        }
    }
}
