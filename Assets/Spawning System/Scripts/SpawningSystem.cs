using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct SpawnObject
{
    public Transform prefab;
    [Range(0, 100)] public int spawnPercent;
}

[System.Serializable]
public struct SpawnPoint
{
    public Transform spawningPoint;
    [Range(0, 100)] public int spawnPercent;
}

public enum ESpawnType
{
    normal,
    withTarget,
    spawnAllSpawnPoints,
}

public class SpawningSystem : MonoBehaviour
{
    public bool canSpawn;
    public bool activeDebugMessage;

    [Space]
    [SerializeField] private List<SpawnObject> spawnObjects;

    [SerializeField] private List<SpawnPoint> spawnPoints;

    [Space]
    [SerializeField] private Transform target;

    [SerializeField] private float startSpawnDelay, spawnDelay;
    [SerializeField] private ESpawnType spawnType;

    [Space]
    public UnityEvent onSpawned;

    private void Start()
    {
        CheckForPercentages();
        StartSpawn();
    }

    private void CheckForPercentages()
    {
        int totalSpawnObjectPercentage = 0;
        for (int i = 0; i < spawnObjects.Count; i++)
        {
            totalSpawnObjectPercentage += spawnObjects[i].spawnPercent;
        }
        if (totalSpawnObjectPercentage != 100 )
        {
            Debug.LogError("Total spawn object percentage is not 100. Total percentage : " + totalSpawnObjectPercentage);
            Debug.LogError("Spawning System can't work correctly!!!");
        }

        int totalSpawnPointPercentage = 0;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            totalSpawnPointPercentage += spawnPoints[i].spawnPercent;
        }
        if (totalSpawnPointPercentage != 100 && spawnType != ESpawnType.spawnAllSpawnPoints)
        {
            Debug.LogError("Total spawn point percentage is not 100. Total percentage : " + totalSpawnPointPercentage);
            Debug.LogError("Spawning System can't work correctly!!!");
        }
    }

    private void WriteDebug(string message)
    {
        if (activeDebugMessage)
            Debug.Log(message);
    }

    private void SpawnRandomPointAndRandomObject()
    {
        if (canSpawn == false)
            return;
        SpawnPoint randomSpawnPoint = new SpawnPoint();
        SpawnObject randomSpawnObject = new SpawnObject();

        int random = Random.Range(0, 101);
        int lastTotalPercentage = 0;
        int currentTotalPercentage = 0;
        if (spawnPoints.Count == 1)
        {
            randomSpawnPoint = spawnPoints[0];
        }
        else
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                currentTotalPercentage = spawnPoints[i].spawnPercent + lastTotalPercentage;
                if (random >= lastTotalPercentage && random <= currentTotalPercentage)
                {
                    randomSpawnPoint = spawnPoints[i];
                    WriteDebug("Random Spawn Point Index: " + i);
                    break;
                }
                lastTotalPercentage += spawnPoints[i].spawnPercent;
            }
        }

        random = Random.Range(0, 101);
        lastTotalPercentage = 0;
        currentTotalPercentage = 0;
        WriteDebug("random2:" + random);
        if (spawnObjects.Count == 1)
        {
            randomSpawnObject = spawnObjects[0];
        }
        else
        {
            for (int i = 0; i < spawnObjects.Count; i++)
            {
                currentTotalPercentage = spawnObjects[i].spawnPercent + lastTotalPercentage;
                if (random >= lastTotalPercentage && random <= currentTotalPercentage)
                {
                    randomSpawnObject = spawnObjects[i];
                    WriteDebug("Random Spawn Object Index: " + i);
                    break;
                }
                lastTotalPercentage += spawnObjects[i].spawnPercent;
            }
        }

        Vector3 spawnPoint = Vector3.zero;
        switch (spawnType)
        {
            case ESpawnType.normal:
                spawnPoint = randomSpawnPoint.spawningPoint.position;
                break;

            case ESpawnType.withTarget:

                Vector3 tempTargetPos = new Vector3(target.position.x, 0, target.position.z);
                spawnPoint = randomSpawnPoint.spawningPoint.position + tempTargetPos;
                break;
        }

        if (randomSpawnObject.prefab != null)
        {
            Instantiate(randomSpawnObject.prefab, spawnPoint, Quaternion.identity);
            onSpawned?.Invoke();
            WriteDebug(randomSpawnObject.prefab.name + " named object spawned at " + randomSpawnPoint.spawningPoint.name);
        }
    }

    private void SpawnInAllPointAndRandomObject()
    {
        if (canSpawn == false)
            return;

        foreach (SpawnPoint itemSpawnPoint in spawnPoints)
        {
            if (Random.Range(0, 101) < itemSpawnPoint.spawnPercent)
            {
                SpawnObject randomSpawnObject = new SpawnObject();

                int random = Random.Range(0, 101);
                WriteDebug("random2:" + random);
                if (spawnObjects.Count == 1)
                {
                    randomSpawnObject = spawnObjects[0];
                }
                else
                {
                    int lastTotalPercentage = 0;
                    int currentTotalPercentage = 0;
                    for (int i = 0; i < spawnObjects.Count; i++)
                    {
                        currentTotalPercentage = spawnObjects[i].spawnPercent + lastTotalPercentage;
                        if (random >= lastTotalPercentage && random <= currentTotalPercentage)
                        {
                            randomSpawnObject = spawnObjects[i];
                            WriteDebug("Random Spawn Object Index: " + i);
                            break;
                        }
                        lastTotalPercentage += spawnObjects[i].spawnPercent;
                    }
                }

                Vector3 spawnPoint = itemSpawnPoint.spawningPoint.position;

                if (randomSpawnObject.prefab != null)
                {
                    Instantiate(randomSpawnObject.prefab, spawnPoint, Quaternion.identity);
                    WriteDebug(randomSpawnObject.prefab.name + " named object spawned at " + itemSpawnPoint.spawningPoint.name);
                }
            }
        }
        onSpawned?.Invoke();
    }

    public void StartSpawn()
    {
        if (spawnType == ESpawnType.normal && spawnType == ESpawnType.withTarget)
        {
            InvokeRepeating("SpawnRandomPointAndRandomObject", startSpawnDelay, spawnDelay);
        }
        else
        {
            SpawnInAllPointAndRandomObject();
        }
    }

    public void StopSpawn()
    {
        CancelInvoke();
    }
}