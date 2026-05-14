using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class SimpleButterflyFlight : MonoBehaviour
{
    [Header("Flight Area")]
    public float areaRadius = 4f;
    public float minHeightOffset = 0.25f;
    public float maxHeightOffset = 0.9f;

    [Header("Movement")]
    public float speed = 0.4f;
    public float turnSpeed = 3.0f;
    public float rotationSpeed = 6.0f;
    public float reachDistance = 0.5f;
    public float targetRadius = 1.4f;
    public float minTargetDistance = 0.8f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float sweepDistance = 0.8f;
    public float sideAvoidStrength = 1.5f;
    public float upAvoidStrength = 0.5f;

    [Header("Ground Safety")]
    public float desiredGroundClearance = 0.45f;
    public float groundRayHeight = 1.0f;
    public float groundRayDistance = 2.0f;
    public float heightAdjustSpeed = 3.0f;

    [Header("Player Reaction")]
    public Transform player;
    public float playerDetectDistance = 2.5f;
    public float fleeDistance = 2.5f;
    public float fleeCooldown = 2.0f;

    private Rigidbody rb;
    private SphereCollider bodyCollider;

    private Vector3 areaCenter;
    private Vector3 targetPoint;
    private Vector3 currentDirection;
    private float baseHeight;
    private float lastFleeTime = -10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<SphereCollider>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        areaCenter = transform.position;
        baseHeight = transform.position.y;
        currentDirection = transform.forward.sqrMagnitude > 0.01f
            ? transform.forward.normalized
            : Vector3.forward;

        PickNewTarget();
    }

    private void FixedUpdate()
    {
        HandlePlayerReaction();

        Vector3 toTarget = targetPoint - transform.position;
        if (toTarget.magnitude <= reachDistance)
        {
            PickNewTarget();
            toTarget = targetPoint - transform.position;
        }

        Vector3 desiredDirection = toTarget.sqrMagnitude > 0.0001f
            ? toTarget.normalized
            : currentDirection;

        desiredDirection = GetSafeDirection(desiredDirection);

        currentDirection = Vector3.Lerp(
            currentDirection,
            desiredDirection,
            turnSpeed * Time.fixedDeltaTime
        ).normalized;

        Vector3 velocity = currentDirection * speed;

        float targetY = GetSafeHeight(transform.position);
        float yDiff = targetY - transform.position.y;
        velocity.y = Mathf.Clamp(yDiff * heightAdjustSpeed, -1.5f, 1.5f);

        rb.velocity= velocity;

        Vector3 lookDirection = new Vector3(currentDirection.x, 0f, currentDirection.z);
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(smoothRotation);
        }
    }

    private void HandlePlayerReaction()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < playerDetectDistance && Time.time > lastFleeTime + fleeCooldown)
        {
            Vector3 awayDir = (transform.position - player.position).normalized;
            Vector3 randomSide = Random.insideUnitSphere * 0.4f;
            randomSide.y = Mathf.Abs(randomSide.y);

            Vector3 fleeDir = (awayDir + randomSide).normalized;
            targetPoint = ClampToArea(transform.position + fleeDir * fleeDistance);
            lastFleeTime = Time.time;
        }
    }

    private Vector3 GetSafeDirection(Vector3 desiredDirection)
    {
        if (!WillHit(desiredDirection, out _))
            return desiredDirection;

        if (WillHit(desiredDirection, out RaycastHit hit))
        {
            // 1) попытка скользить вдоль поверхности
            Vector3 slideDir = Vector3.ProjectOnPlane(desiredDirection, hit.normal).normalized;
            if (slideDir.sqrMagnitude > 0.01f && !WillHit(slideDir, out _))
                return slideDir;

            // 2) уйти в сторону от препятствия
            Vector3 sideDir = (slideDir + transform.right * sideAvoidStrength).normalized;
            if (sideDir.sqrMagnitude > 0.01f && !WillHit(sideDir, out _))
                return sideDir;

            Vector3 otherSideDir = (slideDir - transform.right * sideAvoidStrength).normalized;
            if (otherSideDir.sqrMagnitude > 0.01f && !WillHit(otherSideDir, out _))
                return otherSideDir;

            // 3) слегка вверх
            Vector3 upDir = (desiredDirection + Vector3.up * upAvoidStrength).normalized;
            if (!WillHit(upDir, out _))
                return upDir;
        }

        PickNewTarget();
        return currentDirection;
    }

    private bool WillHit(Vector3 direction, out RaycastHit hit)
    {
        Vector3 dir = direction.normalized;
        float distance = sweepDistance;

        return rb.SweepTest(dir, out hit, distance, QueryTriggerInteraction.Ignore);
    }

    private float GetSafeHeight(Vector3 currentPos)
    {
        float minAllowedY = baseHeight + minHeightOffset;
        float maxAllowedY = baseHeight + maxHeightOffset;

        Vector3 rayOrigin = currentPos + Vector3.up * groundRayHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            float wantedY = hit.point.y + desiredGroundClearance;
            return Mathf.Clamp(wantedY, minAllowedY, maxAllowedY);
        }

        return Mathf.Clamp(currentPos.y, minAllowedY, maxAllowedY);
    }

    private void PickNewTarget()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 offset = Random.insideUnitSphere * targetRadius;
            offset.y *= 0.4f;

            Vector3 candidate = ClampToArea(transform.position + offset);
            float dist = Vector3.Distance(transform.position, candidate);

            if (dist < minTargetDistance)
                continue;

            Vector3 dir = (candidate - transform.position).normalized;

            if (!WillHit(dir, out _))
            {
                targetPoint = candidate;
                return;
            }
        }

        targetPoint = ClampToArea(transform.position + transform.forward * 0.8f + Vector3.up * 0.2f);
    }

    private Vector3 ClampToArea(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, areaCenter.x - areaRadius, areaCenter.x + areaRadius);
        pos.z = Mathf.Clamp(pos.z, areaCenter.z - areaRadius, areaCenter.z + areaRadius);
        pos.y = Mathf.Clamp(pos.y, baseHeight + minHeightOffset, baseHeight + maxHeightOffset);
        return pos;
    }
}