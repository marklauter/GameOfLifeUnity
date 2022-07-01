using System;
using System.Collections.Generic;
using UnityEngine;

// https://en.wikipedia.org/wiki/Conway's_Game_of_Life
public class Simulation
{
    public long generations = 0;

    public readonly int width;
    public readonly int height;
    public readonly int span;
    // recommended values 5 to 15
    public readonly int chanceOfLife;

    private readonly List<(int offset, byte value)> changedCells;
    private readonly byte[] cells;
    private readonly int[][] neighborOffsets;
    private HashSet<int> activeCells = new();
    private readonly System.Random rng = new System.Random(DateTime.UtcNow.Millisecond);
    private const byte alive = 0xFF;
    private const byte dead = 0x00;

    public Simulation(int width, int height, int chanceOfLife)
    {
        this.width = width;
        this.height = height;
        this.chanceOfLife = chanceOfLife;

        span = width * height;
        cells = new byte[span];
        neighborOffsets = new int[span][];
        changedCells = new List<(int offset, byte value)>(span);

        PrecomputeNeighborOffsets();
        LetThereBeLight();
    }

    public byte[] GenerateFrame()
    {
        ApplyRules();
        ++this.generations;
        return cells;
    }

    private void LetThereBeLight()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                cells[y * width + x] = rng.NextDouble() * 100 <= chanceOfLife
                    ? (byte)0xFF
                    : (byte)0x00;
            }
        }

        InitializeActiveCells();
    }

    private void InitializeActiveCells()
    {
        for (int i = 0; i < span; ++i)
        {
            if (cells[i] == 0xFF)
            {
                _ = activeCells.Add(i);
            }
        }
    }

    private void PrecomputeNeighborOffsets()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int xmin = x > 0 ? x - 1 : width - 1;
                int xmax = x < width - 1 ? x + 1 : 0;
                int ymin = y > 0 ? y - 1 : height - 1;
                int ymax = y < height - 1 ? y + 1 : 0;
                int currentPixel = y * width + x;

                neighborOffsets[currentPixel] = new int[8];
                neighborOffsets[currentPixel][0] = xmin + width * ymin;
                neighborOffsets[currentPixel][1] = xmin + width * y;
                neighborOffsets[currentPixel][2] = xmin + width * ymax;
                neighborOffsets[currentPixel][3] = xmax + width * ymin;
                neighborOffsets[currentPixel][4] = xmax + width * y;
                neighborOffsets[currentPixel][5] = xmax + width * ymax;
                neighborOffsets[currentPixel][6] = x + width * ymin;
                neighborOffsets[currentPixel][7] = x + width * ymax;
            }
        }
    }

    private void ApplyRules()
    {
        HashSet<int> newActiveCells = new();
        changedCells.Clear();
        HashSet<int> visited = new();

        // outer loop all the active cells and inner loop all their neighbors
        // this ignores dead regions and saves time
        foreach (int cellOffset in activeCells)
        {
            int offset = cellOffset;
            int[] outerLocalNeighborOffsets = neighborOffsets[offset];

            if (!visited.Contains(offset))
            {
                _ = visited.Add(offset);

                byte originalValue = cells[offset];
                int liveNeighborCount =
                    // left column
                    (cells[outerLocalNeighborOffsets[0]]
                    + cells[outerLocalNeighborOffsets[1]]
                    + cells[outerLocalNeighborOffsets[2]]
                    // right colum
                    + cells[outerLocalNeighborOffsets[3]]
                    + cells[outerLocalNeighborOffsets[4]]
                    + cells[outerLocalNeighborOffsets[5]]
                    // center column (excluding current pixel)
                    + cells[outerLocalNeighborOffsets[6]]
                    + cells[outerLocalNeighborOffsets[7]])
                    / alive;

                byte newValue = (byte)(
                    (((((liveNeighborCount >> 2) ^ 0x01) << 1) & liveNeighborCount) >> 1)
                    * ((liveNeighborCount & 0x01) | (originalValue & 0x01))
                    * alive);

                if(newValue == dead && rng.NextDouble() * 100 <= 0.015)
                {
                    newValue ^= alive;
                }

                if (originalValue != newValue)
                {
                    changedCells.Add((offset, newValue));
                }

                if (newValue == 0xFF)
                {
                    _ = newActiveCells.Add(offset);
                }
            }

            for (int j = 0; j < 8; ++j)
            {
                offset = outerLocalNeighborOffsets[j];

                if (!visited.Contains(offset))
                {
                    _ = visited.Add(offset);

                    byte originalValue = cells[offset];
                    int[] innerLocalNeighborOffsets = neighborOffsets[offset];
                    int liveNeighborCount =
                        // left column
                        (cells[innerLocalNeighborOffsets[0]]
                        + cells[innerLocalNeighborOffsets[1]]
                        + cells[innerLocalNeighborOffsets[2]]
                        // right colum
                        + cells[innerLocalNeighborOffsets[3]]
                        + cells[innerLocalNeighborOffsets[4]]
                        + cells[innerLocalNeighborOffsets[5]]
                        // center column (excluding current pixel)
                        + cells[innerLocalNeighborOffsets[6]]
                        + cells[innerLocalNeighborOffsets[7]])
                        / 0xFF;

                    byte newValue = (byte)(
                        (((((liveNeighborCount >> 2) ^ 0x01) << 1) & liveNeighborCount) >> 1)
                        * ((liveNeighborCount & 0x01) | (originalValue & 0x01))
                        * 0xFF);

                    if (rng.NextDouble() * 100 <= 0.015)
                    {
                        newValue ^= alive;
                    }

                    if (originalValue != newValue)
                    {
                        changedCells.Add((offset, newValue));
                    }

                    if (newValue == 0xFF)
                    {
                        _ = newActiveCells.Add(offset);
                    }
                }
            }
        }

        // update the cell values only for changed
        foreach ((int offset, byte value) in changedCells)
        {
            cells[offset] = value;
        }

        activeCells = newActiveCells;
    }
}

