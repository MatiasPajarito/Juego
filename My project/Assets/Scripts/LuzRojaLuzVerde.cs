using UnityEngine;
using System.Collections;

public class LuzRojaLuzVerde : MonoBehaviour
{
    [Header("Jugadores")]
    public Transform jugador1;
    public Transform jugador2;
    public Animator animatorJugador1;
    public Animator animatorJugador2;
    public float velocidadAvance = 4f;
    [Tooltip("Coordenada Z donde está la línea de llegada")]
    public float posicionMeta = 20f;

    [Header("Semáforo")]
    public Renderer semaforo;
    public Color colorVerde = Color.green;
    public Color colorRojo = Color.red;
    public float tiempoVerdeMin = 2.5f;
    public float tiempoVerdeMax = 4.5f;
    public float tiempoRojoMin = 2.0f;
    public float tiempoRojoMax = 3.5f;

    [Header("Audio SFX & Voces")]
    public AudioSource reproductorSFX;
    public AudioClip audioLuzVerde;
    public AudioClip audioLuzRoja;
    public AudioClip sonidoAtrapado;
    public AudioClip sonidoVictoria;

    // Estados internos
    private bool esperandoInicio = true;
    private bool luzVerde = false;
    private bool juegoTerminado = false;
    private Vector3 posInicialJ1, posInicialJ2;
    private Quaternion rotInicialJ1, rotInicialJ2;
    private Material semaforoMat;
    private Coroutine rutinaSemaforo;

    // Control de mensajes temporales en pantalla
    private string mensajeHUD = "";
    private Color colorHUD = Color.white;
    private float tiempoOcultarHUD = 0f;

    // Resultados finales
    private string tituloFinal = "";
    private string detalleFinal = "";

    // Textura generada por código para fondos oscuros
    private Texture2D texturaFondoNegro;

