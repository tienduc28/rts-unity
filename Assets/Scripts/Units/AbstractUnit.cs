using System;
using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        private NavMeshAgent _agent;
        public float AgentRadius => _agent.radius;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position);
        }
    }
}