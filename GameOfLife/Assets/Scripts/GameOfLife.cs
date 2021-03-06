using UnityEngine;

public class GameOfLife
    : MonoBehaviour
{
    public int width;
    public int height;
    public int chanceOfLife;

    public Simulation simulation;
    private SimulationDisplay simulationDisplay;

    private void Start()
    {
        simulation = new Simulation(width, height, chanceOfLife);
        simulationDisplay = FindObjectOfType<SimulationDisplay>();
        simulationDisplay.SetSimulation(simulation);
    }

    private void Update()
    {
        var cells = simulation.GenerateFrame();
        simulationDisplay.DrawSimulation(cells);
    }
}
