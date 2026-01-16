using UnityEngine;
using System.Collections;

public class VRDeathEffect : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;
    public bool autoRespawn = true;
    
    [Header("Audio")]
    public AudioClip deathSound;
    
    private bool isDead = false;
    private AudioSource audioSource;
    private BlackFadeTransition fade;
    private Camera mainCamera;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 1 = 3D spatial, 0 = stéréo
        audioSource.playOnAwake = false;
        audioSource.dopplerLevel = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        
        mainCamera = Camera.main;
        Debug.Log("✅ VRDeathEffect initialisé");
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        
        Debug.Log("💀 MORT DU JOUEUR !");
        isDead = true;
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        DisablePlayerMovement();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (fade != null)
            yield return StartCoroutine(fade.FadeIn());
        else
            yield return null;

        yield return new WaitForSeconds(respawnDelay);
        
        if (autoRespawn)
        {
            RespawnPlayer();
        }
        if (fade != null)
            StartCoroutine(fade.FadeOut());
    }

    void DisablePlayerMovement()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string name = script.GetType().Name;
            if (name.Contains("Movement") || name.Contains("Locomotion") || name.Contains("Teleport"))
            {
                script.enabled = false;
            }
        }
    }

    void EnablePlayerMovement()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string name = script.GetType().Name;
            if (name.Contains("Movement") || name.Contains("Locomotion") || name.Contains("Teleport"))
            {
                script.enabled = true;
            }
        }
    }

    public void RespawnPlayer()
    {
        Debug.Log("🔄 RESPAWN du joueur");
        
        isDead = false;
        
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        
        EnablePlayerMovement();
        
        BossAI boss = FindObjectOfType<BossAI>();
        if (boss != null)
        {
            boss.ResetBossAfterRespawn();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}