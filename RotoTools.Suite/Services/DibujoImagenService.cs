using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>Resultado de intentar cargar una vista previa visual de un Dibujo (Paso 2 del
    /// asistente de Simulación: "ahí quiero que se muestre el dibujo del modelo"). Nunca lanza: si
    /// no se puede generar una imagen, Imagen queda a null y Mensaje explica el motivo, para que la
    /// UI lo muestre sin romper el asistente (mismo criterio que el resto de esta funcionalidad:
    /// puramente de lectura/diagnóstico, cualquier fallo se degrada con un aviso, nunca revienta).</summary>
    public sealed class ImagenDibujo
    {
        public BitmapSource? Imagen { get; set; }
        public string? Mensaje { get; set; }

        /// <summary>De qué columna (Thumbnail/Metafile) salió la imagen, solo informativo.</summary>
        public string? Fuente { get; set; }
    }

    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): genera una vista
    /// previa visual de un Dibujo a partir de las columnas Thumbnail/Metafile/XamlSilverlight de la
    /// tabla Dibujos (petición del usuario). Se ha confirmado por búsqueda exhaustiva en el código
    /// fuente del RotoTools original que esas tres columnas NUNCA se leen ni se renderizan ahí -no
    /// hay ningún precedente que copiar-, así que el formato exacto de cada una tras descomprimir
    /// se DETECTA en tiempo de ejecución por firma de bytes en vez de asumirse fijo: si el formato
    /// no es ninguno de los reconocidos, esa columna se descarta y se prueba la siguiente, en vez
    /// de reventar la vista previa (o el asistente) por una suposición incorrecta.
    ///
    /// Igual que Buffer (ver DibujoOpcionesRotoService.LeerXmlDescomprimido), estas columnas están
    /// comprimidas con las mismas funciones del esquema [zlib] (confirmado por el usuario: "también
    /// están comprimidos como en Buffer"). A diferencia de Buffer -que es texto XML y por eso se lee
    /// con CAST a NVARCHAR(MAX)-, aquí hace falta el CAST a VARBINARY(MAX): un CAST a texto
    /// corrompería datos binarios de imagen.
    /// </summary>
    public static class DibujoImagenService
    {
        /// <summary>Orden de columnas a probar como imagen rasterizable: Metafile PRIMERO, no
        /// Thumbnail -al revés de la primera versión-. Motivo, confirmado por el usuario tras ver
        /// que la vista previa seguía pixelada pese a rasterizar el metafile a mayor resolución
        /// (ver DecodificarComoMetafile): con Thumbnail primero, ese raster ya pequeño decodificaba
        /// bien a la primera y el bucle de abajo se paraba ahí -devolvía esa imagen y ni siquiera
        /// llegaba a probar Metafile-, así que la mejora de calidad del metafile nunca se estaba
        /// usando en la práctica. Metafile es VECTORIAL (se puede rasterizar nítido a cualquier
        /// tamaño), Thumbnail es un raster ya fijo y pequeño sin más datos que exprimir: si Metafile
        /// decodifica bien, siempre es la opción de más calidad, así que va primero. Thumbnail queda
        /// como último recurso si Metafile no estuviera disponible o no decodificara. XamlSilverlight
        /// se deja fuera de este bucle a propósito: es texto/markup, no una imagen que se pueda
        /// decodificar igual que las otras dos (ver CargarVistaPrevia).</summary>
        private static readonly string[] ColumnasRaster = { "Metafile", "Thumbnail" };

        /// <summary>Columnas a probar para la vista previa del Paso 1 en modo "Presupuesto" (ver
        /// CargarVistaPreviaPresupuesto). Solo "Metafile" -a diferencia de ColumnasRaster, que
        /// también prueba "Thumbnail"-: el usuario solo mencionó esa columna para ContenidoPAFBlob,
        /// con su propia duda ("creo que es el campo Metafile"), sin indicar que exista ninguna
        /// columna equivalente a Thumbnail en esta tabla. Si el nombre no fuera correcto, el error
        /// de SQL (columna inexistente) queda igualmente capturado por el try/catch de más abajo y
        /// se degrada al mensaje genérico de "no se ha podido generar", como el resto de esta
        /// funcionalidad de solo lectura/diagnóstico.</summary>
        private static readonly string[] ColumnasRasterPresupuesto = { "Metafile" };

        public static ImagenDibujo CargarVistaPrevia(string codigoDibujo)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            foreach (var columna in ColumnasRaster)
            {
                byte[]? bytes;
                try
                {
                    bytes = DibujoOpcionesRotoService.EjecutarConReintentoZlib(
                        () => LeerColumnaBinariaDescomprimida(conexion, codigoDibujo, columna));
                }
                catch
                {
                    // Esta columna en concreto ha fallado (p.ej. no aplica la misma función zlib a
                    // este tipo de contenido): se prueba con la siguiente en vez de abortar toda la
                    // vista previa.
                    continue;
                }
                if (bytes == null || bytes.Length == 0) continue;

                var imagen = DecodificarComoRaster(bytes) ?? DecodificarComoMetafile(bytes);
                if (imagen != null)
                    return new ImagenDibujo { Imagen = imagen, Fuente = columna };
            }

            bool tieneXaml = false;
            try
            {
                var xamlBytes = DibujoOpcionesRotoService.EjecutarConReintentoZlib(
                    () => LeerColumnaBinariaDescomprimida(conexion, codigoDibujo, "XamlSilverlight"));
                tieneXaml = xamlBytes is { Length: > 0 };
            }
            catch
            {
                // Sin XamlSilverlight tampoco: se deja tieneXaml=false, el mensaje genérico de abajo
                // ya lo cubre.
            }

            return new ImagenDibujo
            {
                Imagen = null,
                Mensaje = tieneXaml
                    ? SuiteLocalization.GetString("L_Suite_SoloVistaPreviaXamlSilverlight")
                    : SuiteLocalization.GetString("L_Suite_NoSePudoGenerarVistaPreviaDibujo")
            };
        }

        /// <summary>Mismo criterio que DibujoOpcionesRotoService.LeerXmlDescomprimido/
        /// DescomprimirBytes: la función de DEScompresión es siempre [zlib].[UnzipBLOB] (no hace
        /// falta resolverla dinámicamente como la de compresión, ver ResolverFuncionComprimir).
        /// Aquí el CAST del resultado es a VARBINARY(MAX), no a NVARCHAR(MAX): el contenido es una
        /// imagen (bytes crudos), no texto, y pasarlo por una conversión de texto lo corrompería.</summary>
        private static byte[]? LeerColumnaBinariaDescomprimida(SqlConnection conexion, string codigoDibujo, string columna)
        {
            using var cmd = new SqlCommand(
                $"SELECT CAST([zlib].[UnzipBLOB]({columna}) AS VARBINARY(MAX)) FROM Dibujos WHERE Codigo=@codigo", conexion);
            cmd.Parameters.AddWithValue("@codigo", codigoDibujo);

            object? resultado = cmd.ExecuteScalar();
            if (resultado == null || resultado == DBNull.Value) return null;
            return resultado as byte[];
        }

        /// <summary>
        /// Vista previa del Paso 1 en modo "Presupuesto" (petición del usuario: "Al elegir el orden
        /// se previsualizará el dibujo del modelo tambien de la tabla ContenidoPAFBlob, creo que es
        /// el campo Metafile, tal como en la carga de dibujos de la base de datos"). Mismo patrón
        /// EXACTO que CargarVistaPrevia -mismo orden de intentos (raster estándar, luego metaarchivo
        /// WMF/EMF vía DecodificarComoMetafile, reutilizados tal cual), mismo criterio de
        /// degradación silenciosa columna a columna-, pero contra ContenidoPAFBlob filtrado por
        /// Numero+Version+Orden en vez de Dibujos filtrado por Codigo, y solo probando "Metafile"
        /// (ver el comentario de ColumnasRasterPresupuesto sobre por qué no también "Thumbnail").
        /// </summary>
        public static ImagenDibujo CargarVistaPreviaPresupuesto(string numero, string version, string orden)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            foreach (var columna in ColumnasRasterPresupuesto)
            {
                byte[]? bytes;
                try
                {
                    bytes = DibujoOpcionesRotoService.EjecutarConReintentoZlib(
                        () => LeerColumnaBinariaDescomprimidaPresupuesto(conexion, numero, version, orden, columna));
                }
                catch
                {
                    continue;
                }
                if (bytes == null || bytes.Length == 0) continue;

                var imagen = DecodificarComoRaster(bytes) ?? DecodificarComoMetafile(bytes);
                if (imagen != null)
                    return new ImagenDibujo { Imagen = imagen, Fuente = columna };
            }

            return new ImagenDibujo
            {
                Imagen = null,
                Mensaje = SuiteLocalization.GetString("L_Suite_NoSePudoGenerarVistaPreviaPresupuesto")
            };
        }

        /// <summary>Mismo criterio que LeerColumnaBinariaDescomprimida, contra ContenidoPAFBlob
        /// filtrado por Numero+Version+Orden en vez de Dibujos filtrado por Codigo.</summary>
        private static byte[]? LeerColumnaBinariaDescomprimidaPresupuesto(SqlConnection conexion, string numero, string version, string orden, string columna)
        {
            using var cmd = new SqlCommand(
                $"SELECT CAST([zlib].[UnzipBLOB]({columna}) AS VARBINARY(MAX)) FROM ContenidoPAFBlob WHERE Numero=@numero AND Version=@version AND Orden=@orden", conexion);
            cmd.Parameters.AddWithValue("@numero", numero);
            cmd.Parameters.AddWithValue("@version", version);
            cmd.Parameters.AddWithValue("@orden", orden);

            object? resultado = cmd.ExecuteScalar();
            if (resultado == null || resultado == DBNull.Value) return null;
            return resultado as byte[];
        }

        /// <summary>Intenta decodificar "bytes" como una imagen ráster estándar (PNG/JPEG/BMP/GIF/
        /// TIFF: BitmapImage detecta el formato por firma automáticamente). Null si no lo es.</summary>
        private static BitmapSource? DecodificarComoRaster(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Intenta decodificar "bytes" como un metaarchivo WMF/EMF (vectorial de Windows) y
        /// rasterizarlo a una imagen que WPF pueda mostrar. Usa System.Drawing (disponible sin
        /// paquete NuevoAdicional: UseWindowsForms=true en el .csproj ya trae ese ensamblado; se
        /// referencia con nombre completo -no "using System.Drawing;"- porque ese using global se
        /// retira a propósito en todo el proyecto para no chocar con los tipos de WPF, ver
        /// comentario del .csproj). Null si no es un metaarchivo válido.</summary>
        private static BitmapSource? DecodificarComoMetafile(byte[] bytes)
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                using var metafile = new System.Drawing.Imaging.Metafile(ms);

                // Un metaarchivo es VECTORIAL: GDI+ puede "reproducirlo" (DrawImage) a cualquier
                // tamaño de destino sin perder nitidez, porque vuelve a dibujar sus comandos en vez
                // de escalar unos píxeles ya fijados. El bug real de la primera versión era
                // rasterizar aquí al tamaño NATIVO del metafile (metafile.Width/Height, con
                // frecuencia pequeño para un dibujo técnico) y dejar que WPF ampliara esa imagen ya
                // pequeña al hacer zoom -de ahí el pixelado visto en pantalla-. Se rasteriza aquí,
                // de una vez, a un tamaño bastante mayor (lado mayor objetivo, sin encoger si el
                // metafile ya declara un tamaño mayor: no tiene sentido reducir un vector) para que
                // el zoom de la vista previa siga nítido en un rango razonable.
                const int LadoMayorObjetivo = 3000;
                const double EscalaMaxima = 12.0;

                double anchoOriginal = metafile.Width > 0 ? metafile.Width : 800;
                double altoOriginal = metafile.Height > 0 ? metafile.Height : 600;

                double escala = LadoMayorObjetivo / Math.Max(anchoOriginal, altoOriginal);
                escala = Math.Max(1.0, Math.Min(escala, EscalaMaxima));

                int ancho = (int)Math.Round(anchoOriginal * escala);
                int alto = (int)Math.Round(altoOriginal * escala);

                using var bmp = new System.Drawing.Bitmap(ancho, alto);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.DrawImage(metafile, 0, 0, ancho, alto);
                }

                using var ms2 = new MemoryStream();
                bmp.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                ms2.Position = 0;

                var wpfBmp = new BitmapImage();
                wpfBmp.BeginInit();
                wpfBmp.CacheOption = BitmapCacheOption.OnLoad;
                wpfBmp.StreamSource = ms2;
                wpfBmp.EndInit();
                wpfBmp.Freeze();
                return wpfBmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
