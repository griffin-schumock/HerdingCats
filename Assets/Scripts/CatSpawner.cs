using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatSpawner : MonoBehaviour
{
    //Creates all the objects the cat will interact with.
    public List<Transform> randomPositions;
    public List<Transform> goalPositions;
    public Hazard hazard;

    //The spawn rate.
    private const float SPAWN_RATE = 30;

    //The Cat prefab.
    public CatAI catPrefab;

    //Instantiates the cat prefab, and gives it the objects it will interact with.
    public void spawnCat()
    {
        CatAI catAI = Instantiate(catPrefab, transform.position, Quaternion.identity);
        catAI.setSpawner(this);
        catAI.setPositions(randomPositions, goalPositions, hazard);
    }

    //Calls the spawning loop at startup.
    private void Start()
    {
        StartCoroutine("spawnTimer");
    }

    //At an interval of SPAWN_RATE, this loop will spawn a new cat prefab!
    //At this time, there is no breakout case.  There probly won't be one,
    // since it's a survival game ;)
    private IEnumerator spawnTimer()
    {
        while (true)
        {
            spawnCat();
            yield return new WaitForSeconds(SPAWN_RATE);
        }
    }
}
