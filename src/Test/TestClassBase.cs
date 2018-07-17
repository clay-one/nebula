using ComposerCore;
using ComposerCore.Implementation;
using ComposerCore.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    public class TestClassBase
    {
        protected IComponentContext Composer { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            ConfigureComposer();
        }

        private void ConfigureComposer()
        {
            if (Composer != null)
                return;

            var composer = new ComponentContext();

            composer.RegisterAssembly("Nebula");
            composer.ProcessCompositionXml("Connections.config");

            Composer = composer;
        }
    }
}