#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RestaurarEscena : MonoBehaviour
{
    [MenuItem("Herramientas/Reconstruir Escena Completa")]
    public static void Reconstruir()
    {
        // 1. Crear Escenario
        GameObject escenario = new GameObject("Escenario");

        // Pista
        GameObject pista = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pista.name = "Pista";
        pista.transform.parent = escenario.transform;
        pista.transform.position = new Vector3(0, -0.5f, 10);
        pista.transform.localScale = new Vector3(12, 1, 30);

        // Línea Salida
        GameObject lSalida = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lSalida.name = "LineaSalida";
        lSalida.transform.parent = escenario.transform;
        lSalida.transform.position = new Vector3(0, 0.01f, 0);
        lSalida.transform.localScale = new Vector3(12, 0.02f, 0.4f);

        // Línea Meta
        GameObject lMeta = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lMeta.name = "LineaMeta";
        lMeta.transform.parent = escenario.transform;
        lMeta.transform.position = new Vector3(0, 0.01f, 20);
        lMeta.transform.localScale = new Vector3(12, 0.02f, 0.4f);
        Material matMeta = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matMeta.color = Color.red;
        lMeta.GetComponent<Renderer>().material = matMeta;

        // Muros
        GameObject mIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mIzq.name = "MuroIzq";
        mIzq.transform.parent = escenario.transform;
        mIzq.transform.position = new Vector3(-6, 2, 10);
        mIzq.transform.localScale = new Vector3(0.5f, 5, 30);

        GameObject mDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mDer.name = "MuroDer";
        mDer.transform.parent = escenario.transform;
        mDer.transform.position = new Vector3(6, 2, 10);
        mDer.transform.localScale = new Vector3(0.5f, 5, 30);

        GameObject mFondo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mFondo.name = "MuroFondo";
        mFondo.transform.parent = escenario.transform;
        mFondo.transform.position = new Vector3(0, 2, 25);
        mFondo.transform.localScale = new Vector3(12.5f, 5, 0.5f);

        // 2. Semáforo
        GameObject poste = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        poste.name = "Poste";
        poste.transform.parent = escenario.transform;
        poste.transform.position = new Vector3(0, 1.5f, 23);
        poste.transform.localScale = new Vector3(0.6f, 3, 0.6f);

        GameObject semaforo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        semaforo.name = "Semaforo";
        semaforo.transform.parent = escenario.transform;
        semaforo.transform.position = new Vector3(0, 3.2f, 23);
        semaforo.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Material matSemaforo = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matSemaforo.color = Color.red;
        semaforo.GetComponent<Renderer>().material = matSemaforo;

        // 3. Jugadores
        GameObject prefabJ1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Distant Lands/Free Characters/Contents/Prefabs/Male 1.prefab") 
                              ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Distant Lands/Free Characters/Prefabs/Male 1.prefab");
        GameObject prefabJ2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Distant Lands/Free Characters/Contents/Prefabs/Male 2.prefab")
                              ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Distant Lands/Free Characters/Prefabs/Male 2.prefab");

        GameObject j1 = prefabJ1 != null ? (GameObject)PrefabUtility.InstantiatePrefab(prefabJ1) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        j1.name = "Jugador1";
        j1.transform.position = new Vector3(-2, 0, 0);
        j1.transform.rotation = Quaternion.identity;

        GameObject j2 = prefabJ2 != null ? (GameObject)PrefabUtility.InstantiatePrefab(prefabJ2) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        j2.name = "Jugador2";
        j2.transform.position = new Vector3(2, 0, 0);
        j2.transform.rotation = Quaternion.identity;

        // Controller de Animación
        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Scripts/LuzRojaAnimator.controller");
        if (animController != null)
        {
            if (j1.GetComponent<Animator>() != null) j1.GetComponent<Animator>().runtimeAnimatorController = animController;
            if (j2.GetComponent<Animator>() != null) j2.GetComponent<Animator>().runtimeAnimatorController = animController;
        }

        // 4. GameManager
        GameObject gm = new GameObject("GameManager");
        AudioSource audioSource = gm.AddComponent<AudioSource>();
        LuzRojaLuzVerde script = gm.AddComponent<LuzRojaLuzVerde>();

        script.jugador1 = j1.transform;
        script.jugador2 = j2.transform;
        script.animatorJugador1 = j1.GetComponent<Animator>();
        script.animatorJugador2 = j2.GetComponent<Animator>();
        script.semaforo = semaforo.GetComponent<Renderer>();
        script.posicionMeta = 20f;
        script.reproductorSFX = audioSource;

        // Carga de Audios
        script.audioLuzVerde = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/doll-green-light.mp3");
        script.audioLuzRoja = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/doll-red-light.mp3");
        script.sonidoAtrapado = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/disparo-destructor.mp3");
        script.sonidoVictoria = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ff7_victory_QyN4ZfS.mp3");

        // 5. Ajustar Cámara Principal
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 4.5f, -6f);
            cam.transform.rotation = Quaternion.Euler(18f, 0, 0);
        }

        Debug.Log("¡Escena reconstruida y configurada exitosamente!");
    }
}
#endif