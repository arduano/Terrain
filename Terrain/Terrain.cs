using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terrain
{
    public class Terrain
    {
        public short[,] blocks;
        public short[,] walls;
        public Terrain(int w, int h)
        {
            blocks = new short[w, h];
            walls = new short[w, h];
            Generate.GenerateTerrain(blocks, walls, (int)DateTime.Now.Ticks);
        }
    }
}
