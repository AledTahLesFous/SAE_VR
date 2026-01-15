using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("Points de Patrouille")]
    public Transform point_0;
    public Transform point_1;
    public Transform point_2;
    public Transform point_3;
    public Transform point_4;
    public Transform point_5;
    public Transform point_6;
    public Transform point_7;
    public Transform point_8;
    public Transform point_9;
    public Transform point_10;
    public Transform point_11;
    public Transform point_12;
    public Transform point_13;
    public Transform point_14;
    public Transform point_15;

    [Header("Paramètres de Mouvement")]
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 8f;
    public Vector3 visualOffset = new Vector3(0, 0.9f, 0);
    public float minVelocityToWalk = 0.3f;

    [Header("Paramètres de Vision")]
    public float visionRange = 6f; 
    public float visionAngle = 45f; 
    public LayerMask obstacleLayer; 
    public bool debugRaycast = true; 
    
    [Header("Paramètres de Poursuite")]
    public float chaseSpeed = 4f;
    public float chaseTimeBeforeGiveUp = 3f; 

    [Header("Animation aux Points")]
    public string pointAnimationTrigger = "PointAction";
    public float animationDuration = 2f;
    public float pauseTimeAtPoint = 0f;

    [Header("Audio du Boss")]
    public AudioClip grognementSound;
    public float grognementVolume = 0.8f;
    public AudioClip pasSound;
    public float pasVolume = 0.5f;
    public AudioClip attaqueSound;
    public float attaqueVolume = 1f;

    [Header("Visualisation (visible en jeu)")]
    public bool showVisionInGame = true;
    public Color patrolVisionColor = Color.cyan;
    public Color chaseVisionColor = Color.red;

    [Header("Lumière du Boss")]
    public Light bossLight;
    public Color patrolLightColor = Color.white;
    public Color chaseLightColor = Color.red;

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSourceGrognement;
    private AudioSource audioSourcePas;
    private AudioSource audioSourceAttaque;
    private Transform visualRoot;
    private Transform[] points;
    private int currentIndex = 0;

    private enum BossState { Patrol, Chase }
    private BossState currentState = BossState.Patrol;
    private BossState previousState = BossState.Patrol;
    
    private Transform vrCamera;
    private float chaseTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasPlayedAnimationAtPoint = false;
    private bool isPlayingPointAnimation = false;
    private float animationTimer = 0f;
    private bool wasMovingLastFrame = false;
    private bool hasPlayedAttackSound = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        
        // Récupère tous les Audio Sources
        AudioSource[] audioSources = GetComponents<AudioSource>();
        
        // Si on a déjà 3 Audio Sources
        if (audioSources.Length >= 3)
        {
            audioSourceGrognement = audioSources[0];
            audioSourcePas = audioSources[1];
            audioSourceAttaque = audioSources[2];
            Debug.Log("✅ 3 Audio Sources existants trouvés");
        }
        // Si on a 2 Audio Sources, on en crée un troisième
        else if (audioSources.Length == 2)
        {
            audioSourceGrognement = audioSources[0];
            audioSourcePas = audioSources[1];
            audioSourceAttaque = gameObject.AddComponent<AudioSource>();
            Debug.Log("✅ Audio Source pour l'attaque créé");
        }
        // Si on a qu'un seul Audio Source, on en crée deux de plus
        else if (audioSources.Length == 1)
        {
            audioSourceGrognement = audioSources[0];
            audioSourcePas = gameObject.AddComponent<AudioSource>();
            audioSourceAttaque = gameObject.AddComponent<AudioSource>();
            Debug.Log("✅ Audio Sources pour les pas et l'attaque créés");
        }
        // Sinon on en crée 3
        else
        {
            audioSourceGrognement = gameObject.AddComponent<AudioSource>();
            audioSourcePas = gameObject.AddComponent<AudioSource>();
            audioSourceAttaque = gameObject.AddComponent<AudioSource>();
            Debug.Log("✅ 3 Audio Sources créés");
        }

        // Configuration de l'Audio Source pour le grognement (aux points)
        if (audioSourceGrognement != null && grognementSound != null)
        {
            audioSourceGrognement.clip = grognementSound;
            audioSourceGrognement.loop = false;
            audioSourceGrognement.playOnAwake = false;
            audioSourceGrognement.volume = grognementVolume;
            Debug.Log("✅ Audio grognement configuré");
        }

        // Configuration de l'Audio Source pour les pas (en marchant)
        if (audioSourcePas != null && pasSound != null)
        {
            audioSourcePas.clip = pasSound;
            audioSourcePas.loop = true;
            audioSourcePas.playOnAwake = false;
            audioSourcePas.volume = pasVolume;
            Debug.Log("✅ Audio pas configuré");
        }

        // Configuration de l'Audio Source pour l'attaque (en poursuite)
        if (audioSourceAttaque != null && attaqueSound != null)
        {
            audioSourceAttaque.clip = attaqueSound;
            audioSourceAttaque.loop = true; // EN BOUCLE pendant la poursuite
            audioSourceAttaque.playOnAwake = false;
            audioSourceAttaque.volume = attaqueVolume;
            Debug.Log("✅ Audio attaque configuré");
        }
        else if (audioSourceAttaque == null)
        {
            Debug.LogWarning("⚠️ Audio Source pour l'attaque non trouvé !");
        }
        else if (attaqueSound == null)
        {
            Debug.LogWarning("⚠️ Aucun son d'attaque assigné !");
        }

        visualRoot = transform.Find("mixamorigHips");
        if (visualRoot == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("mixamorig") || child.name.Contains("Hips"))
                {
                    visualRoot = child;
                    break;
                }
            }
        }

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.speed = moveSpeed;
        agent.acceleration = 3f;
        agent.angularSpeed = 120f;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.2f;
        agent.radius = 0.25f;

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            vrCamera = mainCam.transform;
            Debug.Log("✅ Caméra VR trouvée : " + vrCamera.name);
        }
        else
        {
            Debug.LogError("❌ Aucune caméra VR trouvée !");
        }

        if (obstacleLayer == 0)
        {
            Debug.LogWarning("⚠️ ATTENTION : obstacleLayer n'est pas configuré ! Le boss verra à travers les murs.");
        }

        points = new Transform[] { 
            point_0, point_1, point_2, point_3, point_4, point_5,
            point_6, point_7, point_8, point_9, point_10, point_11,
            point_12, point_13, point_14, point_15
        };

        System.Array.Sort(points, (a, b) => {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            
            int numA = ExtractPointNumber(a.name);
            int numB = ExtractPointNumber(b.name);
            return numA.CompareTo(numB);
        });

        currentIndex = 0;
        if (points[currentIndex] != null)
        {
            agent.SetDestination(points[currentIndex].position);
        }
    }

    int ExtractPointNumber(string name)
    {
        string[] parts = name.Split('_');
        if (parts.Length > 1)
        {
            string lastPart = parts[parts.Length - 1];
            if (int.TryParse(lastPart, out int number))
            {
                return number;
            }
        }
        return 999;
    }

    void Update()
    {
        if (bossLight != null && currentState != previousState)
        {
            bossLight.color = (currentState == BossState.Chase) ? chaseLightColor : patrolLightColor;
            previousState = currentState;
        }

        CheckVision();

        switch (currentState)
        {
            case BossState.Patrol:
                Patrol();
                break;
            case BossState.Chase:
                ChasePlayer();
                break;
        }
    }

    void CheckVision()
    {
        if (vrCamera == null) return;

        Vector3 directionToPlayer = vrCamera.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= visionRange)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer <= visionAngle / 2f)
            {
                Vector3 rayStart = transform.position + Vector3.up * 1.5f;
                Vector3 rayDirection = (vrCamera.position - rayStart).normalized;
                
                bool hasObstacle = Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, distanceToPlayer, obstacleLayer);
                
                if (debugRaycast)
                {
                    if (hasObstacle)
                    {
                        Debug.DrawLine(rayStart, hit.point, Color.yellow);
                        Debug.Log("🧱 Obstacle détecté : " + hit.collider.gameObject.name + " (Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) + ")");
                    }
                    else
                    {
                        Debug.DrawLine(rayStart, vrCamera.position, Color.green);
                    }
                }

                if (hasObstacle)
                {
                    if (currentState == BossState.Chase)
                    {
                        chaseTimer += Time.deltaTime;
                    }
                }
                else
                {
                    if (currentState != BossState.Chase)
                    {
                        Debug.Log("👁️ JOUEUR VR DÉTECTÉ ! POURSUITE ACTIVÉE !");
                        currentState = BossState.Chase;
                        agent.speed = chaseSpeed;
                        agent.acceleration = 3f;
                        hasPlayedAttackSound = false; // Reset pour jouer le son d'attaque
                    }
                    chaseTimer = 0f;
                    lastKnownPlayerPosition = vrCamera.position;
                }
            }
            else if (currentState == BossState.Chase)
            {
                chaseTimer += Time.deltaTime;
            }
        }
        else if (currentState == BossState.Chase)
        {
            chaseTimer += Time.deltaTime;
        }

        if (currentState == BossState.Chase && chaseTimer >= chaseTimeBeforeGiveUp)
        {
            Debug.Log("🤷 Joueur perdu, retour à la patrouille");
            currentState = BossState.Patrol;
            agent.speed = moveSpeed;
            agent.acceleration = 3f;
            
            // ARRÊTE LE SON D'ATTAQUE
            if (audioSourceAttaque != null && audioSourceAttaque.isPlaying)
            {
                audioSourceAttaque.Stop();
                Debug.Log("🛑 Son d'attaque arrêté");
            }
            
            float minDist = float.MaxValue;
            int closestIndex = 0;
            
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    float dist = Vector3.Distance(transform.position, points[i].position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestIndex = i;
                    }
                }
            }
            
            currentIndex = closestIndex;
            agent.SetDestination(points[currentIndex].position);
            Debug.Log("🔄 Reprend la patrouille au point " + currentIndex);
        }
    }

    void Patrol()
    {
        if (isPlayingPointAnimation)
        {
            agent.isStopped = true;
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationDuration + pauseTimeAtPoint)
            {
                isPlayingPointAnimation = false;
                animationTimer = 0f;
                agent.isStopped = false;
                
                currentIndex = (currentIndex + 1) % points.Length;
                
                if (points[currentIndex] != null)
                {
                    agent.SetDestination(points[currentIndex].position);
                    hasPlayedAnimationAtPoint = false;
                    Debug.Log("🚶 Va au point " + currentIndex + " : " + points[currentIndex].name);
                }
            }
            return;
        }

        agent.speed = moveSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!hasPlayedAnimationAtPoint && anim != null && !string.IsNullOrEmpty(pointAnimationTrigger))
            {
                anim.SetTrigger(pointAnimationTrigger);
                hasPlayedAnimationAtPoint = true;
                isPlayingPointAnimation = true;
                
                // JOUE LE SON DE GROGNEMENT ICI
                if (audioSourceGrognement != null && grognementSound != null)
                {
                    audioSourceGrognement.PlayOneShot(grognementSound, grognementVolume);
                    Debug.Log("🔊 Grognement joué au point " + currentIndex);
                }
                
                Debug.Log("🎬 Animation jouée au point " + currentIndex + " - BOSS ARRÊTÉ");
            }
        }
    }

    void ChasePlayer()
    {
        // JOUE LE SON D'ATTAQUE (UNE SEULE FOIS au début de la poursuite)
        if (!hasPlayedAttackSound && audioSourceAttaque != null && attaqueSound != null)
        {
            audioSourceAttaque.Play();
            hasPlayedAttackSound = true;
            Debug.Log("⚔️ Son d'attaque démarré !");
        }
        
        if (vrCamera != null)
        {
            agent.SetDestination(vrCamera.position);
            
            if (agent.speed != chaseSpeed)
            {
                agent.speed = chaseSpeed;
            }
            
            Debug.DrawLine(transform.position, vrCamera.position, Color.red);
        }
        else
        {
            agent.SetDestination(lastKnownPlayerPosition);
        }
    }

    void LateUpdate()
    {
        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0, agent.velocity.z);
        float speed = horizontalVelocity.magnitude;
        
        bool isMoving = speed > minVelocityToWalk;
        
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
        }

        // GESTION DU SON DES PAS (seulement en PATROUILLE)
        if (currentState == BossState.Patrol)
        {
            if (audioSourcePas != null && pasSound != null)
            {
                if (isMoving && !wasMovingLastFrame)
                {
                    audioSourcePas.Play();
                    Debug.Log("👟 Son des pas démarré");
                }
                else if (!isMoving && wasMovingLastFrame)
                {
                    audioSourcePas.Stop();
                    Debug.Log("🛑 Son des pas arrêté");
                }
            }
        }
        else if (currentState == BossState.Chase)
        {
            // Arrête les pas pendant la poursuite (on a le son d'attaque)
            if (audioSourcePas != null && audioSourcePas.isPlaying)
            {
                audioSourcePas.Stop();
            }
        }
        
        wasMovingLastFrame = isMoving;

        if (visualRoot != null)
        {
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
        }

        if (showVisionInGame)
        {
            DrawVisionCone();
        }
    }

    void DrawVisionCone()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Color lineColor = currentState == BossState.Chase ? chaseVisionColor : patrolVisionColor;

        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * visionRange;

        Debug.DrawLine(eyePosition, eyePosition + leftBoundary, lineColor);
        Debug.DrawLine(eyePosition, eyePosition + rightBoundary, lineColor);

        Vector3 prevPoint = eyePosition + leftBoundary;
        for (int i = 1; i <= 30; i++)
        {
            float angle = -visionAngle / 2f + (visionAngle / 30f) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward * visionRange;
            Vector3 point = eyePosition + direction;
            Debug.DrawLine(prevPoint, point, lineColor);
            prevPoint = point;
        }

        for (int i = 0; i <= 6; i++)
        {
            float angle = -visionAngle / 2f + (visionAngle / 6f) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward * visionRange;
            Debug.DrawLine(eyePosition, eyePosition + direction, lineColor);
        }

        if (vrCamera != null && currentState == BossState.Chase)
        {
            Debug.DrawLine(eyePosition, vrCamera.position, Color.red);
        }

        Vector3 centerDirection = transform.forward * visionRange;
        Debug.DrawLine(eyePosition, eyePosition + centerDirection, Color.green);
    }

    void OnDrawGizmos()
    {
        if (points != null && points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(points[i].position, 0.3f);
                    
                    if (i < points.Length - 1 && points[i + 1] != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(points[i].position, points[i + 1].position);
                    }
                    else if (i == points.Length - 1 && points[0] != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(points[i].position, points[0].position);
                    }
                }
            }
            
            if (Application.isPlaying && currentIndex < points.Length && points[currentIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(points[currentIndex].position, 0.5f);
            }
        }

        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        
        Color visionColor = Color.blue;
        if (Application.isPlaying && currentState == BossState.Chase)
        {
            visionColor = Color.red;
        }
        visionColor.a = 0.2f;
        Gizmos.color = visionColor;

        Gizmos.DrawWireSphere(eyePosition, visionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * visionRange;

        Gizmos.color = visionColor;
        Gizmos.DrawLine(eyePosition, eyePosition + leftBoundary);
        Gizmos.DrawLine(eyePosition, eyePosition + rightBoundary);

        Vector3 prevPoint = eyePosition + leftBoundary;
        for (int i = 1; i <= 30; i++)
        {
            float angle = -visionAngle / 2f + (visionAngle / 30f) * i;
            Vector3 point = eyePosition + Quaternion.Euler(0, angle, 0) * transform.forward * visionRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        if (Application.isPlaying && vrCamera != null && currentState == BossState.Chase)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition, vrCamera.position);
            Gizmos.DrawSphere(vrCamera.position, 0.3f);
        }

        Gizmos.color = Color.green;
        Vector3 forward = transform.forward * 2f;
        Gizmos.DrawLine(eyePosition, eyePosition + forward);
        Gizmos.DrawSphere(eyePosition + forward, 0.1f);
    }
}