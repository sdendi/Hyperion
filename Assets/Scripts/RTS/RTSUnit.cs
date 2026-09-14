using UnityEngine;
using UnityEngine.AI;

namespace RTS
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class RTSUnit : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private GameObject selectionIndicator;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float angularSpeed = 720f;
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float stoppingDistance = 0.15f;

        private bool isSelected = false;

        public bool IsSelected => isSelected;
        public NavMeshAgent Agent => agent;

        private void Awake()
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            ConfigureAgent();
            UpdateVisuals();
        }

        private void ConfigureAgent()
        {
            if (agent != null)
            {
                agent.speed = moveSpeed;
                agent.angularSpeed = angularSpeed;
                agent.acceleration = acceleration;
                agent.stoppingDistance = stoppingDistance;
                agent.autoBraking = true;
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }

        public void MoveTo(Vector3 destination)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
            }
        }

        public void Stop()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        public void SetSelectionIndicator(GameObject indicator)
        {
            selectionIndicator = indicator;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (agent != null && agent.hasPath)
            {
                Gizmos.color = Color.green;
                Vector3 prev = transform.position;
                foreach (var corner in agent.path.corners)
                {
                    Gizmos.DrawLine(prev, corner);
                    Gizmos.DrawSphere(corner, 0.1f);
                    prev = corner;
                }
            }
        }
    }
}
