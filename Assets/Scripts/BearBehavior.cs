using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using System.Collections.Generic;

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
 


    public override void OnEpisodeBegin()
    {
        hungerBar = transform.Find("Canvas/HungerBar").GetComponent<FloatingBar>();
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
        Debug.Log("Action: " + actions.DiscreteActions[0]);
        int actionIndicator = actions.DiscreteActions[0];
        float moveSpeed = 4f;
        Vector3 previousBearLocation = transform.localPosition;
        Vector3 previousDeerLocation = targetTransform.localPosition;

        elapsedTime = 0f;

        // Calculate movement
        //Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;
        //transform.localPosition += movement * Time.deltaTime * moveSpeed;
        //Debug.Log("Indicator Status: " + actionIndicator.ToString());
        if(actionIndicator == 0){
            MoveTowardsDeer(targetTransform.localPosition, moveSpeed);
        }
        if(actionIndicator == 1){
            MoveTowardsDeer(transform.localPosition, moveSpeed);
        
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

        // Calculate the new position using MoveTowards
        Vector3 newPosition = Vector3.MoveTowards(transform.position, deerPosition, moveSpeed * Time.deltaTime); // Adjust speed as needed

        // Update the bear's position
        transform.position = newPosition;

        // Rotate the bear to face the deer
        if (direction.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Adjust rotation speed as needed
        }
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

    private void UpdateHunger(float deltaTime)
    {
        hunger -= deltaTime *10f;
        hungerBar.UpdateBar(hunger, maxHunger);
        if (hunger <= 0)
        {
        EndEpisode();
        AddReward(-100f);
        }
    }
}
