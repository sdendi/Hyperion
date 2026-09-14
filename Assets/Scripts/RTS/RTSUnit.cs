using UnityEngine;
using UnityEngine.AI;

namespace RTS
{
    public enum UnitType
    {
        Frigate,
        Corvette,
        Destroyer,
        Cruiser,
        Battleship
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class RTSUnit : MonoBehaviour
    {
        [Header("Unit Properties")]
        [SerializeField] private string unitName = "Corvette";
        [SerializeField] private UnitType unitType = UnitType.Corvette;
        [Tooltip("Group / hotkey number assigned to select all units of this group (1-9). 0 = None.")]
        [SerializeField] private int groupNumber = 1;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private int resourceCost = 50;

        [Header("Components")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private GameObject selectionIndicator;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float angularSpeed = 720f;
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float stoppingDistance = 0.3f;
        [SerializeField] private int avoidancePriority = 50;

        private bool isSelected = false;
        private bool hasMoveOrder = false;

        public string UnitName => unitName;
        public UnitType Type => unitType;
        public int GroupNumber { get => groupNumber; set => groupNumber = value; }
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public int ResourceCost => resourceCost;
        public bool IsSelected => isSelected;
        public NavMeshAgent Agent => agent;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            ConfigureAgent();
            UpdateVisuals();
        }

        private void Update()
        {
            CheckArrival();
        }

        private void CheckArrival()
        {
            if (hasMoveOrder && agent != null && agent.isOnNavMesh)
            {
                if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.25f))
                {
                    // Arrived at destination: stop and clear path to lock position/rotation and avoid jitter
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    hasMoveOrder = false;
                }
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        private void Die()
        {
            Destroy(gameObject);
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
                agent.avoidancePriority = avoidancePriority;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
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
                hasMoveOrder = true;
            }
        }

        public void Stop()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                hasMoveOrder = false;
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
