using BaseLunar.App.Vistas;

namespace BaseLunar.App.Animacion;

/// <summary>
/// Posturas y ciclos del astronauta: reposo, caminar, saludar, empujar, regar, refugiarse, celebrar y
/// tiritar. Los ángulos son en grados: un brazo a 90° (izquierdo) o −90° (derecho) apunta hacia su lado.
/// </summary>
internal static class Posturas
{
    private const double Paso = 420;

    /// <summary>Camina hasta un desplazamiento horizontal: piernas y brazos se balancean y el cuerpo rebota.</summary>
    public static void Caminar(Coreografia c, Astronauta a, double hastaX, double inicio, double duracion)
    {
        c.Hacia(a.Cuerpo, Eje.X, hastaX, inicio, duracion, null);
        c.Oscilar(a.PiernaIzq, Eje.Giro, 18, Paso, inicio, duracion);
        c.Oscilar(a.PiernaDer, Eje.Giro, -18, Paso, inicio, duracion);
        c.Oscilar(a.BrazoIzq, Eje.Giro, -14, Paso, inicio, duracion);
        c.Oscilar(a.BrazoDer, Eje.Giro, 14, Paso, inicio, duracion);
        c.Oscilar(a.Cuerpo, Eje.Y, -5, Paso / 2, inicio, duracion);
    }

    /// <summary>Lleva los brazos a un ángulo (izquierdo positivo hacia fuera, derecho negativo hacia fuera).</summary>
    public static void Brazos(Coreografia c, Astronauta a, double izquierdo, double derecho, double inicio, double duracion = 280)
    {
        c.Hacia(a.BrazoIzq, Eje.Giro, izquierdo, inicio, duracion);
        c.Hacia(a.BrazoDer, Eje.Giro, derecho, inicio, duracion);
    }

    /// <summary>Saluda con el brazo derecho en alto.</summary>
    public static void Saludar(Coreografia c, Astronauta a, double inicio, double duracion)
    {
        c.Hacia(a.BrazoDer, Eje.Giro, -150, inicio, 280);
        c.Oscilar(a.BrazoDer, Eje.Giro, 18, 360, inicio + 280, duracion - 560);
        c.Hacia(a.BrazoDer, Eje.Giro, 0, inicio + duracion - 280, 280);
    }

    /// <summary>Se agacha con los brazos sobre el casco (refugio).</summary>
    public static void Agacharse(Coreografia c, Astronauta a, double inicio, double duracion = 320)
    {
        c.Hacia(a.Cuerpo, Eje.EscalaY, 0.84, inicio, duracion);
        c.Hacia(a.PiernaIzq, Eje.Giro, 14, inicio, duracion);
        c.Hacia(a.PiernaDer, Eje.Giro, -14, inicio, duracion);
        Brazos(c, a, 155, -155, inicio, duracion);
    }

    /// <summary>Celebra: brazos en alto y saltos.</summary>
    public static void Celebrar(Coreografia c, Astronauta a, double inicio, int saltos = 3)
    {
        Brazos(c, a, 145, -145, inicio);
        for (var i = 0; i < saltos; i++)
        {
            var t = inicio + 200 + i * 560;
            c.Hacia(a.Cuerpo, Eje.Y, -46, t, 260);
            c.Hacia(a.Cuerpo, Eje.Y, 0, t + 260, 260);
        }
    }

    /// <summary>Tirita de frío (fallo de batería): un temblor corto de lado a lado.</summary>
    public static void Tiritar(Coreografia c, Astronauta a, double inicio, double duracion = 900) =>
        c.Oscilar(a.Cuerpo, Eje.X, 3, 90, inicio, duracion);
}
