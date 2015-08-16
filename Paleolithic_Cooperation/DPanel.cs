using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace Paleolithic_Cooperation
{
    /// <summary>
    /// Die DPanel Klasse gehört zu View, ist vom normalen Form-Element Panel abgeleitet
    /// und stellt ein DoubleBuffered Panel dar
    /// </summary>
    public partial class DPanel : Panel
    {
        public DBGraphics DBpanelgr;
        public bool blockRedraw;

        /// <summary>
        /// Konstruktor, setzt DoubleBuffered-Eigenschaften auf True und initialisiert das DBGraphics-Objekt
        /// </summary>
        public DPanel()
        {
            // Set the value of the double-buffering style bits to true.
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.UserPaint |
              ControlStyles.AllPaintingInWmPaint,
              true);

            this.DoubleBuffered = true;

            this.UpdateStyles();

            blockRedraw = false;

            DBpanelgr = new DBGraphics();
        }
        
        /// <summary>
        /// OnPaint rendert die DBGraphics-Graphik
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DBpanelgr.CanDoubleBuffer() && !blockRedraw)
            {
                DBpanelgr.Render(e.Graphics);
            }
        }
    }

}
