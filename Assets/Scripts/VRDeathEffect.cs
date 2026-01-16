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

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        
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
        yield return new WaitForSeconds(respawnDelay);
        
        if (autoRespawn)
        {
            RespawnPlayer();
        }
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