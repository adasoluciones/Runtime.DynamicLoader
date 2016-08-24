using Ada.Framework.Configuration.Xml;
using Ada.Framework.RunTime.DynamicLoader.Config.Entities;
using System.Xml;

namespace Ada.Framework.RunTime.DynamicLoader.Config
{
    public class DynamicLoaderConfigManager : ConfiguracionXmlManager<DynamicLoaderConfigTag>
    {
        public override string NombreArchivoConfiguracion { get { return "DynamicLoaderConfig"; } }

        public override string NombreArchivoPorDefecto { get { return "DynamicLoader.Config.xml"; } }

        public override string NombreArchivoValidacionConfiguracion { get { return "DynamicLoaderConfigValidate"; } }

        public override string NombreArchivoValidacionPorDefecto { get { return "DynamicLoader.Config.xsd"; } }

        protected override bool ValidarXmlSchema { get { return false; } }

        protected override void ValidarXml(XmlDocument documento) { }
    }
}
