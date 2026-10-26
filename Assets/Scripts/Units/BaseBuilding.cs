using System.Collections;
using UnityEngine;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public void BuildUnit(UnitSO unit)
        {
            StartCoroutine(DoBuildUnit(unit));
        }

        private IEnumerator DoBuildUnit(UnitSO unit)
        {
            Debug.Log("Start Coroutine");
            yield return new WaitForSeconds(unit.BuildTime);
            Debug.Log("End Coroutine");
            Instantiate(unit.Prefab, transform.position, Quaternion.identity);
        }
    }
}