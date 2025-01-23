using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine.AI;

public class BearBehavior : Agent 
{

    [SerializeField] private Transform targetTransform;
   // [SerializeField] float health, maxHealth = 100f; // Health of the bear
    [SerializeField] float hunger, maxHunger = 100f;
    //[SerializeField] FloatingBar healthBar;
    [SerializeField] FloatingBar hungerBar;
    //[SerializeField] private Rigidbody deerRigidbody;
    private Animator animator;              // Animator component
    Vector3 offset = new Vector3(0, 1, 0);
    private float elapsedTime;
    public int currentAction;
 


    public override void OnEpisodeBegin()
    {
        hungerBar = transform.Find("Canvas/HungerBar").GetComponent<FloatingBar>();
        currentAction = 0;
        //health = 100f;
        hunger = 100f;
        transform.localPosition = offset + new Vector3(Random.Range(-3f, 10f), 9f, Random.Range(-10f, 15f));
        targetTransform.localPosition = new Vector3(Random.Range(-3f, 10f), 8f, Random.Range(-20f, 25f));
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
        // Penalizing changing actions to avoid ineficient behavior switching
        if(currentAction != actions.DiscreteActions[0]){
            AddReward(-10f);
           // Debug.Log("Action changed Reward: " + GetCumulativeReward());
        }
        currentAction = actions.DiscreteActions[0];
        
        int actionIndicator = actions.DiscreteActions[0];
        float moveSpeed = 4f;
        Vector3 previousBearLocation = transform.localPosition;
        Vector3 previousDeerLocation = targetTransform.localPosition;
        elapsedTime = 0f;

        
        Transform nearestDeer = FindNearestDeer();
        
        if(actionIndicator == 0 && nearestDeer != null){
            MoveTowardsDeer(nearestDeer.position, moveSpeed);
        }
        if(actionIndicator == 1){
            //MoveTowardsDeer(transform.localPosition, moveSpeed);
        
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

        UpdateHunger(Time.deltaTime);

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
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        // Default to idle action
        discreteActions[0] = 1; // Idle action

        // Override if specific keys are held down
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 0; // Chase deer action
        }

        // Updating time
       // UpdateHunger(Time.deltaTime);
    }


    private void MoveTowardsDeer(Vector3 deerPosition, float moveSpeed)
    {
        // Calculate direction to the deer
        Vector3 direction = (deerPosition - transform.position).normalized;

        // Perform a raycast to detect obstacles in the direction of movement
        RaycastHit hit;
        float obstacleDetectionRange = 2f; // Adjust based on the size of obstacles

        if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, obstacleDetectionRange))
        {
            if (hit.collider.CompareTag("Terrain")) // Ensure obstacles are tagged appropriately
            {
                // Calculate a direction to avoid the obstacle
                Vector3 avoidanceDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
                direction += avoidanceDirection; // Adjust the direction slightly
            }
        }

        // Calculate the new position using MoveTowards
        Vector3 newPosition = Vector3.MoveTowards(transform.position, transform.position + direction, moveSpeed * Time.deltaTime);

        // Update the bear's position
        transform.position = newPosition;

        // Rotate the bear to face the direction of movement
        if (direction.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private Transform FindNearestDeer()
    {
        GameObject[] allDeers = GameObject.FindGameObjectsWithTag("Deer"); // Ensure all deer are tagged as "Deer"
        Transform nearestDeer = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject deer in allDeers)
        {
            float distance = Vector3.Distance(transform.position, deer.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDeer = deer.transform;
            }
        }

        return nearestDeer;;
    }

    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DeerBehavior deer))
        {
            float foodReward = Mathf.Min(50f,maxHunger - hunger); // Reward based on remaining hunger
            hunger += foodReward;
            AddReward(foodReward*10f);
            Debug.Log("Deer caught by the bear!");
            Debug.Log("Reward: " + GetCumulativeReward());
            hungerBar.UpdateBar(hunger, maxHunger);
            deer.RespawnDeer();
        }

    }

    private void UpdateHunger(float deltaTime)
    {
        hunger -= deltaTime *10f;
        hungerBar.UpdateBar(hunger, maxHunger);
        if (hunger <= 0)
        {
            AddReward(-100f);
            Debug.Log("The bear starved! Final Reward: " + GetCumulativeReward());
            EndEpisode();
        
        }
    }
}
