using System.Collections.Generic;
using UnityEngine;

public class EnemyEggPool : MonoBehaviour
{
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Camera gameCamera;
    [SerializeField, Min(0.1f)] private float fallSpeed = 4f;

    private readonly Queue<GameObject> availableEggs = new Queue<GameObject>();
    private readonly List<GameObject> activeEggs = new List<GameObject>();

    public void DropEgg(Vector3 position)
    {
        GameObject egg = availableEggs.Count > 0
            ? availableEggs.Dequeue()
            : Instantiate(eggPrefab, transform);

        egg.transform.position = position;
        egg.SetActive(true);
        activeEggs.Add(egg);
    }
    public bool TryRecycleEgg(GameObject egg)
    {
        if (!activeEggs.Remove(egg))
            return false;

        egg.SetActive(false);
        availableEggs.Enqueue(egg);
        return true;
    }
    public void ClearAllEggs()
    {
        for (int i = activeEggs.Count - 1; i >= 0; i--)
        {
            TryRecycleEgg(activeEggs[i]);
        }
    }

    private void Update()
    {
        for (int i = activeEggs.Count - 1; i >= 0; i--)
        {
            GameObject egg = activeEggs[i];
            egg.transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            if (gameCamera.WorldToViewportPoint(egg.transform.position).y < -0.05f)
            {
                egg.SetActive(false);
                activeEggs.RemoveAt(i);
                availableEggs.Enqueue(egg);
            }
        }
    }
    
}