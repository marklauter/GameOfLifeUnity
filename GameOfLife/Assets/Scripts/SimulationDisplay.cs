using UnityEngine;

public class SimulationDisplay 
    : MonoBehaviour
{
    public Renderer simulationRenderer;
    public Simulation simulation;

    private Color32[] bitmap;

    public void DrawSimulation(byte[] cells)
    {
        var texture = (Texture2D)simulationRenderer.sharedMaterial.mainTexture;

        for (int i = 0; i < simulation.span; ++i)
        {
            var cell = cells[i];
            bitmap[i] = new Color32(cell, cell, cell, 255);
        }

        texture.SetPixels32(bitmap);
        texture.Apply();
    }

    internal void SetSimulation(Simulation simulation)
    {
        this.simulation = simulation;
        var texture = new Texture2D(simulation.width, simulation.height);
        simulationRenderer.sharedMaterial.mainTexture = texture;
        bitmap = new Color32[simulation.span];
    }
}
