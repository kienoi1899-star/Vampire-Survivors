using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile that falls from sky and explodes on ground.
/// Creates AoE damage zone on impact.
/// </summary>
public class RainProjectile : Projectile
{
    public EnemyDamager damager;            // AoE damage zone template
    public float explosionRadius = 2f;      // Radius of explosion
    public float fallSpeed = 10f;           // Speed of falling
    public float damage = 10f;              // Damage amount
    
    private bool hasExploded = false;       // Did it already explode?
    private float fallCounter;              // Counter for destruction
    
    private GameObject aoeIndicator;        // Visual circle showing AoE area
    private CircleCollider2D indicatorCollider;

    void Start()
    {
        // Projectile falls with gravity
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().gravityScale = 1f;
            GetComponent<Rigidbody2D>().velocity = Vector2.down * fallSpeed;
        }
        
        fallCounter = 0f;
        
        // Create visual indicator circle
        CreateAoEIndicator();
    }
    
    /// <summary>
    /// Creates a visual circle showing where AoE will hit
    /// </summary>
    void CreateAoEIndicator()
    {
        aoeIndicator = new GameObject("AoE_Indicator");
        aoeIndicator.transform.SetParent(transform);
        aoeIndicator.transform.localPosition = Vector3.zero;
        
        // Add sprite renderer for circle
        SpriteRenderer spriteRenderer = aoeIndicator.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite(explosionRadius);
        spriteRenderer.color = new Color(1f, 0.5f, 0f, 0.4f);  // Orange, semi-transparent
        spriteRenderer.sortingOrder = -1;  // Behind projectile
        
        // Add collider for reference (visual only)
        CircleCollider2D collider = aoeIndicator.AddComponent<CircleCollider2D>();
        collider.radius = explosionRadius;
        collider.enabled = false;  // Just for visualization
        
        indicatorCollider = collider;
    }
    
    /// <summary>
    /// Creates a simple circle sprite for the AoE indicator
    /// </summary>
    Sprite CreateCircleSprite(float radius)
    {
        // Try to use a simple circle sprite if available
        // Otherwise, we'll create a simple visualization
        Texture2D circleTex = new Texture2D((int)(radius * 2), (int)(radius * 2), TextureFormat.RGBA32, false);
        
        for (int x = 0; x < circleTex.width; x++)
        {
            for (int y = 0; y < circleTex.height; y++)
            {
                float dx = x - radius;
                float dy = y - radius;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                // Draw circle border (thickness ~0.3)
                if (dist > radius - 0.3f && dist < radius + 0.3f)
                    circleTex.SetPixel(x, y, new Color(1, 0.5f, 0, 0.8f));
                else
                    circleTex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        
        circleTex.Apply();
        return Sprite.Create(circleTex, new Rect(0, 0, circleTex.width, circleTex.height), 
                            new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        // Check if hit ground or enemies
        fallCounter += Time.deltaTime;
        
        // Auto-explode after 5 seconds if not hit
        if (fallCounter > 5f && !hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Explode on hitting ground or anything solid
        if (!hasExploded && (collision.CompareTag("Ground") || collision.name.Contains("Ground")))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Remove AoE indicator
        if (aoeIndicator != null)
            Destroy(aoeIndicator);

        // Create explosion effect at current position
        if (damager != null)
        {
            EnemyDamager explosionZone = Instantiate(damager, transform.position, Quaternion.identity);
            explosionZone.transform.localScale = Vector3.one * explosionRadius;
            explosionZone.damageAmount = damage;
            explosionZone.gameObject.SetActive(true);
            
            // Add temporary visual effect for explosion
            CreateExplosionEffect(explosionZone);
            
            // Play explosion sound
            if (SFXManager.instance != null)
                SFXManager.instance.PlaySFXPitched(7);
        }

        // Destroy projectile
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Creates a temporary visual flash for explosion impact
    /// </summary>
    void CreateExplosionEffect(EnemyDamager explosionZone)
    {
        GameObject explosionVFX = new GameObject("Explosion_Flash");
        explosionVFX.transform.position = explosionZone.transform.position;
        explosionVFX.transform.SetParent(explosionZone.transform);
        
        // Add sprite renderer for flash effect
        SpriteRenderer flashRenderer = explosionVFX.AddComponent<SpriteRenderer>();
        flashRenderer.sprite = explosionZone.GetComponent<SpriteRenderer>()?.sprite;
        flashRenderer.color = new Color(1f, 0.8f, 0f, 0.6f);  // Yellow flash
        flashRenderer.sortingOrder = 0;
        
        // Scale up for visual impact
        explosionVFX.transform.localScale = Vector3.one * 1.3f;
        
        // Fade out effect
        StartCoroutine(FadeOutFlash(flashRenderer, 0.3f));
    }
    
    /// <summary>
    /// Coroutine to fade out explosion flash
    /// </summary>
    IEnumerator FadeOutFlash(SpriteRenderer renderer, float duration)
    {
        float elapsed = 0f;
        Color startColor = renderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        if (renderer.gameObject != null)
            Destroy(renderer.gameObject);
    }
}
