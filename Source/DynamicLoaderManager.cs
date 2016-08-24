using Ada.Framework.RunTime.DynamicLoader.Config;
using Ada.Framework.RunTime.DynamicLoader.Config.Entities;
using Ada.Framework.Util.FileMonitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ada.Framework.RunTime.DynamicLoader
{
    public static class DynamicLoaderManager
    {
        private static bool Inicializado { get; set; }
        public static string CodigoDominioActual { get { return "[Current]"; } }
        public static IList<AppDomain> Dominios { get; private set; }
        private static DynamicLoaderConfigTag Config = null;

        public delegate void AlCargarEnsamblado(Assembly ensamblado);
        public delegate void AlDescargarEnsamblado(Assembly ensamblado);
        public delegate void AlCambiarRuta(string oldPath, string newPath);
        public delegate void AlRecargarEnsamblado(Assembly oldAssembly, Assembly newAssembly);
        
        public static event AlCargarEnsamblado OnLoadAssembly;
        public static event AlDescargarEnsamblado OnUnLoadAssembly;
        public static event AlRecargarEnsamblado OnReLoadAssembly;

        #region Dominios

            public static void RegistrarDominio(AppDomain dominio)
            {
                if (ObtenerDominio(dominio.FriendlyName) == null)
                {
                    Dominios.Add(dominio);
                }
            }
        
            public static AppDomain CrearDominio(string nombreDominio)
            {
                AppDomain retorno = ObtenerDominio(nombreDominio);

                if (retorno == null)
                {
                    retorno = AppDomain.CreateDomain(nombreDominio);
                    RegistrarDominio(retorno);
                }

                return retorno;
            }

            public static AppDomain ObtenerDominio(string nombreDominio)
            {
                if (nombreDominio.Equals(CodigoDominioActual, StringComparison.InvariantCultureIgnoreCase))
                {
                    return AppDomain.CurrentDomain;
                }

                return Dominios.FirstOrDefault(c => c.FriendlyName.Equals(nombreDominio));
            }

            public static void DescargarDominio(AppDomain dominio)
            {
                AppDomain.Unload(dominio);
            }

            public static void DescargarDominio(string nombreDominio)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if(dominio != null)
                {
                    DescargarDominio(dominio);
                }
            }

        #endregion Dominios

        #region Ensamblados
            
            public static Assembly[] ObtenerEnsamblados()
            {
                IList<Assembly> retorno = new List<Assembly>();

                foreach (AppDomain dominio in Dominios)
                {
                    retorno.Concat(dominio.GetAssemblies());
                }

                return retorno.ToArray();
            }
        
            public static Assembly[] ObtenerEnsamblados(string nombreDominio)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if (dominio != null)
                {
                    return dominio.GetAssemblies();
                }

                return null;
            }

            public static Assembly ObtenerEnsamblado(string nombreDominio, string assemblyName)
            {
                Assembly[] ensamblados = ObtenerEnsamblados(nombreDominio);

                if (ensamblados != null)
                {
                    return ensamblados.FirstOrDefault(c => c.GetName().Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase));
                }

                return null;
            }

            public static Assembly ObtenerEnsamblado(string assemblyName)
            {
                Assembly[] ensamblados = ObtenerEnsamblados();

                if (ensamblados != null)
                {
                    return ensamblados.FirstOrDefault(c => c.GetName().Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase));
                }

                return null;
            }

            public static Assembly ObtenerEnsambladoPorRuta(AppDomain dominio, string rutaEnsamblado)
            {
                return dominio.GetAssemblies().FirstOrDefault(c => c.Location.Equals(rutaEnsamblado, StringComparison.InvariantCultureIgnoreCase));
            }

            public static Assembly ObtenerEnsambladoPorRuta(string nombreDominio, string rutaEnsamblado)
            {
                rutaEnsamblado = MonitorArchivoFactory.ObtenerArchivo().ObtenerRutaAbsoluta(rutaEnsamblado);

                AppDomain dominio = ObtenerDominio(nombreDominio);

                if (dominio != null)
                {
                    return ObtenerEnsambladoPorRuta(dominio, rutaEnsamblado);
                }

                return null;
            }

            public static Assembly ObtenerEnsambladoPorRuta(string rutaEnsamblado)
            {
                rutaEnsamblado = MonitorArchivoFactory.ObtenerArchivo().ObtenerRutaAbsoluta(rutaEnsamblado);

                Assembly[] ensamblados = ObtenerEnsamblados();

                if (ensamblados != null)
                {
                    return ensamblados.FirstOrDefault(c => c.Location.Equals(rutaEnsamblado, StringComparison.InvariantCultureIgnoreCase));
                }

                return null;
            }

            public static void CargarEnsamblados(AppDomain dominio, string rutaCarpeta, bool recursivo = false)
            {
                SearchOption opcionBusqueda = recursivo ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                
                foreach (string rutaEnsamblado in Directory.GetFiles(rutaCarpeta, "*.dll", opcionBusqueda))
                {
                    FileInfo fileInfo = new FileInfo(rutaEnsamblado);

                    if (fileInfo.Extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        CargarEnsamblado(dominio, rutaEnsamblado);
                    }
                }
            }

            public static void CargarEnsamblados(string rutaCarpeta, bool recursivo = false)
            {
                SearchOption opcionBusqueda = recursivo ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                foreach (string rutaEnsamblado in Directory.GetFiles(rutaCarpeta, "*.dll", opcionBusqueda))
                {
                    FileInfo fileInfo = new FileInfo(rutaEnsamblado);

                    if (fileInfo.Extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        AppDomain dominio = CrearDominio(rutaEnsamblado);
                        CargarEnsamblado(dominio, rutaEnsamblado);
                    }
                }
            }

            public static void CargarEnsamblados(string nombreDominio, string rutaCarpeta, bool recursivo = false)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if(dominio != null)
                {
                    CargarEnsamblados(dominio, rutaCarpeta, recursivo);
                }
            }
        
            public static Assembly CargarEnsamblado(AppDomain dominio, string rutaEnsamblado)
            {
                rutaEnsamblado = MonitorArchivoFactory.ObtenerArchivo().ObtenerRutaAbsoluta(rutaEnsamblado);

                Assembly retorno = ObtenerEnsambladoPorRuta(dominio, rutaEnsamblado);

                if (retorno == null)
                {
                    string nombreEnsamblado = Assembly.LoadFrom(rutaEnsamblado).FullName;
                    retorno = dominio.Load(nombreEnsamblado);
                    OnLoadAssembly(retorno);
                }

                return retorno;
            }

            public static Assembly CargarEnsamblado(string nombreDominio, string rutaEnsamblado)
            {
                rutaEnsamblado = MonitorArchivoFactory.ObtenerArchivo().ObtenerRutaAbsoluta(rutaEnsamblado);

                AppDomain dominio = ObtenerDominio(nombreDominio);
                
                if (dominio != null)
                {
                    return CargarEnsamblado(dominio, rutaEnsamblado);
                }

                return null;
            }
            
            public static Assembly CargarEnsamblado(string rutaEnsamblado)
            {
                return CargarEnsamblado(AppDomain.CurrentDomain, rutaEnsamblado);
            }
            
            public static void DescargarEnsamblado(string nombreEnsamblado)
            {
                foreach (AppDomain dominio in Dominios)
                {
                    Assembly assembly = dominio.GetAssemblies().FirstOrDefault(c => c.FullName.Equals(nombreEnsamblado));

                    if (assembly != null)
                    {
                        AppDomain.Unload(dominio);
                        OnUnLoadAssembly(assembly);
                    }
                }
            }

            public static void DescargarEnsambladoPorRuta(string rutaEnsamblado)
            {
                foreach (AppDomain dominio in Dominios)
                {
                    Assembly assembly = dominio.GetAssemblies().FirstOrDefault(c => c.Location.Equals(rutaEnsamblado));

                    if (assembly != null)
                    {
                        AppDomain.Unload(dominio);
                        OnUnLoadAssembly(assembly);
                    }
                }
            }

            public static void DescargarEnsamblado(Assembly ensamblado)
            {
                DescargarEnsamblado(ensamblado.FullName);
            }
            
            public static void RecargarEnsamblado(string rutaEnsamblado)
            {
                Assembly oldAssembly = ObtenerEnsambladoPorRuta(rutaEnsamblado);
                
                DescargarEnsambladoPorRuta(rutaEnsamblado);
                Assembly newAssembly = CargarEnsamblado(rutaEnsamblado);
                
                OnReLoadAssembly(oldAssembly, newAssembly);
            }
            
            public static void RecargarEnsamblado(AppDomain dominio, string rutaEnsamblado)
            {
                Assembly oldAssembly = ObtenerEnsambladoPorRuta(dominio, rutaEnsamblado);
                
                DescargarEnsambladoPorRuta(rutaEnsamblado);
                Assembly newAssembly = CargarEnsamblado(dominio, rutaEnsamblado);

                OnReLoadAssembly(oldAssembly, newAssembly);
            }
        
            public static void RecargarEnsamblado(string nombreDominio, string rutaEnsamblado)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if(dominio != null)
                {
                    RecargarEnsamblado(dominio, rutaEnsamblado);
                }
            }

        #endregion Ensamblados

        #region Instancias
            
            public static IList<T> ObtenerInstancias<T>(Assembly ensamblado)
            {
                IList<T> retorno = new List<T>();

                foreach (Type tipo in ensamblado.GetTypes())
                {
                    if (typeof(T).IsAssignableFrom(tipo))
                    {
                        T instancia = (T)Activator.CreateInstance(tipo);
                        retorno.Add(instancia);
                    }
                }

                return retorno;
            }

            public static IList<T> ObtenerInstancias<T>()
            {
                IList<T> retorno = new List<T>();

                foreach (Assembly ensamblado in ObtenerEnsamblados())
                {
                    retorno.Concat(ObtenerInstancias<T>(ensamblado));
                }

                return retorno;
            }
            
            public static IList<T> ObtenerInstancias<T>(AppDomain dominio)
            {
                IList<T> retorno = new List<T>();

                foreach (Assembly ensamblado in dominio.GetAssemblies())
                {
                    retorno.Concat(ObtenerInstancias<T>(ensamblado));
                }
            
                return retorno;
            }

            public static IList<T> ObtenerInstancias<T>(string nombreDominio)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if(dominio != null)
                {
                    return ObtenerInstancias<T>(dominio);
                }

                return new List<T>();
            }

            public static IList<T> ObtenerInstancias<T>(string nombreDominio, string nombreEnsamblado)
            {
                Assembly ensamblado = ObtenerEnsamblado(nombreDominio, nombreEnsamblado);

                if (ensamblado != null)
                {
                    return ObtenerInstancias<T>(ensamblado);
                }

                return new List<T>();
            }

            public static IList<T> ObtenerInstancias<T>(AppDomain dominio, string nombreEnsamblado)
            {
                Assembly ensamblado = dominio.GetAssemblies().FirstOrDefault(c => c.FullName.Equals(nombreEnsamblado));

                if (ensamblado != null)
                {
                    return ObtenerInstancias<T>(ensamblado);
                }

                return new List<T>();
            }
            
            public static T ObtenerInstancia<T>(Assembly ensamblado)
            {
                return ObtenerInstancias<T>(ensamblado).FirstOrDefault();
            }
            
            public static T ObtenerInstancia<T>()
            {
                return ObtenerInstancias<T>().FirstOrDefault();
            }
            
            public static T ObtenerInstancia<T>(AppDomain dominio)
            {
                return ObtenerInstancias<T>(dominio).FirstOrDefault();
            }
            
            public static T ObtenerInstancia<T>(string nombreDominio)
            {
                return ObtenerInstancias<T>(nombreDominio).FirstOrDefault();
            }
            
            public static T ObtenerInstancia<T>(string nombreDominio, string nombreEnsamblado)
            {
                return ObtenerInstancias<T>(nombreDominio, nombreEnsamblado).FirstOrDefault();
            }
            
            public static T ObtenerInstancia<T>(AppDomain dominio, string nombreEnsamblado)
            {
                return ObtenerInstancias<T>(dominio, nombreEnsamblado).FirstOrDefault();
            }
            
            public static IList<object> ObtenerInstancias(Type tipo, Assembly ensamblado)
            {
                IList<object> retorno = new List<object>();

                foreach (Type tipoEnsamblado in ensamblado.GetTypes())
                {
                    if (tipo.IsAssignableFrom(tipoEnsamblado))
                    {
                        object instancia = Activator.CreateInstance(tipoEnsamblado);
                        retorno.Add(instancia);
                    }
                }

                return retorno;
            }

            public static IList<object> ObtenerInstancias(Type tipo)
            {
                IList<object> retorno = new List<object>();

                foreach (Assembly ensamblado in ObtenerEnsamblados())
                {
                    retorno.Concat(ObtenerInstancias(tipo, ensamblado));
                }

                return retorno;
            }

            public static IList<object> ObtenerInstancias(Type tipo, AppDomain dominio)
            {
                IList<object> retorno = new List<object>();

                foreach (Assembly ensamblado in dominio.GetAssemblies())
                {
                    retorno.Concat(ObtenerInstancias(tipo, ensamblado));
                }

                return retorno;
            }

            public static IList<object> ObtenerInstancias(Type tipo, string nombreDominio)
            {
                AppDomain dominio = ObtenerDominio(nombreDominio);

                if (dominio != null)
                {
                    return ObtenerInstancias(tipo, dominio);
                }

                return new List<object>();
            }

            public static IList<object> ObtenerInstancias(Type tipo, string nombreDominio, string nombreEnsamblado)
            {
                Assembly ensamblado = ObtenerEnsamblado(nombreDominio, nombreEnsamblado);

                if (ensamblado != null)
                {
                    return ObtenerInstancias(tipo, ensamblado);
                }

                return new List<object>();
            }

            public static IList<object> ObtenerInstancias(Type tipo, AppDomain dominio, string nombreEnsamblado)
            {
                Assembly ensamblado = dominio.GetAssemblies().FirstOrDefault(c => c.FullName.Equals(nombreEnsamblado));

                if (ensamblado != null)
                {
                    return ObtenerInstancias(tipo, ensamblado);
                }

                return new List<object>();
            }
        
            public static object ObtenerInstancia(Type tipo, Assembly ensamblado)
            {
                return ObtenerInstancias(tipo, ensamblado).FirstOrDefault();
            }

            public static object ObtenerInstancia(Type tipo)
            {
                return ObtenerInstancias(tipo).FirstOrDefault();
            }

            public static object ObtenerInstancia(Type tipo, AppDomain dominio)
            {
                return ObtenerInstancias(tipo, dominio).FirstOrDefault();
            }

            public static object ObtenerInstancia(Type tipo, string nombreDominio)
            {
                return ObtenerInstancias(tipo, nombreDominio).FirstOrDefault();
            }

            public static object ObtenerInstancia(Type tipo, string nombreDominio, string nombreEnsamblado)
            {
                return ObtenerInstancias(tipo, nombreDominio, nombreEnsamblado).FirstOrDefault();
            }

            public static object ObtenerInstancia(Type tipo, AppDomain dominio, string nombreEnsamblado)
            {
                return ObtenerInstancias(tipo, dominio, nombreEnsamblado).FirstOrDefault();
            }

        #endregion Instancias

        static DynamicLoaderManager()
        {
            Dominios = new List<AppDomain>();
            Inicializar();
        }

        public static void Inicializar()
        {
            if (!Inicializado)
            {
                Config = new DynamicLoaderConfigManager().ObtenerConfiguracion();

                IMonitorArchivo monitor = MonitorArchivoFactory.ObtenerArchivo();

                foreach (AppDomainTag appDomainTag in Config.Domains)
                {
                    foreach (AElementsTag elemento in appDomainTag.Elementos)
                    {
                        elemento.Ruta = monitor.ObtenerRutaAbsoluta(elemento.Ruta);

                        if (elemento is DirectoryTag)
                        {
                            CargarEnsamblados(appDomainTag.Nombre, elemento.Ruta, (elemento as DirectoryTag).Recursivo);
                        }
                        else if (elemento is AssemblyTag)
                        {
                            CargarEnsamblado(appDomainTag.Nombre, elemento.Ruta);
                        }

                        FileSystemWatcher watcher = new FileSystemWatcher();
                        watcher.Path = elemento.Ruta;
                        watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                        watcher.Filter = "*.dll";

                        watcher.Changed += new FileSystemEventHandler(OnChanged);
                        watcher.EnableRaisingEvents = true;
                    }
                }

                foreach (AElementsTag elemento in Config.Elementos)
                {
                    elemento.Ruta = monitor.ObtenerRutaAbsoluta(elemento.Ruta);

                    if (elemento is DirectoryTag)
                    {
                        CargarEnsamblados(elemento.Ruta, (elemento as DirectoryTag).Recursivo);
                    }
                    else if (elemento is AssemblyTag)
                    {
                        AppDomain dominio = CrearDominio(elemento.Ruta);
                        CargarEnsamblado(dominio, elemento.Ruta);
                    }

                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = elemento.Ruta;
                    watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                    watcher.Filter = "*.dll";

                    watcher.Changed += new FileSystemEventHandler(OnChanged);
                    watcher.EnableRaisingEvents = true;
                }
            }
        }

        public static void ReInicializar()
        {
            Inicializado = false;
            Inicializar();
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            IMonitorArchivo monitor = MonitorArchivoFactory.ObtenerArchivo();

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    AppDomain dominio = CrearDominio(e.FullPath);
                    CargarEnsamblado(dominio, e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    DescargarEnsambladoPorRuta(e.FullPath);
                    break;
                case WatcherChangeTypes.Changed:
                    RecargarEnsamblado(e.FullPath);
                    break;
                case WatcherChangeTypes.Renamed:
                    
                    break;
            }
        }
    }
}