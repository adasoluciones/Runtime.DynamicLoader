using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ada.Framework.RunTime.DynamicLoader.Config.Entities
{
    [XmlType("AppDomain")]
    public class AppDomainTag
    {
        [XmlAttribute("Name")]
        public string Nombre { get; set; }

        [XmlArrayItem(typeof(DirectoryTag))]
        [XmlArrayItem(typeof(AssemblyTag))]
        [XmlArray("Elements")]
        public List<AElementsTag> Elementos { get; set; }
    }
    
    public abstract class AElementsTag
    {
        [XmlAttribute("Path")]
        public string Ruta { get; set; }
    }

    [XmlType("Directory")]
    public class DirectoryTag : AElementsTag
    {
        [XmlAttribute("Recursive")]
        public bool Recursivo { get; set; }
    }

    [XmlType("Assembly")]
    public class AssemblyTag : AElementsTag { }
}
