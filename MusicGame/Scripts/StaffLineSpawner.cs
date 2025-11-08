using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffLineSpawner : MonoBehaviour
{
    [Header("五线谱生成设置")]
    public GameObject staffLinePrefab;
    public Transform[] spawnPositions; // 不同高度的生成位置
    public float spawnInterval = 2f;
    public float moveSpeed = 5f;
    
    [Header("节奏生成")]
    public bool useBeatSpawn = true;
    public int[] beatPattern = { 2, 4, 6, 8 }; // 在哪些节拍生成
    
    private List<GameObject> activeStaffLines = new List<GameObject>();
    private float spawnTimer = 0f;
    private int patternIndex = 0;
    
    private void Start()
    {
        if (staffLinePrefab == null)
        {
            Debug.LogError("Staff Line Prefab 未设置!");
            return;
        }
    }
    
    private void Update()
    {
        if (!useBeatSpawn)
        {
            // 定时器生成模式
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnStaffLine();
                spawnTimer = 0f;
            }
        }
        
        // 清理超出屏幕的五线谱
        CleanupStaffLines();
    }
    
    public void OnBeat(int beatNumber)
    {
        if (useBeatSpawn && patternIndex < beatPattern.Length && beatNumber == beatPattern[patternIndex])
        {
            SpawnStaffLine();
            patternIndex = (patternIndex + 1) % beatPattern.Length;
        }
    }
    
    private void SpawnStaffLine()
    {
        if (spawnPositions.Length == 0) return;
        
        // 随机选择生成位置
        int positionIndex = Random.Range(0, spawnPositions.Length);
        Transform spawnPos = spawnPositions[positionIndex];
        
        // 创建五线谱
        GameObject newStaffLine = Instantiate(staffLinePrefab, spawnPos.position, spawnPos.rotation);
        
        // 设置移动速度
        StaffLineController controller = newStaffLine.GetComponent<StaffLineController>();
        if (controller != null)
        {
            controller.SetSpeed(moveSpeed);
        }
        
        activeStaffLines.Add(newStaffLine);
    }
    
    private void CleanupStaffLines()
    {
        for (int i = activeStaffLines.Count - 1; i >= 0; i--)
        {
            if (activeStaffLines[i] == null || activeStaffLines[i].transform.position.x < -20f)
            {
                if (activeStaffLines[i] != null)
                    Destroy(activeStaffLines[i]);
                activeStaffLines.RemoveAt(i);
            }
        }
    }
    
    public void ResetAllStaffLines()
    {
        foreach (GameObject staffLine in activeStaffLines)
        {
            if (staffLine != null)
                Destroy(staffLine);
        }
        activeStaffLines.Clear();
        patternIndex = 0;
    }
}