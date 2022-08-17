using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Bot : MonoBehaviour
{
    private NavMeshAgent agent;
    private Drive targetDrive;

    private Vector3 wanderTarget = Vector3.zero;
    private Vector3 targetWorld;

    public GameObject target;

    public float wanderDistance = 10;
    public float wanderRadius = 20;
    public float wanderJitter = 1;

    public bool coolDown;

    public string stateName;

    public TextMeshProUGUI state;

    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        targetDrive = target.GetComponent<Drive>();
    }

    // Update is called once per frame
    void Update()
    {
        //Pursue();
        //Evade();
        //Wander();
        //Hide();

        canvas.transform.rotation = Quaternion.Euler(transform.rotation.x, -114.317f, transform.rotation.z);

        state.text = stateName;

        if (!coolDown)
        {
            if (!TargetInRange())
            {
                Wander();
            }
            else if (CanSeeTarget() && TargetCanSeeMe())
            {
                CleverHide();
                coolDown = true;
                Invoke("BehaviorCoolDown", 5);
            }
            else
            {
                Pursue();
            }
        }

        //if (!coolDown)
        //{
        //    float distance = Vector3.Distance(transform.position, target.transform.position);

        //    if (distance > 10)
        //    {
        //        Wander();
        //        coolDown = true;
        //        Invoke("BehaviorCoolDown", 5);
        //        print(distance);
        //    }
        //    else if (TargetCanSeeMe() && CanSeeTarget())
        //    {
        //        CleverHide();
        //    }
        //    else
        //    {
        //        Pursue();
        //    }
        //} 
    }

    private void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    private void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        agent.SetDestination(transform.position - fleeVector);
    }

    private void Pursue()
    {
        stateName = "Pursue";

        Vector3 targetDir = target.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));

        if ((toTarget > 90 && relativeHeading < 20) || targetDrive.currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + targetDrive.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);

       
    }

    private void Evade()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + targetDrive.currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    private void Wander()
    {
        stateName = "Wander";

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                            0, Random.Range(-1.0f, 1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        targetWorld = transform.InverseTransformVector(targetLocal);
        Seek(targetWorld);
        Debug.DrawLine(transform.position, targetWorld, Color.green);

        
    }

    private void Hide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Seek(chosenSpot);

        Debug.DrawLine(transform.position, chosenSpot, Color.green);
    }

    private void CleverHide()
    {
        stateName = "CleverHide";

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10;

            if (Vector3.Distance(transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);

        Vector3 location = info.point + chosenDir.normalized * 2;

        Seek(location);

        Debug.DrawLine(transform.position, location, Color.green);

       
    }

    private bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - transform.position;

        Debug.DrawRay(transform.position, rayToTarget, Color.red);

        if (Physics.Raycast(transform.position, rayToTarget, out raycastInfo))
        {
            if (raycastInfo.transform.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private bool TargetCanSeeMe()
    {
        Vector3 toAgent = transform.position - target.transform.position;
        float lookingAngle = Vector3.Angle(target.transform.forward, toAgent);

        if (lookingAngle < 60)
        {
            return true;
        }
        return false;
    }

    private bool TargetInRange()
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 10)
        {
            return true;
        }

        return false;
    }

    private void BehaviorCoolDown()
    {
        coolDown = false;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(targetWorld, wanderRadius);
    }
}
