using Ada.Framework.RunTime.DynamicLoader.Config;
using Ada.Framework.RunTime.DynamicLoader.Config.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DynamicLoaderConfigTag tag = new DynamicLoaderConfigManager().ObtenerConfiguracion();
            
        }
    }
}
