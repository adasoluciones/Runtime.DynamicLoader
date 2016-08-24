using Ada.Framework.Util.Object;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Reflection;

namespace Ada.Framework.RunTime.DynamicLoader
{
    [AttributeUsage(AttributeTargets.Property)]
    [PSerializable]
    public sealed class InyectarAttribute : OnMethodBoundaryAspect
    {
        [PNonSerialized]
        private Type Tipo = null;

        [PNonSerialized]
        private AppDomain Dominio = null;

        [PNonSerialized]
        private Assembly Ensamblado = null;

        private bool EsColeccion = false;
        
        public InyectarAttribute() { }

        public InyectarAttribute(Type tipo)
        {
            Tipo = tipo;
        }

        public InyectarAttribute(AppDomain dominio)
        {
            Dominio = dominio;
        }

        public InyectarAttribute(Type tipo, AppDomain dominio)
        {
            Tipo = tipo;
            Dominio = dominio;
        }

        public InyectarAttribute(Assembly ensamblado)
        {
            Ensamblado = ensamblado;
        }

        public InyectarAttribute(Type tipo, Assembly ensamblado)
        {
            Tipo = tipo;
            Ensamblado = ensamblado;
        }

        public InyectarAttribute(string nombreDominio)
        {
            Dominio = DynamicLoaderManager.ObtenerDominio(nombreDominio);
        }

        public InyectarAttribute(Type tipo, string nombreDominio)
        {
            Tipo = tipo;
            Dominio = DynamicLoaderManager.ObtenerDominio(nombreDominio);
        }

        public InyectarAttribute(string nombreDominio, string nombreEnsamblado)
        {
            Dominio = DynamicLoaderManager.ObtenerDominio(nombreDominio);
            Ensamblado = DynamicLoaderManager.ObtenerEnsamblado(nombreDominio, nombreEnsamblado);
        }

        public InyectarAttribute(Type tipo, string nombreDominio, string nombreEnsamblado)
        {
            Tipo = tipo;
            Dominio = DynamicLoaderManager.ObtenerDominio(nombreDominio);
            Ensamblado = DynamicLoaderManager.ObtenerEnsamblado(nombreDominio, nombreEnsamblado);
        }

        public InyectarAttribute(AppDomain dominio, string nombreEnsamblado)
        {
            Ensamblado = DynamicLoaderManager.ObtenerEnsamblado(nombreEnsamblado);
        }

        public InyectarAttribute(Type tipo, AppDomain dominio, string nombreEnsamblado)
        {
            Tipo = tipo;
            Ensamblado = DynamicLoaderManager.ObtenerEnsamblado(nombreEnsamblado);
        }
        
        #region Build-Time Logic
        
            public override bool CompileTimeValidate(MethodBase method)
            {
                /*
                if (false)
                {
                    Message.Write(method, SeverityType.Error, "MY001", "Cannot apply InyectarAttribute to method '{0}'.", method);
                    return false;
                }*/

                return true;
            }
        
        #endregion
        
        public override void OnExit(MethodExecutionArgs args)
        {
            MethodInfo methodInfo = args.Method as MethodInfo;

            if (Tipo == null) Tipo = methodInfo.ReturnType;

            IObjeto objeto = ObjetoFactory.ObtenerObjeto();

            if(objeto.esColeccion(Tipo))
            {
                Tipo = Type.GetType(objeto.TipoSingular(Tipo));
                EsColeccion = true;
            }

            object instancia = null;

            if (Ensamblado != null)
            {
                if(EsColeccion)
                {
                    instancia = DynamicLoaderManager.ObtenerInstancias(Tipo, Ensamblado);
                }
                else
                {
                    instancia = DynamicLoaderManager.ObtenerInstancia(Tipo, Ensamblado);
                }
            }
            
            if (instancia == null)
            {
                if (Dominio != null)
                {
                    if(EsColeccion)
                    {
                        instancia = DynamicLoaderManager.ObtenerInstancias(Tipo, Dominio);
                    }
                    else
                    {
                        instancia = DynamicLoaderManager.ObtenerInstancia(Tipo, Dominio);
                    }
                }
            }

            if (instancia == null)
            {
                if(EsColeccion)
                {
                    instancia = DynamicLoaderManager.ObtenerInstancias(Tipo);
                }
                else
                {
                    instancia = DynamicLoaderManager.ObtenerInstancia(Tipo);
                }
            }

            if(instancia != null)
            {
                args.ReturnValue = Convert.ChangeType(instancia, methodInfo.ReturnType);
            }
            else
            {
                args.ReturnValue = Activator.CreateInstance(methodInfo.ReturnType);
            }
        }
    }
}
