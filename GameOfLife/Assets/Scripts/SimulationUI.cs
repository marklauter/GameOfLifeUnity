using UnityEngine;

public class SimulationUI
    : MonoBehaviour
{
    private GameOfLife gameOfLife;

    // Start is called before the first frame update
    private void Start()
    {
        gameOfLife = FindObjectOfType<GameOfLife>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            gameOfLife.simulation = new Simulation(
                gameOfLife.simulation.width,
                gameOfLife.simulation.height,
                gameOfLife.simulation.chanceOfLife);
        }
    }
}
