using System;
using System.Collections.Generic;

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
    private readonly System.Random rng = new(DateTime.UtcNow.Millisecond);
    private const byte alive = 0xFF;
    private const byte dead = 0x00;

    public Simulation(int width, int height, int chanceOfLife)
    {
        this.width = width;
        this.height = height;
        this.chanceOfLife = chanceOfLife;

        this.span = width * height;
        this.cells = new byte[this.span];
        this.neighborOffsets = new int[this.span][];
        this.changedCells = new List<(int offset, byte value)>(this.span);

        this.PrecomputeNeighborOffsets();
        this.LetThereBeLight();
    }

    public List<(int offset, byte value)> GenerateFrame()
    {
        this.ApplyRules();
        ++this.generations;
        return this.changedCells;
    }

    private void LetThereBeLight()
    {
        for (var x = 0; x < this.width; ++x)
        {
            for (var y = 0; y < this.height; ++y)
            {
                this.cells[y * this.width + x] = this.rng.NextDouble() * 100 <= this.chanceOfLife
                    ? (byte)0xFF
                    : (byte)0x00;
            }
        }

        this.InitializeActiveCells();
    }

    private void InitializeActiveCells()
    {
        for (var i = 0; i < this.span; ++i)
        {
            if (this.cells[i] == 0xFF)
            {
                _ = this.activeCells.Add(i);
            }
        }
    }

    private void PrecomputeNeighborOffsets()
    {
        for (var x = 0; x < this.width; ++x)
        {
            for (var y = 0; y < this.height; ++y)
            {
                var xmin = x > 0 ? x - 1 : this.width - 1;
                var xmax = x < this.width - 1 ? x + 1 : 0;
                var ymin = y > 0 ? y - 1 : this.height - 1;
                var ymax = y < this.height - 1 ? y + 1 : 0;
                var currentPixel = y * this.width + x;

                this.neighborOffsets[currentPixel] = new int[8];
                this.neighborOffsets[currentPixel][0] = xmin + this.width * ymin;
                this.neighborOffsets[currentPixel][1] = xmin + this.width * y;
                this.neighborOffsets[currentPixel][2] = xmin + this.width * ymax;
                this.neighborOffsets[currentPixel][3] = xmax + this.width * ymin;
                this.neighborOffsets[currentPixel][4] = xmax + this.width * y;
                this.neighborOffsets[currentPixel][5] = xmax + this.width * ymax;
                this.neighborOffsets[currentPixel][6] = x + this.width * ymin;
                this.neighborOffsets[currentPixel][7] = x + this.width * ymax;
            }
        }
    }

    private void ApplyRules()
    {
        HashSet<int> newActiveCells = new();
        this.changedCells.Clear();
        HashSet<int> visited = new();

        // outer loop all the active cells and inner loop all their neighbors
        // this ignores dead regions and saves time
        foreach (var cellOffset in this.activeCells)
        {
            var offset = cellOffset;
            var outerLocalNeighborOffsets = this.neighborOffsets[offset];

            if (!visited.Contains(offset))
            {
                _ = visited.Add(offset);

                var originalValue = this.cells[offset];
                var liveNeighborCount =
                    // left column
                    (this.cells[outerLocalNeighborOffsets[0]]
                    + this.cells[outerLocalNeighborOffsets[1]]
                    + this.cells[outerLocalNeighborOffsets[2]]
                    // right colum
                    + this.cells[outerLocalNeighborOffsets[3]]
                    + this.cells[outerLocalNeighborOffsets[4]]
                    + this.cells[outerLocalNeighborOffsets[5]]
                    // center column (excluding current pixel)
                    + this.cells[outerLocalNeighborOffsets[6]]
                    + this.cells[outerLocalNeighborOffsets[7]])
                    / alive;

                var newValue = (byte)(
                    (((((liveNeighborCount >> 2) ^ 0x01) << 1) & liveNeighborCount) >> 1)
                    * ((liveNeighborCount & 0x01) | (originalValue & 0x01))
                    * alive);

                //if(newValue == dead && rng.NextDouble() * 100 <= 0.015)
                //{
                //    newValue ^= alive;
                //}

                if (originalValue != newValue)
                {
                    this.changedCells.Add((offset, newValue));
                }

                if (newValue == 0xFF)
                {
                    _ = newActiveCells.Add(offset);
                }
            }

            for (var j = 0; j < 8; ++j)
            {
                offset = outerLocalNeighborOffsets[j];

                if (!visited.Contains(offset))
                {
                    _ = visited.Add(offset);

                    var originalValue = this.cells[offset];
                    var innerLocalNeighborOffsets = this.neighborOffsets[offset];
                    var liveNeighborCount =
                        // left column
                        (this.cells[innerLocalNeighborOffsets[0]]
                        + this.cells[innerLocalNeighborOffsets[1]]
                        + this.cells[innerLocalNeighborOffsets[2]]
                        // right colum
                        + this.cells[innerLocalNeighborOffsets[3]]
                        + this.cells[innerLocalNeighborOffsets[4]]
                        + this.cells[innerLocalNeighborOffsets[5]]
                        // center column (excluding current pixel)
                        + this.cells[innerLocalNeighborOffsets[6]]
                        + this.cells[innerLocalNeighborOffsets[7]])
                        / 0xFF;

                    var newValue = (byte)(
                        (((((liveNeighborCount >> 2) ^ 0x01) << 1) & liveNeighborCount) >> 1)
                        * ((liveNeighborCount & 0x01) | (originalValue & 0x01))
                        * 0xFF);

                    //if (this.rng.NextDouble() * 100 <= 0.015)
                    //{
                    //    newValue ^= alive;
                    //}

                    if (originalValue != newValue)
                    {
                        this.changedCells.Add((offset, newValue));
                    }

                    if (newValue == 0xFF)
                    {
                        _ = newActiveCells.Add(offset);
                    }
                }
            }
        }

        // update the cell values only for changed
        foreach ((var offset, var value) in this.changedCells)
        {
            this.cells[offset] = value;
        }

        this.activeCells = newActiveCells;
    }
}

