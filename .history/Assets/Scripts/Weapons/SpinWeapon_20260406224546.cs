using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;

public class SpinWeapon : Weapon
{
    public float rotateSpeed;
    public Transform holder, fireballToSpawn;
    public float timeBetweenSpawn;
    private float spawnCounter;
    public EnemyDamager damager;

    void Start()
    {
        SetStats();
    }

    private void OnEnable()
    {
        SetStats();
        spawnCounter = 0f;
    }

    void Update()
    {
        Debug.Log("SpinWeapon Update running");

        if (holder == null || fireballToSpawn == null || stats == null || stats.Count == 0) 
            return;

        // ✅ Safety check - weaponLevel phải hợp lệ
        

        holder.rotation = Quaternion.Euler(
            0f, 0f, holder.rotation.eulerAngles.z + (rotateSpeed * Time.deltaTime * stats[weaponLevel].speed)
        );

        spawnCounter -= Time.deltaTime;
        if (spawnCounter <= 0)
        {
            spawnCounter = timeBetweenSpawn;

            float radius = stats[weaponLevel].range;

            for (int i = 0; i < stats[weaponLevel].amount; i++)
            {
                float rot = (360f / stats[weaponLevel].amount) * i;

                Transform bullet = Instantiate(fireballToSpawn, holder);
                Vector3 offset = Quaternion.Euler(0, 0, rot) * Vector3.up * radius;

                bullet.localPosition = offset;
                bullet.localRotation = Quaternion.identity;
                bullet.gameObject.SetActive(true);
            }

            if (SFXManager.instance != null)
                SFXManager.instance.PlaySFX(8);
        }

        if (statsUpdated)
        {
            SetStats();
            statsUpdated = false;
        }
    }

    public void SetStats()
    {
        if (stats == null || stats.Count == 0) return;

        damager.damageAmount = stats[weaponLevel].damage;
        timeBetweenSpawn = stats[weaponLevel].timeBetweenAttacks;
        damager.lifeTime = stats[weaponLevel].duration;
        spawnCounter = 0f;
    }
}