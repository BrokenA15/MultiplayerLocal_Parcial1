using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [Header("Base Stats")]
    public float baseSpeed = 2f;
    public float baseFireRate = 1f;
    public float baseProjectileSize = 1f;

    [Header("Runtime Stats")]
    public float speedMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float projectileSizeMultiplier = 1f;

    Dictionary<PowerUpType, float> activePowerups = new Dictionary<PowerUpType, float>();

    void Update()
    {
        UpdatePowerups();
    }

    void UpdatePowerups()
    {

        List<PowerUpType> keys = new List<PowerUpType>(activePowerups.Keys);

        foreach (PowerUpType key in keys)
        {
            activePowerups[key] -= Time.deltaTime;

            if (activePowerups[key] <= 0)
            {
                RemovePowerup(key);
                activePowerups.Remove(key);
            }
        }
    }

    public void AddPowerup(PowerUpType type, float duration, float value)
    {

        if (activePowerups.ContainsKey(type))
            activePowerups[type] += duration;
        else
        {
            activePowerups.Add(type, duration);
            ApplyPowerup(type, value);
        }

    }

    void ApplyPowerup(PowerUpType type, float value)
    {

        switch (type)
        {
            case PowerUpType.Speed:
                speedMultiplier += value;
                break;

            case PowerUpType.Big:
                sizeMultiplier += value;
                UpdateScale();
                break;

            case PowerUpType.Small:
                sizeMultiplier -= value;
                UpdateScale();
                break;

            case PowerUpType.FastFire:
                fireRateMultiplier += value;
                break;

            case PowerUpType.BigProjectiles:
                projectileSizeMultiplier += value;
                break;
        }
    }

    void RemovePowerup(PowerUpType type)
    {

        switch (type)
        {
            case PowerUpType.Speed:
                speedMultiplier = 1;
                break;

            case PowerUpType.Big:
            case PowerUpType.Small:
                sizeMultiplier = 1;
                UpdateScale();
                break;

            case PowerUpType.FastFire:
                fireRateMultiplier = 1;
                break;

            case PowerUpType.BigProjectiles:
                projectileSizeMultiplier = 1;
                break;
        }

    }

    void UpdateScale()
    {
        transform.localScale = Vector3.one * sizeMultiplier;
    }

    public float GetSpeed()
    {
        return baseSpeed * speedMultiplier;
    }

    public float GetFireRate()
    {
        return baseFireRate * fireRateMultiplier;
    }

    public float GetProjectileSize()
    {
        return baseProjectileSize * projectileSizeMultiplier;
    }

}

public enum PowerUpType
{
    Speed,
    Big,
    Small,
    FastFire,
    BigProjectiles
}