using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Paleolithic_Cooperation
{
    class GridDrawer
    {
        private DPanel target;
        private Graphics gr;
        private Pen pen;
        private Font font;

        private int gridW, gridH;
        private float cellW, cellH;

        public Color penColor {
            get { return pen.Color; }
            set { pen.Color = value; }
        }

        public float fontSize {
            get { return font.Size; }
            set { font = new Font(new FontFamily("Arial"), value); }
        }

        public GridDrawer(DPanel dp) {
            target = dp;
            target.DBpanelgr.CreateDoubleBuffer(target.CreateGraphics(), target.ClientRectangle.Width, target.ClientRectangle.Height);

            gr = target.DBpanelgr.g;
            pen = new Pen(Color.Black);
            Font font = new Font(new FontFamily("Arial"), 8);

            gridW = Environment.envW;
            gridH = Environment.envH;

            cellW = (float)target.ClientRectangle.Width / gridW;
            cellH = (float)target.ClientRectangle.Height / gridH;

            Utils.imgW = (int)(cellW - 1);
            Utils.imgH = (int)(cellH - 1);
        }

        public void clear() {
            try
            {
                if (gr != null)
                    gr.Clear(Color.White);
                else
                {
                    target.DBpanelgr.CreateDoubleBuffer(target.CreateGraphics(), target.ClientRectangle.Width, target.ClientRectangle.Height);
                    gr = target.DBpanelgr.g;
                    if (gr != null) gr.Clear(Color.White);
                }
            } catch (Exception ex) { }
        }

        public void drawString(string str, Color col, float left, float top)
        {
            try {
                gr.DrawString(str, font, new SolidBrush(col), left, top);
            }
            catch (Exception ex) { }
        }

        public void drawLine(float x1, float y1, float x2, float y2)
        {
            try {
                gr.DrawLine(pen, x1, y1, x2, y2);
            } catch (Exception ex) { }
        }

        public void drawGrid() {
            try
            {
                for (int i = 0; i < gridW; i++)
                {
                    for (int j = 0; j < gridH; j++)
                    {
                        drawLine((int)(i * cellW), 0, (int)(i * cellW), target.ClientSize.Height);
                        drawLine(0, (int)(j * cellH), target.ClientSize.Width, (int)(j * cellH));
                    }
                }
                drawLine(target.ClientSize.Width - 1, 0, target.ClientSize.Width - 1, target.ClientSize.Height);
                drawLine(0, target.ClientSize.Height - 1, target.ClientSize.Width, target.ClientSize.Height - 1);
            }
            catch (Exception ex) { }
        }

        public bool drawImg(Image img, float left, float top, float w, float h, Color bgCol) {
            try
            {
                if (img == null || gr == null) return false;
                gr.FillRectangle(new SolidBrush(bgCol), left - 1, top - 1, Utils.imgW + 1, Utils.imgH + 1);
                gr.DrawImage(img, left, top, (float)w, (float)h);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        public bool drawImg(string src, int x, int y, Color bgCol)
        {
            try
            {
                float left = x * cellW + 1;
                float top = y * cellH + 1;
                drawImg(Utils.getImg(src), left, top, Utils.imgW, Utils.imgH, bgCol);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        public void draw(Entity d) {
            try
            {
                float left = d.x * cellW + 1;
                float top = d.y * cellH + 1;
                drawImg(d.Image, left, top, Utils.imgW, Utils.imgH, d.getColor());
            }
            catch (Exception ex) { }
        }

        public void render()
        {
            target.DBpanelgr.Render(target.CreateGraphics());
        }
    }
}
