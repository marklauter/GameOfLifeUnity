using UnityEngine;

public class CubeUI
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

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.left, 10 * Time.deltaTime);
    }
}
