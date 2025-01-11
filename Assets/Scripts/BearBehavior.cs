using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BearBehavior : Agent 
{

    [SerializeField] private Transform targetTransform;
    Vector3 offset = new Vector3(0, 1, 0);
 


    public override void OnEpisodeBegin()
    {
        transform.localPosition =  offset + new Vector3(543f, 23f,388f);
        targetTransform.localPosition =  new Vector3(Random.Range(540f, 550f), 24f, Random.Range(382f, 382f));
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
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
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
