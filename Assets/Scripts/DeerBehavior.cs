using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DeerBehavior : MonoBehaviour
{
    [SerializeField] public Transform bearTransform; // Reference to the bear's transform
    public float RunSpeed = 2f;             // Running speed of the deer
    public float idleTime = 2f;            // Time the deer idles
    public float RunTime = 3f;             // Time the deer runs
    public float terrainSize = 1000f;        // Size of the terrain to limit movement
    public float maxDistance = 1000f;         // Define the maximum distance the deer can run
    private Animator animator;             // Animator component
    private Vector3 targetPosition;        // Deer's random target position

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(deerRoutine());
    }

    
    void Update()
    {
        if(Vector3.Distance(transform.position, bearTransform.transform.position)<0.5f){
            RespawnDeer();
        }
    }

    private IEnumerator deerRoutine()
    {
        while (true)
        {
            // Idle Phase
            animator.SetBool("DeerRun", false);
           // Debug.Log("Set DeerRun to false (Idle Phase)");
            yield return new WaitForSeconds(idleTime);

            // Run Phase
            targetPosition = GetRandomPosition();
            animator.SetBool("DeerRun", true);
            //Debug.Log("Set DeerRun to true (Run Phase)");

            // Move toward the target position until the deer gets close enough
            while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
            {
                MoveTowardsTarget();
                yield return null;
            }

            // Once the deer reaches the target, return to idle
            animator.SetBool("DeerRun", false);
        }
    }

    private Vector3 GetRandomPosition()
    {
        if (Terrain.activeTerrain != null)
        {
            // Generate a random offset within the maximum distance
            float offsetX = Random.Range(-maxDistance, maxDistance);
            float offsetZ = Random.Range(-maxDistance, maxDistance);

            // Calculate the new position
            Vector3 newPosition = transform.position + new Vector3(offsetX, 0, offsetZ);

            // Clamp the new position to stay within the terrain boundaries
            Terrain terrain = Terrain.activeTerrain;
            Vector3 terrainPosition = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            newPosition.x = Mathf.Clamp(newPosition.x, terrainPosition.x, terrainPosition.x + terrainSize.x);
            newPosition.z = Mathf.Clamp(newPosition.z, terrainPosition.z, terrainPosition.z + terrainSize.z);

            // Get the terrain height at the new position
            float y = terrain.SampleHeight(newPosition) + terrain.transform.position.y;

            return new Vector3(newPosition.x, y, newPosition.z);
        }

        // Fallback to current position if no terrain is available
        return transform.position;
    }

    private void MoveTowardsTarget()
    {
        // Move the deer horizontally toward the target position
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, RunSpeed * Time.deltaTime);

        // Adjust the y-position to match the terrain height
        if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(nextPosition) + Terrain.activeTerrain.transform.position.y;
            nextPosition.y = terrainHeight;
        }

        transform.position = nextPosition;

        // Rotate the deer to face the target
        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }
    }

    public void RespawnDeer()
    {
        // Generate a new random position
        Vector3 newPosition = GetRandomPosition();

        // Update the deer's position
        transform.position = newPosition;

        // Optionally reset animations or other states
        animator.SetBool("DeerRun", false);
        targetPosition = newPosition; // Reset the target position

        Debug.Log("Deer respawned at: " + newPosition);
    }
}
