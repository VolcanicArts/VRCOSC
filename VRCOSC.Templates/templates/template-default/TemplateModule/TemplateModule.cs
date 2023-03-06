using VRCOSC.Game.Modules;

namespace TemplateModule
{
    public partial class TemplateModule : Module
    {
        public override string Title => "Template";
        public override string Description => "Template description";
        public override string Author => "Author";
        public override ModuleType Type => ModuleType.General;
        protected override TimeSpan DeltaUpdate => TimeSpan.MaxValue;

        protected override void CreateAttributes()
        {
            CreateSetting(TemplateSetting.ExampleSetting, "Example Setting", "An example setting", string.Empty);
            CreateParameter<bool>(TemplateParameter.ExampleParameter, ParameterMode.ReadWrite, "ExampleParameterName", "Example Parameter Display Name", "This is an example parameter");
        }

        protected override void OnModuleStart()
        {
        }

        protected override void OnModuleUpdate()
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
