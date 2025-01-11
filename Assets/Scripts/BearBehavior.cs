using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BearBehavior : Agent 
{

    [SerializeField] private Transform targetTransform;
    [SerializeField] float health, maxHealth = 100f; // Health of the bear
    [SerializeField] FloatingBar healthBar;
    private Animator animator;              // Animator component
    Vector3 offset = new Vector3(0, 1, 0);
 


    public override void OnEpisodeBegin()
    {
        healthBar = GetComponentInChildren<FloatingBar>();
        health =100f;
        transform.localPosition =  offset + new Vector3(543f, 23f,388f);
        targetTransform.localPosition =  new Vector3(Random.Range(540f, 550f), 22f, Random.Range(382f, 382f));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);

        }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveSpeed = 4f;

        // Calculate movement vector
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;

        // Move the bear
        transform.localPosition += movement * Time.deltaTime * moveSpeed;

        // Rotate the bear to face the movement direction
        if (movement.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Raycast to align the bear with the terrain's surface normal
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10f))
        {
            // Align the bear's up vector with the surface normal
            Vector3 terrainNormal = hit.normal;
            Quaternion terrainAlignment = Quaternion.FromToRotation(transform.up, terrainNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, terrainAlignment, Time.deltaTime * 10f);
        }
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
        if (other.TryGetComponent<DeerBehavior>(out DeerBehavior deer))
        {
            SetReward(1f);
            EndEpisode();
        }

    }
}
