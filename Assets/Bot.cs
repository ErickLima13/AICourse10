using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    private NavMeshAgent Agent;
    private Drive targetDrive;

    private Vector3 wanderTarget = Vector3.zero;
    private Vector3 targetWorld;

    public GameObject target;

    public float wanderDistance = 10;
    public float wanderRadius = 20;
    public float wanderJitter = 1;

    // Start is called before the first frame update
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        targetDrive = target.GetComponent<Drive>();
    }

    // Update is called once per frame
    void Update()
    {
        //Pursue();
        //Evade();
        Wander();
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
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (Agent.speed + targetDrive.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    private void Evade()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        float lookAhead = targetDir.magnitude / (Agent.speed + targetDrive.currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    private void Wander()
    {
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                            0, Random.Range(-1.0f, 1.0f) * wanderJitter);
        
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        targetWorld = transform.InverseTransformVector(targetLocal);
        Seek(targetWorld);
        Debug.DrawLine(transform.position, targetWorld,Color.green);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetWorld, wanderRadius);
    }
}
