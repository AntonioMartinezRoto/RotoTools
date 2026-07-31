# RotoTools Suite (segundo intento — proyecto nuevo e independiente)

## Por qué este cambio de enfoque

El primer intento (`RotoTools.Modern`, ya descartado) usaba una librería de terceros para dar el
aspecto "Fluent" (WPF-UI). Esa librería obligaba a adivinar nombres exactos de su API (sobre todo
nombres de iconos) que no se pudieron verificar uno a uno en el entorno donde se escribió el
código, y eso causó bastantes errores de compilación difíciles de depurar.

Este proyecto nuevo (`RotoTools.Suite`) usa **solo WPF "de fábrica" de .NET 8, sin ninguna
librería de terceros para la interfaz**. El aspecto moderno (fondo con degradado, tarjetas con
sombra, menú lateral oscuro con acento rojo Roto, insignias de color por módulo en vez de un
paquete de iconos) se ha construido a mano con estilos WPF estándar
(`Style`/`ControlTemplate`/`Trigger`), que es API muy estable y bien conocida — mucho menos
propensa a errores de compilación que una librería externa. Sigo en C#/.NET porque así se
reutiliza el 100% de la lógica de negocio ya existente (acceso a SQL Server, catálogos, reglas de
negocio) sin reescribir ni arriesgar nada de eso; cambiar a un lenguaje totalmente distinto
obligaría a reescribir esa lógica desde cero, con mucho más riesgo que beneficio para este caso.

**Este proyecto es completamente independiente**: vive en su propia carpeta (`RotoTools.Suite\`)
y tiene su propio fichero de solución (`RotoTools.Suite.sln`, en la raíz del repositorio). El
`RotoTools.sln` y el `RotoTools.csproj` originales **no se han tocado en absoluto** — puedes
seguir abriendo y usando la app clásica exactamente igual que siempre.

## Qué incluye esta primera entrega

Esta vez, a propósito, la entrega es más pequeña que el primer intento, para asegurar que
compila y se ve sin errores antes de avanzar:

- Ventana principal maximizada, con cabecera de marca (logo Roto real + selector de idioma) y
  menú lateral con los 10 módulos del menú original (cada uno con una insignia de color propio).
- Multilenguaje funcionando (español/inglés/portugués), reutilizando tal cual el
  `LocalizationManager` y los `.resx` ya existentes.
- Página de "Inicio" explicando la fase de la migración.
- El resto de módulos (incluido CAM) muestran de momento una página "próximamente": la lógica de
  cada uno se irá incorporando módulo a módulo en próximas entregas, empezando por CAM en la
  siguiente.

## Paso a paso: cómo verla en Visual Studio

1. En el explorador de archivos, ve a la carpeta del repositorio (`...\repos\RotoTools\`).
2. Haz doble clic en **`RotoTools.Suite.sln`** (el fichero nuevo, no en `RotoTools.sln`). Se abre
   Visual Studio con dos proyectos en el Explorador de soluciones: `RotoTools` (el de siempre) y
   `RotoTools.Suite` (este nuevo).
3. En el Explorador de soluciones, clic derecho sobre **`RotoTools.Suite`** → **"Establecer como
   proyecto de inicio"** (Set as Startup Project). Debe quedar en **negrita**.
4. Arriba, comprueba que el desplegable de configuración pone **Debug** (no Release).
5. Pulsa **F5** (o el botón ▶ verde "RotoTools.Suite"). La primera vez, Visual Studio restaura los
   paquetes NuGet automáticamente (este proyecto en concreto no añade ninguno nuevo: solo usa lo
   que ya tenía `RotoTools.csproj`).
6. Debería abrirse la ventana maximizada con el menú lateral y el logo Roto en la cabecera.

### Si sigue sin compilar

Copia y pégame el texto exacto de la ventana **"Lista de errores"** (Error List) de Visual
Studio — con el mensaje literal puedo corregir el fichero concreto en минutos. Sin el texto del
error es muy difícil acertar a la primera qué falla en tu máquina en concreto.

Comprobaciones rápidas más habituales, por si acaso:

- **Visual Studio 2022** (17.8 o superior) con la carga de trabajo **".NET desktop development"**
  instalada (Herramientas → Obtener herramientas y características).
- **SDK de .NET 8** instalado (`dotnet --version` en una terminal debería empezar por `8.`).
- Si Visual Studio se queja de que no encuentra `RotoTools.csproj`: confirma que no has movido la
  carpeta `RotoTools.Suite` fuera de la carpeta del repositorio (usa una ruta relativa
  `..\RotoTools.csproj` para encontrar el proyecto original).

## Publicar como ejecutable portable (más adelante)

Cuando quieras generar el `.exe` portable (no hace falta para verlo en Visual Studio con F5):

```
cd RotoTools.Suite
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Siguiente paso

En cuanto confirmes que esto compila y se ve bien, seguimos con la migración del primer módulo
funcional (CAM), reutilizando exactamente la misma lógica de negocio, ya sin la librería de
terceros que causaba los errores.
