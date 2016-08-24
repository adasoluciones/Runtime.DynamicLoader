using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ada.Framework.RunTime.DynamicLoader.Config.Entities
{
    [XmlRoot("DynamicLoaderConfig")]
    public class DynamicLoaderConfigTag
    {
        [XmlArray]
        public List<AppDomainTag> Domains { get; set; }

        [XmlArrayItem(typeof(DirectoryTag))]
        [XmlArrayItem(typeof(AssemblyTag))]
        [XmlArray("Elements")]
        public List<AElementsTag> Elementos { get; set; }
    }
}
