using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    private NavMeshAgent Agent;
    private Drive targetDrive;

    public GameObject target;



    // Start is called before the first frame update
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        targetDrive = target.GetComponent<Drive>();
    }

    // Update is called once per frame
    void Update()
    {
        Pursue();
    }

    private void Seek(Vector3 location)
    {
        Agent.SetDestination(location);
    }

    private void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        Agent.SetDestination(transform.position - fleeVector);
    }

    private void Pursue()
    {
        Vector3 targetDir = target.transform.position - transform.position;

        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));

        if ((toTarget > 90 && relativeHeading < 20) || targetDrive.currentSpeed < 0.01f)
        {
            //Seek(target.transform.position);
            Evade();
            return;
        }

        float lookAhead = targetDir.magnitude / (Agent.speed + targetDrive.currentSpeed);

        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    private void Evade()
    {
        Vector3 targetDir = transform.position - target.transform.position;

        Flee(targetDir);
    }
}
