using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    public GameObject[] powerups;
    public int powerupSpawnCooldown;

    private WaveManager waver;
    private PlayerController player;

    public bool nextWave = true;

    private float spawnRange = 9;
    public int waveNum;
    public int maxWave;
    public bool endLevel;

    // Start is called before the first frame update
    void Start()
    {
        waveNum = 1;
        
        waver = GameObject.Find("Wave Manager").GetComponent<WaveManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();

        StartCoroutine(waveLauncher());
        StartCoroutine(powerupSpawner());
    }

    // Update is called once per frame
    void Update()
    {
        //if (FindObjectsOfType<Enemy>().Length < waves[0, waveNum] && !spawning)
        //{
        //    StartCoroutine(waveLauncher(++waveNum));
        //    spawning = true;
        //}
        
    }
    IEnumerator powerupSpawner()
    {
        while (!player.gameOver && !endLevel)
        {
            int numPowerups = GameObject.FindGameObjectsWithTag("Powerup").Length;

            float wait = Random.Range(powerupSpawnCooldown - 5.0f + (numPowerups * 3), powerupSpawnCooldown + 5.0f + (numPowerups * 3));
            yield return new WaitForSeconds(wait);

            if (numPowerups <= 3) // only 3 powerups at a time in the ring
            {
                Debug.Log("Spawn time for powerup was: " + wait + " seconds.");
                GameObject powerup = powerups[Random.Range(0, powerups.Length)];
                Instantiate(powerup, new Vector3(Random.Range(-spawnRange, spawnRange), -0.05f, Random.Range(-spawnRange, spawnRange)), powerup.transform.rotation);
            }
        }

    }

    IEnumerator waveLauncher()
    {
        while (!player.gameOver && !endLevel)
        {
            yield return new WaitUntil(() => waver.finishedLevel);
            nextWave = false;
            int totalEnemies = 0;
            foreach (Wave wave in waver.level)
            {
                spawnEnemyWave(wave.emType, wave.numEm);
                totalEnemies += wave.numEm;
            }
            Debug.Log("Level " + waveNum + ", there needs to be less than " + (totalEnemies / 3 + 1) + " enemies to spawn next level");
            yield return new WaitUntil(() => FindObjectsOfType<Enemy>().Length < totalEnemies / 3 + 1);
            //StartCoroutine(nextLevelCountdown()); // add this at some point
            Debug.Log("Waiting to spawn next wave");
            yield return new WaitForSeconds(5); // start countdown until next wave
            if (waveNum == maxWave) // for endless, maxWave is set to 0, so this is never true!
            {
                endLevel = true;
            }
            else
            {
                waveNum++;
                nextWave = true;
            }
            yield return new WaitForFixedUpdate();
        }

    }

    void spawnEnemyWave(GameObject enemy, int numEm) //, int enemyType)
    {
        for (int i = 0; i < numEm; i++)
        {
            Instantiate(enemy, new Vector3(Random.Range(-spawnRange, spawnRange), 0, Random.Range(-spawnRange, spawnRange)), enemy.transform.rotation);
        }
    }

    
    
}
