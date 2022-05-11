using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//The different states the cat can be in.  The program will check its state
//when updating.
public enum CatState
{
    ROAMING,
    CURIOUS,
    DISTRESS,
    DEAD
}
public class CatAI : MonoBehaviour
{
    //The spawner for the cat prefabs.
    private CatSpawner catSpawner;

    //Cat state is roaming by default.
    private CatState currentState = CatState.ROAMING;

    //The different target locations a cat might go.
    [SerializeField] private List<Transform> randomPositions = new List<Transform>();
    [SerializeField] private List<Transform> goalPositions = new List<Transform>();

    //The hazard.  We'll change this to a list once we add more of them.
    [SerializeField] private Hazard hazard;

    //Different time variables for the timers.
    private float decisionTime = 5f;
    private float distressTime = 5f;

    //The cat's curiousity levels.  This will increase over time.
    private float curiosity = 10f;

    //The help points for the help function.  See below.
    private const float HELP_POINTS = 27;
    private float playerHelp = 0;

    //NavMesh stuff.
    private NavMeshAgent agent;

    //Generate NavMesh.
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    //Set the spawner.
    public void setSpawner(CatSpawner catSpawner)
    {
        this.catSpawner = catSpawner;
    }

    //Set the objects the cat interacts with.
    public void setPositions(List<Transform> randomPositions, List<Transform> goalPositions, Hazard hazard)
    {
        this.randomPositions = randomPositions;
        this.goalPositions = goalPositions;
        this.hazard = hazard;
    }

    //Set default state.  Once this calls, the AI loop begins!
    private void Start()
    {
        setState(currentState);
    }

    private void Update()
    {
        
    }

    //This switch statement is called whenever states change around.  Very handy.
    public void setState(CatState catState)
    {
        //Cancel previous coroutines generated by previous state, and assign new state.
        StopAllCoroutines();
        currentState = catState;

        switch (currentState)
        {
            //If the cat is roaming, do the romaing function. Etc.
            case CatState.ROAMING:
                StartCoroutine("roam");
                break;
            case CatState.CURIOUS:
                StartCoroutine("ApproachHazard");
                break;
            case CatState.DISTRESS:
                StartCoroutine("CatInTrouble");
                break;
            case CatState.DEAD:
                //If the cat dies, de-occupy the hazard and destroy the cat prefab.
                Debug.Log("A cat has fallen.");
                hazard.occupied = false;
                Destroy(this.gameObject);
                break;
        }
    }

    //Roaming Script.
    private IEnumerator roam()
    {
        while (currentState == CatState.ROAMING)
        {

            //Pick a random number between the cat's current curiosity level and 100.
            //If the number is above the threshold, the cat enters its curious state.
            //Ensures that, eventually, the cat's curiosity will overwhelm it.
            if (Random.Range(curiosity, 100) >= 95)
            {
                //Checks if the hazard is occupied.  If it is, it keeps roaming.
                if (hazard.occupied == true)
                {
                    chooseRandomTarget();
                }
                else
                {
                    hazard.occupied = true;
                    setState(CatState.CURIOUS);
                }
            }
            //Otherwise, the cat's curiosity grows.
            else
            {
                chooseRandomTarget();
                curiosity = Mathf.Clamp(curiosity + 10, 0, 100);
            }

            //Kind of like a sleep() function for the loop.  Very necessary.
            yield return new WaitForSeconds(decisionTime);
        }
    }

    //Utility function to make the cat walk to a "random" postion.
    private void chooseRandomTarget()
    {
        if (randomPositions.Count == 0) return;

        Debug.Log("choosing a random position. Curiosity: " + curiosity);

        //Just picks a pseudo-random point on the mesh to walk to.
        //Right now, the cat can choose the same spot over and over, which is kinda lame.
        var target = randomPositions[Random.Range(0, randomPositions.Count - 1)];
        agent.destination = target.position;
    }

    //Utility function to make the cat walk to a hazard.
    private IEnumerator ApproachHazard()
    {
        Debug.Log("I have chosen death. Curiosity: " + curiosity);

        //Targets a random hazard to go "investigate."
        var target = goalPositions[Random.Range(0, goalPositions.Count - 1)];

        agent.destination = target.position;

        while (Vector3.Distance(transform.position, target.position) > agent.stoppingDistance)
        {
            //Waits for the cat to get to where its going.
            //Was necessary when the cat's state changed to distress inside here, and now
            // I'm too afraid to change it.
            yield return new WaitForEndOfFrame();
        }
        //setState(CatState.DISTRESS);
    }

    //Utility function that waits for nine seconds, then changes the cat's
    // state to DEAD
    private IEnumerator CatInTrouble()
    {
        Debug.Log("I'm hella distressed!");
        yield return new WaitForSeconds(distressTime);
        setState(CatState.DEAD);

    }

    //A help function for when the player tries to rescue the cat.
    public void help()
    {
        //Each time help() is called this increments by nine.
        playerHelp += 9;

        //Once it hits the HELP_POINTS threshhold, the cat is rescued!
        if (playerHelp >= HELP_POINTS)
        {
            rescue();
        }
    }

    //This function is nearly identical to the cat dying, but respawns a new
    // cat at the spawn point that represents the rescued cat.
    public void rescue()
    {
        Debug.Log("Rescued!");
        StopAllCoroutines();
        hazard.occupied = false;
        catSpawner.spawnCat();
        Destroy(this.gameObject);
    }
}