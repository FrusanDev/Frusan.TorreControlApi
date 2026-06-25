using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Unity;

namespace Utilitarios.InversionDeControl
{
    public static class CargadorModulos
    {
        public static void CargarContenedor(IUnityContainer contenedor, string ruta, string patron)
        {
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(ruta, patron);
            ImportDefinition definition = CargadorModulos.BuildImportDefinition();
            try
            {
                using (AggregateCatalog aggregateCatalog = new AggregateCatalog())
                {
                    aggregateCatalog.Catalogs.Add((ComposablePartCatalog)directoryCatalog);
                    using (CompositionContainer compositionContainer = new CompositionContainer((ComposablePartCatalog)aggregateCatalog, new ExportProvider[0]))
                    {
                        IEnumerable<IModulo> modulos = compositionContainer.GetExports(definition).Select<Export, IModulo>((Func<Export, IModulo>)(export => export.Value as IModulo)).Where<IModulo>((Func<IModulo, bool>)(m => m != null));
                        RegistrarModulos registrarModulos = new RegistrarModulos(contenedor);
                        foreach (IModulo modulo in modulos)
                            modulo.Initialize((IRegistrarModulos)registrarModulos);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Exception loaderException in ex.LoaderExceptions)
                    stringBuilder.AppendFormat("{0}\n", (object)loaderException.Message);
                throw new TypeLoadException(stringBuilder.ToString(), (Exception)ex);
            }
        }

        private static ImportDefinition BuildImportDefinition()
        {
            return new ImportDefinition((Expression<Func<ExportDefinition, bool>>)(def => true), typeof(IModulo).FullName, ImportCardinality.ZeroOrMore, false, false);
        }
    }
}