    void Start()
    {
        if (jugador1 != null)
        {
            posInicialJ1 = jugador1.position;
            // Se captura la rotación real que el personaje tiene en el editor
            // (la que tú le hayas puesto manualmente para que mire hacia adelante),
            // en vez de forzarla a 0 sin importar cómo lo hayas orientado.
            rotInicialJ1 = jugador1.rotation;
        }
        if (jugador2 != null)
        {
            posInicialJ2 = jugador2.position;
            rotInicialJ2 = jugador2.rotation;
        }

        if (semaforo != null)
            semaforoMat = semaforo.material;

        texturaFondoNegro = new Texture2D(1, 1);
        texturaFondoNegro.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.82f));
        texturaFondoNegro.Apply();
    }

    void Update()
    {
        // 1. Pantalla previa de inicio
        if (esperandoInicio)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                esperandoInicio = false;
                IniciarRonda();
            }
            return;
        }

        // 2. Partida terminada
        if (juegoTerminado)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IniciarRonda();
            }
            return;
        }

        // 3. Controles
        bool j1Avanza = Input.GetKey(KeyCode.W);
        bool j2Avanza = Input.GetKey(KeyCode.UpArrow);

        if (animatorJugador1 != null) animatorJugador1.SetBool("Avanzando", j1Avanza);
        if (animatorJugador2 != null) animatorJugador2.SetBool("Avanzando", j2Avanza);

        // Infracción en Luz Roja
        if (!luzVerde)
        {
            if (j1Avanza) { ProcesarDerrota(1, "¡Jugador 1 se movió durante la Luz Roja!"); return; }
            if (j2Avanza) { ProcesarDerrota(2, "¡Jugador 2 se movió durante la Luz Roja!"); return; }
        }
        else
        {
            // Movimiento estrictamente sobre el eje Z
            if (j1Avanza && jugador1 != null)
                jugador1.position += Vector3.forward * velocidadAvance * Time.deltaTime;

            if (j2Avanza && jugador2 != null)
                jugador2.position += Vector3.forward * velocidadAvance * Time.deltaTime;
        }

        // Cruce de meta
        if (jugador1 != null && jugador1.position.z >= posicionMeta) { ProcesarVictoria(1, "¡Jugador 1 cruzó la meta primero!"); return; }
        if (jugador2 != null && jugador2.position.z >= posicionMeta) { ProcesarVictoria(2, "¡Jugador 2 cruzó la meta primero!"); return; }
    }

    void LateUpdate()
    {
        // Ya NO se recalcula el ángulo con una fórmula/slider: se mantiene fija
        // la rotación que cada personaje tenía en el editor (capturada en Start()).
        // Esto evita que el Animator o alguna animación de mocap los "tuerza",
        // pero respeta hacia dónde tú los hayas orientado manualmente en la escena.
        if (jugador1 != null)
        {
            jugador1.rotation = rotInicialJ1;
            jugador1.position = new Vector3(posInicialJ1.x, jugador1.position.y, jugador1.position.z);
        }

        if (jugador2 != null)
        {
            jugador2.rotation = rotInicialJ2;
            jugador2.position = new Vector3(posInicialJ2.x, jugador2.position.y, jugador2.position.z);
        }
    }

    IEnumerator CicloSemaforo()
    {
        while (!juegoTerminado)
        {
            // VERDE
            luzVerde = true;
            ActualizarVisualSemaforo(colorVerde);
            MostrarHUDTemporal("¡LUZ VERDE! (AVANZA)", Color.green, 1.6f);

            if (reproductorSFX != null && audioLuzVerde != null)
                reproductorSFX.PlayOneShot(audioLuzVerde);

            yield return new WaitForSeconds(Random.Range(tiempoVerdeMin, tiempoVerdeMax));

            if (juegoTerminado) yield break;

            // ROJO
            luzVerde = false;
            ActualizarVisualSemaforo(colorRojo);
            MostrarHUDTemporal("¡LUZ ROJA! (DETENTE)", Color.red, 1.6f);

            if (reproductorSFX != null && audioLuzRoja != null)
                reproductorSFX.PlayOneShot(audioLuzRoja);

            yield return new WaitForSeconds(Random.Range(tiempoRojoMin, tiempoRojoMax));
        }
    }

    void ActualizarVisualSemaforo(Color c)
    {
        if (semaforoMat != null)
        {
            if (semaforoMat.HasProperty("_BaseColor"))
                semaforoMat.SetColor("_BaseColor", c);
            else
                semaforoMat.color = c;
        }
    }

    void MostrarHUDTemporal(string texto, Color c, float duracion)
    {
        mensajeHUD = texto;
        colorHUD = c;
        tiempoOcultarHUD = Time.time + duracion;
    }

    void ProcesarDerrota(int perdedor, string motivo)
    {
        juegoTerminado = true;
        if (rutinaSemaforo != null) StopCoroutine(rutinaSemaforo);

        if (reproductorSFX != null && sonidoAtrapado != null)
            reproductorSFX.PlayOneShot(sonidoAtrapado);

        if (perdedor == 1)
        {
            if (animatorJugador1 != null) animatorJugador1.SetTrigger("Morir");
            if (animatorJugador2 != null) animatorJugador2.SetTrigger("Victoria");
            tituloFinal = "¡VICTORIA DEL JUGADOR 2!";
            detalleFinal = motivo;
        }
        else
        {
            if (animatorJugador2 != null) animatorJugador2.SetTrigger("Morir");
            if (animatorJugador1 != null) animatorJugador1.SetTrigger("Victoria");
            tituloFinal = "¡VICTORIA DEL JUGADOR 1!";
            detalleFinal = motivo;
        }
    }

    void ProcesarVictoria(int ganador, string motivo)
    {
        juegoTerminado = true;
        if (rutinaSemaforo != null) StopCoroutine(rutinaSemaforo);

        if (reproductorSFX != null && sonidoVictoria != null)
            reproductorSFX.PlayOneShot(sonidoVictoria);

        if (ganador == 1)
        {
            if (animatorJugador1 != null) animatorJugador1.SetTrigger("Victoria");
            tituloFinal = "¡VICTORIA DEL JUGADOR 1!";
            detalleFinal = motivo;
        }
        else
        {
            if (animatorJugador2 != null) animatorJugador2.SetTrigger("Victoria");
            tituloFinal = "¡VICTORIA DEL JUGADOR 2!";
            detalleFinal = motivo;
        }
    }

    void IniciarRonda()
    {
        juegoTerminado = false;
        mensajeHUD = "";
        if (rutinaSemaforo != null) StopCoroutine(rutinaSemaforo);

        if (jugador1 != null)
        {
            jugador1.position = posInicialJ1;
            jugador1.rotation = rotInicialJ1;
        }
        if (jugador2 != null)
        {
            jugador2.position = posInicialJ2;
            jugador2.rotation = rotInicialJ2;
        }

        rutinaSemaforo = StartCoroutine(CicloSemaforo());
    }

    void OnGUI()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        if (esperandoInicio)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), texturaFondoNegro);

            GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(sh * 0.055f), 28, 55),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.1f) }
            };
            GUI.Label(new Rect(0, sh * 0.22f, sw, sh * 0.12f), "LUZ ROJA, LUZ VERDE", estiloTitulo);

            GUIStyle estiloInfo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(sh * 0.03f), 16, 28),
                normal = { textColor = Color.white }
            };

            string textoControles = "JUGADOR 1: Mantener [W]\nJUGADOR 2: Mantener [FLECHA ARRIBA]\n\n" +
                                   "¡Avanza en VERDE y detente inmediatamente en ROJO!\n\n" +
                                   "Presiona [ESPACIO] para comenzar";

            GUI.Label(new Rect(sw * 0.1f, sh * 0.38f, sw * 0.8f, sh * 0.45f), textoControles, estiloInfo);
            return;
        }

        if (!juegoTerminado && Time.time < tiempoOcultarHUD && !string.IsNullOrEmpty(mensajeHUD))
        {
            GUIStyle estiloHUD = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(sh * 0.045f), 24, 45),
                fontStyle = FontStyle.Bold,
                normal = { textColor = colorHUD }
            };

            GUIStyle estiloSombra = new GUIStyle(estiloHUD) { normal = { textColor = Color.black } };
            GUI.Label(new Rect(2, (sh * 0.08f) + 2, sw, sh * 0.08f), mensajeHUD, estiloSombra);
            GUI.Label(new Rect(0, sh * 0.08f, sw, sh * 0.08f), mensajeHUD, estiloHUD);
        }

        if (juegoTerminado)
        {
            float anchoPanel = Mathf.Min(sw * 0.75f, 750);
            float altoPanel = Mathf.Min(sh * 0.5f, 380);
            float xPanel = (sw - anchoPanel) * 0.5f;
            float yPanel = (sh - altoPanel) * 0.5f;

            GUI.DrawTexture(new Rect(xPanel, yPanel, anchoPanel, altoPanel), texturaFondoNegro);

            GUIStyle estiloGanador = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(sh * 0.05f), 24, 48),
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.1f) }
            };
            GUI.Label(new Rect(xPanel, yPanel + (altoPanel * 0.15f), anchoPanel, altoPanel * 0.25f), tituloFinal, estiloGanador);

            GUIStyle estiloDetalle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(sh * 0.028f), 15, 24),
                normal = { textColor = Color.white }
            };

            string textoReinicio = detalleFinal + "\n\nPresiona [ESPACIO] para jugar de nuevo";
            GUI.Label(new Rect(xPanel + 20, yPanel + (altoPanel * 0.45f), anchoPanel - 40, altoPanel * 0.45f), textoReinicio, estiloDetalle);
        }
    }
}