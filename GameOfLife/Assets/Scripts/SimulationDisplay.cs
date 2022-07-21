using System.Collections.Generic;
using UnityEngine;

public class SimulationDisplay
    : MonoBehaviour
{
    // public Renderer simulationRenderer;
    public Simulation simulation;
    public GameObject cellPrefab;

    private GameObject[] cellMap;

    public void DrawSimulation(List<(int offset, byte value)> cells)
    {
        for (var i = 0; i < simulation.span; ++i)
        {
            var go = this.cellMap[i];
            if (go != null)
            {
                var rb = go.GetComponent<Rigidbody>();
                rb.AddForce(Physics.gravity * 1.5F, ForceMode.Acceleration);
                rb.AddForce(Vector3.back * 6.75F, ForceMode.Impulse);
                rb.useGravity = true;
                var lifetime = Random.Range(0.1F, 5);
                Destroy(go, lifetime);
                this.cellMap[i] = null;
            }
        }

        for (var i = 0; i < cells.Count; ++i)
        {
            var (offset, value) = cells[i];
            if (value == 0xFF)
            {
                var y = offset / this.simulation.height;
                var x = offset - y * this.simulation.width;
                this.cellMap[offset] = Instantiate(this.cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
            }
        }
    }

    internal void SetSimulation(Simulation simulation)
    {
        this.simulation = simulation;
        this.cellMap = new GameObject[simulation.span];
    }
}
