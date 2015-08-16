using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Paleolithic_Cooperation
{
    class ChartDrawer
    {
        private Panel pnlChart;

        private Environment environment;

        public ChartDrawer(Panel pnl, Environment env) {
            pnlChart = pnl;
            environment = env;
        }

        public void draw() {
            List<int[]> countData = environment.strategyCountHistory;
            if (countData == null || countData.Count < 2) return;

            Graphics gr = pnlChart.CreateGraphics();
            gr.Clear(Color.Black);
            int y0 = pnlChart.Height + 1;
            int x0 = pnlChart.Width - 1;
            double scale = 1;
            int max = 0;
            int i, j;

            for (j = countData.Count - 1; j >= 0 && j > countData.Count - pnlChart.Width; j--)
                for (i = 0; i < countData[j].Length; i++)
                {
                    if (countData[j][i] > max) max = countData[j][i];
                }

            if (max == 0) scale = 1;
            else scale = (double)(y0 - 2) / max;

            Color[] colors = new Color[] {Color.LightBlue, Color.Yellow, Color.SteelBlue, Color.Chocolate, Color.Red};
            int x, y, prevx, prevy;
            float width;
            for (i = countData.Count - 2; i > countData.Count - pnlChart.Width && i >= 0; i--)
            {
                for (j = 0; j < countData[i].Length; j++)
                {
                    prevx = x0 - (countData.Count - 1 - i) + 1;
                    prevy = y0 - (int)(countData[i + 1][j] * scale);
                    x = x0 - (countData.Count - 1 - i);
                    y = y0 - (int)(countData[i][j]*scale);
                    width = 1;
                    if (pnlChart.Width > 200) width = 2;
                    gr.DrawLine(new Pen(colors[j], width), prevx, prevy, x, y);
                }
            }
        }
    }
}
