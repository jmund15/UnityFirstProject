using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    public TextMeshProUGUI scoreTest;
    public TextMeshProUGUI gameOverText;

    public GameObject[] powerups;
    public int powerupSpawnCooldown;

    private WaveManager waver;
    private PlayerController player;

    public bool nextWave = true;

    private float spawnRange = 9;
    public int waveNum;
    private int oldWaveNum;
    public int score;
    private int oldScore;

    // TODO - CHANGE NAME TO GameManager

    // Start is called before the first frame update
    void Start()
    {
        waveNum = 1;
        score = 0;
        oldWaveNum = waveNum;
        oldScore = score;
        waver = GameObject.Find("Wave Manager").GetComponent<WaveManager>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        StartCoroutine(waveLauncher());
        StartCoroutine(powerupSpawner());
        scoreTest.text = "Wave: 1\nScore: 0";
        gameOverText.gameObject.SetActive(false);
        StartCoroutine(gameOver());

    }

    // Update is called once per frame
    void Update()
    {
        //if (FindObjectsOfType<Enemy>().Length < waves[0, waveNum] && !spawning)
        //{
        //    StartCoroutine(waveLauncher(++waveNum));
        //    spawning = true;
        //}
        if (waveNum != oldWaveNum || score != oldScore)
        {
            scoreTest.text = "Wave: " + waveNum + "\nScore: " + score;
            oldWaveNum = waveNum;
            oldScore = score;
        }
    }
    IEnumerator powerupSpawner()
    {
        while (!waver.finishedGame)
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
        while (!waver.finishedGame && !player.gameOver)
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
            waveNum++;
            nextWave = true;
            yield return new WaitForFixedUpdate();
        }

    }

    IEnumerator gameOver()
    {
        yield return new WaitUntil(() => player.gameOver || waver.finishedGame); // Wait until player falls off the stage or finished level

        if (player.gameOver)
        {
            nextWave = false;
            // IDEA - zoom in to player falling (in slow motion) while game over text fades into view, maybe add some sound effect, game over music as well
            gameOverText.gameObject.SetActive(true); // display game over text
        }
        else // player finished game/level
        {
            // IDEA - zoom in to player, jumping/rotating happily, confetti ensues, maybe all of the enemies he defeated are stacked up in the corner.
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
