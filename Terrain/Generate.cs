using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terrain
{
    public static class Generate
    {
        //Block IDs
        //001 Dirt
        //002 Stone
        //003 Grass
        //004 Tree
        //005 Iron
        //006 Copper
        //007 Tungsten
        //008 Titanium
        //009 Gold
        //010 Platnum

        public static bool Completed = false;
        public static int ProgressPart;
        public static double Progress;

        static Perlin perlin = new Perlin();
        public static void GenerateTerrain(short[,] blocks, short[,] walls, int seed)
        {
            perlin.Randomize(seed);
            Random r = new Random(seed);
            int width = blocks.GetLength(0);
            int height = blocks.GetLength(1);

            ProgressPart = 1;
            #region Stone, dirt and surface grass + Hills
            List<int> hillcaves = new List<int>();
            Parallel.For(1, width / 150, i => 
            {
                if (r.Next(6) > 3) hillcaves.Add(150 * i);
            });
            Parallel.For(0, width, x =>
            {
                Progress = (float)x / width;
                int i = (int)(perlin.OctavePerlin((double)x / 150 * ((double)Math.Abs(x - width / 2) / (double)width * 2 * 0.3 + 1), 0, 0, 8, 0.5) * 200) - 50;
                int hill = 0;
                for (int j = 0; j < hillcaves.Count; j++)
                {
                    if (Math.Abs(hillcaves[j] - x) < 16)
                    {
                        hill = (int)(Math.Sin(Math.Acos((double)(x - hillcaves[j]) / 15)) * 15);
                    }
                }
                i -= hill;
                int i2 = (int)(perlin.OctavePerlin((double)x / 300, 5.1, 0, 8, 2) * 20);
                for (int n = i + height / 10; n <= i + height / 10 + i2; n++)
                {
                    blocks[x, n] = 1;
                }
                blocks[x, i + height / 10] = 3;
                for (int y = i + height / 10 + 1; y < height / 5 * 2 + 30; y++)
                {
                    walls[x, y] = 1;
                }
                for (int y = 0; y < height; y++)
                {
                    short b = blocks[x, y];
                    int decreasor = (int)(i + height / 10 * 1.5f) - y;
                    if (decreasor < 0) decreasor = 0;
                    int i3 = (int)(perlin.OctavePerlin((double)x / 1000, (double)y / 1000, 0, 7, 1.5) * 200) - decreasor;
                    if (i3 < 0) i3 = 0;
                    if (blocks[x, y] == 0 && y > i + height / 10 + i2)
                    {
                        if (i3 + y / 3 - i - 10 < 105) b = 1;
                        else if (i3 < 105) b = 2;
                    }
                    if (i3 > 140) b = 0;
                    blocks[x, y] = b;
                }
            });
            #endregion

            ProgressPart = 2;
            #region Hill Caves
            Progress = 0;
            foreach (int hill in hillcaves)
            {
                Progress += (float)1 / hillcaves.Count;
                bool right;
                sbyte rightStart = 1;
                int pos;
                if (r.Next(2) > 0)
                {
                    pos = hill + 13;
                    right = false;
                }
                else
                {
                    pos = hill - 13;
                    right = true;
                }
                int yPos = 0;
                for (int y = 0; y < height; y++)
                {
                    if (Placeable(blocks[pos, y]))
                    {
                        yPos = y;
                        break;
                    }
                }
                if (!right) rightStart = -1;
                int yStart = yPos;

                for (int i = 0; i < r.Next(6, 15); i++)
                {
                    int yEnd = r.Next(30, 60);
                    int posStart = pos;
                    if (i == 0)
                    {
                        if (right) pos += 10;
                        else pos -= 10;
                        yEnd = 5;
                    }
                    else
                    {
                        if (right) pos += r.Next(10, 60);
                        else pos -= r.Next(10, 60);
                    }
                    for (float y = 0; y < (float)yEnd; y += 0.1f)
                    {
                        int multiplier = (int)(y + yPos - yStart) * 2;
                        if (multiplier > 100) multiplier = 100;
                        Circle(blocks, 0, (int)((pos - posStart) * (y / yEnd) + posStart) + (int)(perlin.OctavePerlin(0, (y + yPos) / 20, 0, 5, 0.2) * multiplier * rightStart), (int)(yPos + y), (int)(perlin.OctavePerlin(0, (y + yPos) / 10, 0, 6, 2) * 10 - 2));
                    }
                    yPos += yEnd;
                    right = !right;
                }
            }
            #endregion

            ProgressPart = 3;
            #region Smooth dirt walls
            Progress = 0;
            int loops = 20;
            for (int i = 0; i < loops + 1; i++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    Progress = ((float)x / width / loops) + (i / (loops + 1));
                    for (int y = 2; y < height / 5 * 3; y++)
                    {
                        if (walls[x, y] == 1 && blocks[x, y] == 0)
                        {
                            byte count = 0;

                            for (int x2 = -2; x2 < 3; x2++)
                            {
                                for (int y2 = -2; y2 < 3; y2++)
                                {
                                    if (walls[x + x2, y + y2] > 0) count++;
                                    if (Placeable(blocks[x + x2, y + y2])) count += 1;
                                }
                            }

                            if (count < 18) walls[x, y] = 0;
                        }
                    }
                }
            }
            #endregion

            ProgressPart = 4;
            #region Trees
            Progress = 0;
            for (int x = 50; x < width - 50; x += 7)
            {
                Progress = (float)x / width;
                int i = (int)(perlin.OctavePerlin((double)x / 60, 5, 0, 4, 1) * 32);
                if (i > 10 && r.NextDouble() > 0.6)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        if (blocks[x, y + 1] == 3)
                        {
                            bool b = false;
                            if (blocks[x - 1, y + 1] == 3)
                            {
                                blocks[x - 1, y] = 4;
                                b = true;
                            }
                            if (blocks[x + 1, y + 1] == 3)
                            {
                                blocks[x + 1, y] = 4;
                                b = true;
                            }
                            if (b)
                            {
                                byte branches = (byte)r.Next(1, 3);
                                for (int h = 0; h < i; h++)
                                {
                                    blocks[x, y - h] = 4;
                                    if (h % branches == 0 && h != 0 && i - h > 3)
                                    {
                                        byte count = (byte)r.Next(0, 8);
                                        if ((count == 1 || count == 3) && blocks[x - 1, y - h + 1] == 0) blocks[x - 1, y - h] = 4;
                                        if ((count == 2 || count == 3) && blocks[x + 1, y - h + 1] == 0) blocks[x + 1, y - h] = 4;
                                    }
                                }
                            }
                            break;
                        }
                        if (blocks[x, y + 1] > 0) break;
                    }
                }
            }
            #endregion

            ProgressPart = 5;
            #region Generate Ores
            Progress = 0;
            short[,] ores = new short[width, height];
            for (int x = 0; x < width; x++)
            {
                Progress = (float)x / width;
                for (int y = 0; y < height; y++)
                {
                    short b = blocks[x, y];
                    int i = (int)(perlin.OctavePerlin((double)x / 30, (double)y / 20, 5, 3, 3) * 200);
                    b = 2;
                    if (i > 130)
                    {
                        b = 0;
                    }
                    ores[x, y] = b;
                }
            }
            #endregion

            ProgressPart = 6;
            #region Apply Ores
            Progress = 0;
            for (int x = 0; x < width; x++)
            {
                Progress = (float)x / width;
                for (int y = 0; y < height; y++)
                {
                    if (ores[x, y] == 0)
                    {
                        short ore;
                        if (y < height / 5 * 2) ore = (short)r.Next(5, 7);
                        else ore = (short)r.Next(5, 11);
                        List<int> Xs = new List<int>();
                        List<int> Ys = new List<int>();
                        Xs.Add(x);
                        Ys.Add(y);
                        while (Xs.Count > 0)
                        {
                            for (int i = 0; i < Xs.Count; i++)
                            {
                                if (Xs[i] + 1 < width)
                                {
                                    if (ores[Xs[i] + 1, Ys[i]] == 0)
                                    {
                                        Xs.Add(Xs[i] + 1);
                                        Ys.Add(Ys[i]);
                                    }
                                }
                                if (Xs[i] - 1 >= 0)
                                {
                                    if (ores[Xs[i] - 1, Ys[i]] == 0)
                                    {
                                        Xs.Add(Xs[i] - 1);
                                        Ys.Add(Ys[i]);
                                    }
                                }
                                if (Ys[i] + 1 < height)
                                {
                                    if (ores[Xs[i], Ys[i] + 1] == 0)
                                    {
                                        Xs.Add(Xs[i]);
                                        Ys.Add(Ys[i] + 1);
                                    }
                                }
                                if (Ys[i] - 1 >= 0)
                                {
                                    if (ores[Xs[i], Ys[i] - 1] == 0)
                                    {
                                        Xs.Add(Xs[i]);
                                        Ys.Add(Ys[i] - 1);
                                    }
                                }
                                ores[Xs[i], Ys[i]] = ore;
                                Xs.RemoveAt(i);
                                Ys.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (blocks[x, y] == 2) blocks[x, y] = ores[x, y];
                }
            }
            #endregion

            ProgressPart = 7;
            #region Underground Dirt
            Progress = 0;
            short[,] dirt = new short[width, height];
            Parallel.For(0, width, x =>
            {
                Progress = (float)x / width;
                for (int y = 0; y < height; y++)
                {
                    short b = blocks[x, y];
                    int i = (int)(perlin.OctavePerlin((double)x / 30, (double)y / 20, 10, 5, 0.8) * 200);
                    b = 2;
                    if (i > 105)
                    {
                        b = 1;
                    }
                    dirt[x, y] = b;
                }
            });
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (blocks[x, y] == 2) blocks[x, y] = dirt[x, y];
                }
            }

            #endregion

            Completed = true;
        }

        private static void Circle(short[,] blocks, short block, int x, int y, int radius)
        {
            for (int i = -radius; i < radius + 1; i++)
            {
                for (int j = -radius; j < radius + 1; j++)
                {
                    if (Math.Sqrt(i * i + j * j) < radius)
                    {
                        if (i + x >= 0 && i + x < blocks.GetLength(0) && j + y >= 0 && j + y < blocks.GetLength(1)) blocks[i + x, j + y] = block;
                    }
                }
            }
        }

        private static bool Placeable(int block)
        {
            if (block == 0) return false;
            if (block == 4) return false;
            if (block == 0) return false;
            if (block == 0) return false;
            if (block == 0) return false;
            if (block == 0) return false;
            return true;
        }
    }
}
