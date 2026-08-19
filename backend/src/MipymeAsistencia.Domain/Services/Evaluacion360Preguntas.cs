namespace MipymeAsistencia.Domain.Services;

/// <summary>
/// Catálogo fijo de las 20 preguntas ponderadas del modelo 360°.
/// Fuente: FALTANTE.md — Tabla Ponderada de 20 Preguntas Esenciales.
/// </summary>
public static class Evaluacion360Preguntas
{
    public record Pregunta(int Numero, string Categoria, string Texto, string Tipo, decimal Peso);

    public static readonly IReadOnlyList<Pregunta> Catalogo = new[]
    {
        new Pregunta(1,  "Pensamiento Estratégico",  "¿Comprende las implicaciones de sus decisiones en el negocio a corto y largo plazo?",           "Estratégica",  8m),
        new Pregunta(2,  "Pensamiento Estratégico",  "¿Determina objetivos claros y establece prioridades efectivas para alcanzarlos?",                "Estratégica",  7m),
        new Pregunta(3,  "Pensamiento Estratégico",  "¿Considera los riesgos e implicaciones antes de ejecutar una acción relevante?",                 "Operativa",    5m),
        new Pregunta(4,  "Organización y Tiempo",    "¿Completa de manera efectiva, en tiempo y forma, los proyectos asignados?",                      "Crítica",      8m),
        new Pregunta(5,  "Organización y Tiempo",    "¿Es capaz de jerarquizar y reordenar prioridades ante cambios o imprevistos?",                   "Operativa",    4m),
        new Pregunta(6,  "Organización y Tiempo",    "¿Utiliza eficientemente los recursos asignados (presupuesto, herramientas, tiempo)?",            "Operativa",    3m),
        new Pregunta(7,  "Resolución de Problemas",  "¿Se enfoca en la causa raíz de los asuntos clave para resolver problemas efectivamente?",        "Crítica",      7m),
        new Pregunta(8,  "Resolución de Problemas",  "¿Conserva la calma, la objetividad y el control en situaciones complicadas o bajo presión?",     "Conductual",   6m),
        new Pregunta(9,  "Resolución de Problemas",  "¿Recauda y analiza información de diferentes fuentes antes de tomar una decisión?",              "Operativa",    5m),
        new Pregunta(10, "Trabajo en Equipo",         "¿Se desempeña como un miembro activo, colaborativo y comprometido con el grupo?",                "Conductual",   6m),
        new Pregunta(11, "Trabajo en Equipo",         "¿Inspira, motiva y guía al equipo para el logro de las metas comunes?",                         "Liderazgo",    5m),
        new Pregunta(12, "Trabajo en Equipo",         "¿Comparte activamente sus conocimientos, habilidades y experiencia con los demás?",              "Conductual",   4m),
        new Pregunta(13, "Comunicación Asertiva",     "¿Expresa sus ideas y opiniones con claridad, precisión y respeto hacia la otra persona?",        "Conductual",   5m),
        new Pregunta(14, "Comunicación Asertiva",     "¿Escucha activamente y muestra apertura ante críticas constructivas u opiniones ajenas?",        "Conductual",   4m),
        new Pregunta(15, "Comunicación Asertiva",     "¿Fomenta el diálogo abierto y directo para resolver diferencias o alinear expectativas?",       "Conductual",   3m),
        new Pregunta(16, "Enfoque en el Cliente",     "¿Entiende las necesidades del cliente (interno/externo) y busca superar sus expectativas?",      "Servicio",     4m),
        new Pregunta(17, "Enfoque en el Cliente",     "¿Procura la satisfacción del cliente brindando un servicio de excelencia?",                      "Servicio",     3m),
        new Pregunta(18, "Enfoque en el Cliente",     "¿Es percibido como una persona de confianza que representa con integridad a la empresa?",        "Servicio",     3m),
        new Pregunta(19, "Mejora Continua",           "¿Demuestra flexibilidad, rápida adaptación y disposición ante nuevos procesos o tecnologías?",   "Innovación",   5m),
        new Pregunta(20, "Mejora Continua",           "¿Busca activamente nuevas formas de optimizar sus tareas e innovar en sus aportes?",             "Innovación",   5m),
    };

    /// <summary>
    /// Calcula el puntaje final (0-100) según la fórmula del FALTANTE.md:
    /// Puntaje = Σ (calificacion_i / 5) × peso_i
    /// calificaciones: diccionario { numeroPregunta → calificacion (1-5) }
    /// </summary>
    public static decimal CalcularPuntaje(IReadOnlyDictionary<int, int> calificaciones)
    {
        decimal puntaje = 0m;
        foreach (var pregunta in Catalogo)
        {
            if (calificaciones.TryGetValue(pregunta.Numero, out var cal))
            {
                puntaje += (cal / 5m) * pregunta.Peso;
            }
        }
        return Math.Round(puntaje, 2);
    }
}
