using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BearBehavior : Agent 
{

    [SerializeField] private Transform targetTransform;
    [SerializeField] float health, maxHealth = 100f; // Health of the bear
    [SerializeField] FloatingBar healthBar;
    //[SerializeField] private Rigidbody deerRigidbody;
    private Animator animator;              // Animator component
    Vector3 offset = new Vector3(0, 1, 0);
    private float elapsedTime;
 


    public override void OnEpisodeBegin()
    {
        healthBar = GetComponentInChildren<FloatingBar>();
        //health = 100f;
        transform.localPosition = offset + new Vector3(-5f, 9f, 0.2f);
        targetTransform.localPosition = new Vector3(Random.Range(-3f, 10f), 9f, Random.Range(-10f, 15f));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Bear's position
        sensor.AddObservation(transform.localPosition);

        // Relative position of the deer
        Vector3 relativePosition = targetTransform.localPosition - transform.localPosition;
        sensor.AddObservation(relativePosition);

        // Bear's rotation and velocity (for more context)
        //sensor.AddObservation(transform.forward);

        // Deer's velocity (direction and speed)
        //sensor.AddObservation(deerRigidbody.linearVelocity);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveSpeed = 4f;
        Vector3 previousBearLocation = transform.localPosition;
        Vector3 previousDeerLocation = targetTransform.localPosition;

        elapsedTime = 0f;

        // Calculate movement
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;
        transform.localPosition += movement * Time.deltaTime * moveSpeed;

        // Rotate the bear to face the movement direction
        if (movement.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Align the bear to the terrain
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10f))
        {
            Vector3 terrainNormal = hit.normal;
            Quaternion terrainAlignment = Quaternion.FromToRotation(transform.up, terrainNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, terrainAlignment, Time.deltaTime * 10f);
        }

        // Reward for moving closer to the deer
        float distanceToDeer = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float previousDistance = Vector3.Distance(previousBearLocation, previousDeerLocation);

        
        // Updating time
        elapsedTime += Time.deltaTime;

        AddReward(previousDistance - distanceToDeer - elapsedTime);

    }


    /*
    private void UpdateAnimation(float horizontal, float vertical)
    {
        bool isWalking = horizontal != 0 || vertical != 0;
        animator.SetBool("WalkForward", isWalking);
    }
    */

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        continuousActions[0] = horizontal;
        continuousActions[1] = vertical;

       // UpdateAnimation(horizontal, vertical);
    }  
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DeerBehavior deer))
        {
            AddReward(100f);
            Debug.Log("Deer caught by the bear!");
            Debug.Log("Reward: " + GetCumulativeReward());
            EndEpisode();
        }

    }
}
