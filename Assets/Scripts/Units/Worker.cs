using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable, IMoveable
    {
        [SerializeField] private Transform target;
        [SerializeField] private DecalProjector decalProjector;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        

        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);    
            }
        }

        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }
        }
        

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position);
        }
    }
}