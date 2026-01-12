using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;
    public Transform pointD;

    public float moveSpeed = 2.0f;
    public float rotationSpeed = 8f;

    public Vector3 visualOffset = new Vector3(0, 0.9f, 0);  // ← DÉCALAGE AVEC Y=0.9 pour relever

    private NavMeshAgent agent;
    private Animator anim;
    private Transform visualRoot;

    private Transform[] points;
    private int currentIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // Trouvez le modèle visuel (mixamorigHips)
        visualRoot = transform.Find("mixamorigHips");
        if (visualRoot == null)
        {
            // Cherchez dans tous les enfants
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
        agent.stoppingDistance = 0.1f;

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        // Liste des points
        points = new Transform[] { pointA, pointB, pointC, pointD };

        currentIndex = 0;
        agent.SetDestination(points[currentIndex].position);
    }

    void LateUpdate()
    {
        // Animation
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f;
        
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
        }

        // CORRECTION FORCÉE DU DÉCALAGE
        if (visualRoot != null)
        {
            visualRoot.localPosition = visualOffset;
            visualRoot.localRotation = Quaternion.identity;
        }

        // Arrivé au point → suivant
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentIndex = (currentIndex + 1) % points.Length;
            agent.SetDestination(points[currentIndex].position);
        }
    }
}