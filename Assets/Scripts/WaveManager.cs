using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Wave
{
    public Wave(GameObject enemy, int num)
    {
        emType = enemy;
        numEm = num;
    }
    public GameObject emType;
    public int numEm;
}

public class WaveManager : MonoBehaviour
{ 
    private SpawnManager spawner;

    public GameObject[] enemies; //array, so we can add in unity UI

    public List<Wave> level = new List<Wave>();

    public bool finishedLevel = false;

    public bool finishedGame = false;

    // Start is called before the first frame update
    void Start()
    {
        spawner = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();

        StartCoroutine((generateLevel()));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator generateLevel()
    {
        while (spawner.waveNum < 30)
        {
            yield return new WaitUntil(() => spawner.nextWave); // wait until we're ready for next wave;
            level.Clear();
            finishedLevel = false;
            int points = (int)(1 + (spawner.waveNum * 1.0f)); // inital points = 1 + wave number * 1.0
            points += spawner.waveNum / 10; // for every 10 waves add an extra point
            randomWaves(points);
            finishedLevel = true;
            yield return new WaitForSeconds(1);
        }
    }
    void randomWaves(int spendingPoints)
    {
        List<GameObject> validEms = new List<GameObject>();
        int totalCost = 0;
        int avgCost = 0;
        int lowestCost = 100;

        // first filter out enemies that can't be spawned due to point cost
        foreach(GameObject obj in enemies)
        {
            if (spawner.waveNum == obj.GetComponent<Enemy>().unlockRound && obj.GetComponent<Enemy>().soloOnUnlock)
            {
                Debug.Log("Unlock round for: " + obj);
                int numSolo = (spendingPoints / obj.GetComponent<Enemy>().enemyCost == 0 ? 1 : spendingPoints / obj.GetComponent<Enemy>().enemyCost);
                level.Add(new Wave(obj, numSolo)); // solo with this enemy
                return;
            }
            // Second conditional filters out low level enemies with higher wave numbers
            else if (obj.GetComponent<Enemy>().enemyCost <= spendingPoints && obj.GetComponent<Enemy>().obsoleteRound > spawner.waveNum)
            {
                
                if (spawner.waveNum > obj.GetComponent<Enemy>().unlockRound)
                {
                    validEms.Add(obj);
                    totalCost += obj.GetComponent<Enemy>().enemyCost;

                    lowestCost = (obj.GetComponent<Enemy>().enemyCost < lowestCost ? obj.GetComponent<Enemy>().enemyCost : lowestCost);
                }
            }
        }

        //Wave newWave = new Wave(validEms[0], 1);
        //level.Add(newWave);
        List<GameObject> removalList = new List<GameObject>();

        while (spendingPoints > 0 && validEms.Count != 0)
        {
            bool foundEnemy = false;
            avgCost = totalCost / validEms.Count;

            Debug.Log("The total amount of valid enemies available is: " + validEms.Count + "\nOur spending points total is: " + spendingPoints + "\nThe total cost of all available enemies is: " + totalCost + "\nThe avg cost of all available enemies is: " + avgCost);

            GameObject waveEnemy = new GameObject();
            int waveEnemyNum = 0;
            int cost = 0;
            int maxNum = 0; // maximum number of enemies able to be spawned 

            while (!foundEnemy)
            {
                waveEnemy = validEms[Random.Range(0, validEms.Count)]; // choose from list of valid enemies
                validEms.Remove(waveEnemy); // no longer use this enemy

                cost = waveEnemy.GetComponent<Enemy>().enemyCost;
                maxNum = spendingPoints / cost; // maximum number of enemies able to be spawned 


                if (validEms.Count == 0)
                {
                    waveEnemyNum = maxNum; // use all remaining points if this is last enemy
                    foundEnemy = true;
                    continue;
                }

                for (int i = maxNum; i > 0; i--)
                {
                    foreach (GameObject obj in validEms)
                    {
                        if ((spendingPoints - (i * cost)) % obj.GetComponent<Enemy>().enemyCost == 0) // we want to be able to use this enemy while still having having points for enemies left over or being left at 0
                        {
                            foundEnemy = true;
                            maxNum = i;
                            break;
                        }
                    }
                    if (foundEnemy == true)
                    {
                        break;
                    }
                }
                
            }
            Debug.Log("Enemy for this wave is: " + waveEnemy + "\nmax number of this enemy is: " + maxNum);


            if (maxNum == 1)
            {
                waveEnemyNum = maxNum;
            }
            else if (cost < avgCost)
            {
                waveEnemyNum = maxNum / Random.Range(2, maxNum + 1); // generally use less of this enemy to allow for more expensive enemies
            }
            else
            {
                waveEnemyNum = maxNum / Random.Range(1, maxNum + 1);
            }

            totalCost -= cost;
            spendingPoints -= waveEnemyNum * cost;

            
            //Debug.Log("added wave of" + waveEnemy);

            
            foreach (GameObject obj in validEms)
            {
                if (obj.GetComponent<Enemy>().enemyCost > spendingPoints)
                {
                    removalList.Add(obj);
                    totalCost -= obj.GetComponent<Enemy>().enemyCost;
                }
            }
            foreach (GameObject obj in removalList) { validEms.Remove(obj); } //remove enemies that can no longer be spawned

            if (waveEnemyNum < maxNum && validEms.Count == 0)
            {
                waveEnemyNum = maxNum; // final check, if there are no more valid enemies to spawn, add as many of current enemy to wave as possible
            }
            level.Add(new Wave(waveEnemy, waveEnemyNum)); // Add this wave to level

        }
    }
}
