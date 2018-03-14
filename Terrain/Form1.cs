using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Terrain
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Terrain terrain;
        int width = 2100;//4200;
        int height = 1200;


        private void generateTerrain()
        {
            terrain = new Terrain(width, height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();

            Thread generate = new Thread(generateTerrain);
            generate.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            progressBar1.Value = Generate.ProgressPart;
            progressBar2.Value = (int)(Generate.Progress * 100);
            if (Generate.Completed)
            {
                Bitmap map = new Bitmap(width, height);
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        short b = terrain.blocks[i, j];
                        short w = terrain.walls[i, j];
                        Color c = new Color();
                        if (b == 0)
                        {
                            c = Color.White;
                            if (w == 0)
                            {
                                if (j > height / 5) c = Color.SaddleBrown;
                                if (j > height / 5 * 2) c = Color.FromArgb(80, 80, 80);
                            }
                            if (w == 1) c = Color.FromArgb(100, 50, 0);
                        }
                        if (b == 1) c = Color.SaddleBrown;
                        if (b == 2) c = Color.Gray;
                        if (b == 3) c = Color.Green;
                        if (b == 4) c = Color.SandyBrown;
                        if (b == 5) c = Color.LightGray;
                        if (b == 6) c = Color.Orange;
                        if (b == 7) c = Color.Black;
                        if (b == 8) c = Color.DarkGray;
                        if (b == 9) c = Color.Gold;
                        if (b == 10) c = Color.Silver;
                        map.SetPixel(i, j, c);
                    }
                }
                map.Save("map.png", System.Drawing.Imaging.ImageFormat.Png);
                map.Dispose();
                System.Diagnostics.Process.Start("map.png");
                Application.Exit();
            }
        }
    }
}
