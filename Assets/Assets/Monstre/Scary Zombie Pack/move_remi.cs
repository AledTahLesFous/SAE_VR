using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
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

    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;
    public Vector3 visualOffset = new Vector3(0, 0.9f, 0);

    private NavMeshAgent agent;
    private Animator anim;
    private Transform visualRoot;
    private Transform[] points;
    private int currentIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

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

        // Configuration de l'agent
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.speed = moveSpeed;
        agent.acceleration = 10f;
        agent.angularSpeed = 120f;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.2f;
        agent.radius = 0.3f; // Réduit le radius pour passer les portes

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        // Liste des 14 points AVANT tri
        points = new Transform[] { 
            point_0, point_1, point_2, point_3, point_4, point_5,
            point_6, point_7, point_8, point_9, point_10, point_11,
            point_12, point_13
        };

        Debug.Log("=== ORDRE DES POINTS AVANT TRI ===");
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                Debug.Log("Index " + i + " → " + points[i].name + " (Position: " + points[i].position + ")");
            }
            else
            {
                Debug.LogError("Index " + i + " → NULL !");
            }
        }

        // 🔧 TRI AUTOMATIQUE par numéro dans le nom
        System.Array.Sort(points, (a, b) => {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            
            int numA = ExtractPointNumber(a.name);
            int numB = ExtractPointNumber(b.name);
            return numA.CompareTo(numB);
        });

        // VÉRIFICATION après tri
        Debug.Log("=== ORDRE DES POINTS APRÈS TRI ===");
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                Debug.Log("Index " + i + " → " + points[i].name + " (Position: " + points[i].position + ")");
            }
            else
            {
                Debug.LogError("Index " + i + " → NULL !");
            }
        }

        currentIndex = 0;
        if (points[currentIndex] != null)
        {
            agent.SetDestination(points[currentIndex].position);
            Debug.Log("🚀 DÉPART vers : " + points[currentIndex].name);
        }
    }

    // Fonction pour extraire le numéro d'un nom comme "Point_15" → 15
    int ExtractPointNumber(string name)
    {
        // Cherche le dernier underscore et prend le nombre après
        string[] parts = name.Split('_');
        if (parts.Length > 1)
        {
            string lastPart = parts[parts.Length - 1];
            if (int.TryParse(lastPart, out int number))
            {
                return number;
            }
        }
        
        // Si pas de format "Point_X", retourne un grand nombre pour le mettre à la fin
        Debug.LogWarning("Impossible d'extraire le numéro de : " + name);
        return 999;
    }

    void LateUpdate()
    {
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;
        
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
        }

        if (visualRoot != null)
        {
            visualRoot.localPosition = visualOffset;
            visualRoot.localRotation = Quaternion.identity;
        }

        // Changement de point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("✅ ARRIVÉ à : " + points[currentIndex].name + " (index " + currentIndex + ")");
            
            // Passage au point SUIVANT (boucle infinie)
            currentIndex = (currentIndex + 1) % points.Length;
            
            if (points[currentIndex] != null)
            {
                agent.SetDestination(points[currentIndex].position);
                Debug.Log("🎯 DIRECTION vers : " + points[currentIndex].name + " (index " + currentIndex + ")");
            }
        }
    }

    // Visualisation dans la Scene
    void OnDrawGizmos()
    {
        if (points != null && points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    // Numéro du point
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(points[i].position, 0.3f);
                    
                    // Flèche vers le suivant
                    if (i < points.Length - 1 && points[i + 1] != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(points[i].position, points[i + 1].position);
                        DrawArrow(points[i].position, points[i + 1].position);
                    }
                    else if (i == points.Length - 1 && points[0] != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(points[i].position, points[0].position);
                        DrawArrow(points[i].position, points[0].position);
                    }
                }
            }
            
            // Point actuel en ROUGE
            if (Application.isPlaying && currentIndex < points.Length && points[currentIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(points[currentIndex].position, 0.5f);
            }
        }
    }
    
    void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 mid = (from + to) / 2f;
        Vector3 right = Vector3.Cross(Vector3.up, direction) * 0.2f;
        
        Gizmos.DrawLine(mid, mid - direction * 0.3f + right);
        Gizmos.DrawLine(mid, mid - direction * 0.3f - right);
    }
}