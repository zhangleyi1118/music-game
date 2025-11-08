using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [Header("方块生成设置")]
    public GameObject cubePrefab;
    public Transform[] spawnPoints;
    public int maxCubes = 10;
    public float spawnInterval = 2f;
    
    [Header("节奏生成")]
    public bool useBeatSpawn = true;
    public int spawnOnBeat = 4; // 每4个节拍生成
    
    private List<GameObject> activeCubes = new List<GameObject>();
    private float spawnTimer = 0f;
    
    private void Start()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab 未设置!");
            return;
        }
        
        // 初始化生成一些方块
        for (int i = 0; i < Mathf.Min(maxCubes / 2, spawnPoints.Length); i++)
        {
            SpawnCube();
        }
    }
    
    private void Update()
    {
        if (!useBeatSpawn)
        {
            // 定时器生成模式
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval && activeCubes.Count < maxCubes)
            {
                SpawnCube();
                spawnTimer = 0f;
            }
        }
        
        // 清理超出屏幕的方块
        CleanupCubes();
    }
    
    public void OnBeat(int beatNumber)
    {
        if (useBeatSpawn && beatNumber % spawnOnBeat == 0 && activeCubes.Count < maxCubes)
        {
            SpawnCube();
        }
    }
    
    private void SpawnCube()
    {
        if (spawnPoints.Length == 0) return;
        
        // 随机选择生成点
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];
        
        // 创建方块
        GameObject newCube = Instantiate(cubePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // 随机设置方块类型
        CubeController cubeController = newCube.GetComponent<CubeController>();
        if (cubeController != null)
        {
            cubeController.cubeType = Random.Range(0, 4); // 假设有4种类型
        }
        
        activeCubes.Add(newCube);
    }
    
    private void CleanupCubes()
    {
        for (int i = activeCubes.Count - 1; i >= 0; i--)
        {
            if (activeCubes[i] == null || !activeCubes[i].activeInHierarchy)
            {
                activeCubes.RemoveAt(i);
            }
            else if (activeCubes[i].transform.position.x < -20f) // 超出屏幕
            {
                Destroy(activeCubes[i]);
                activeCubes.RemoveAt(i);
            }
        }
    }
    
    public void ResetAllCubes()
    {
        foreach (GameObject cube in activeCubes)
        {
            if (cube != null)
            {
                Destroy(cube);
            }
        }
        activeCubes.Clear();
    }
}