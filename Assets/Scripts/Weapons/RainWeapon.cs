using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rain/Meteor weapon that spawns projectiles from above enemies.
/// Projectiles fall down and create AoE damage zones on landing.
/// </summary>
public class RainWeapon : Weapon
{
    public Projectile projectileToSpawn;      // Falling projectile prefab
    public EnemyDamager explosionDamager;     // AoE damage zone on impact
    
    private float shotCounter;                // Counter for shot timing
    public float weaponRange = 15f;           // Range to find enemies
    public LayerMask whatIsEnemy;             // Enemy layer mask
    
    public float spawnHeightAbove = 5f;       // How high projectiles spawn above (in pixels/units)

    void Start()
    {
        SetStats();
    }

    void Update()
    {
        if (statsUpdated == true)
        {
            statsUpdated = false;
            SetStats();
        }

        shotCounter -= Time.deltaTime;
        if (shotCounter <= 0)
        {
            shotCounter = stats[weaponLevel].timeBetweenAttacks;
            
            // Find enemies in range
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, weaponRange * stats[weaponLevel].range, whatIsEnemy);
            
            if (enemies.Length > 0)
            {
                // Spawn projectiles at random enemies
                for (int i = 0; i < stats[weaponLevel].amount; i++)
                {
                    Vector3 enemyPosition = enemies[Random.Range(0, enemies.Length)].transform.position;
                    Vector3 spawnPosition = enemyPosition + Vector3.up * spawnHeightAbove;
                    
                    Projectile newProjectile = Instantiate(projectileToSpawn, spawnPosition, Quaternion.identity);
                    newProjectile.gameObject.SetActive(true);
                    
                    // Pass info to projectile
                    RainProjectile rainProjectile = newProjectile as RainProjectile;
                    if (rainProjectile != null)
                    {
                        rainProjectile.damager = explosionDamager;
                        rainProjectile.explosionRadius = stats[weaponLevel].range;
                        rainProjectile.damage = stats[weaponLevel].damage;
                    }
                }
                
                SFXManager.instance.PlaySFXPitched(8);
            }
        }
    }

    void SetStats()
    {
        if (explosionDamager != null)
        {
            explosionDamager.damageAmount = stats[weaponLevel].damage;
            explosionDamager.lifeTime = stats[weaponLevel].duration;
        }

        shotCounter = 0f;
    }
}
