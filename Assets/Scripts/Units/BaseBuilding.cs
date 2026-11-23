using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private readonly Queue<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.LogError("BuildUnit called when the queue was already full!");
                return;
            }

            buildingQueue.Enqueue(unit);
            if (buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnit());
            }
        }

        private IEnumerator DoBuildUnit()
        {
            while (buildingQueue.Count > 0)
            {
                UnitSO currentUnit = buildingQueue.Peek();
                yield return new WaitForSeconds(currentUnit.BuildTime);
                Instantiate(currentUnit.Prefab, transform.position, Quaternion.identity);
                buildingQueue.Dequeue();
            }
        }
    }
}
