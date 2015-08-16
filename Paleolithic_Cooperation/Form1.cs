using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Paleolithic_Cooperation.Objects;
using Paleolithic_Cooperation.Agents;

namespace Paleolithic_Cooperation
{
    public partial class Form1 : Form
    {
        private Environment environment;
        private ChartDrawer chartDrawer;
        private ChartDrawer bigChartDrawer;
        private Form frminf;
        private Panel bigPanel;
        private bool running;

        public const int WIDTH = 50, HEIGHT = 30;

        public Form1()
        {
            environment = null;
            running = false;

            Utils.initImgs();

            InitializeComponent();

            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (environment != null)
            {
                environment.draw();
               if (chartDrawer != null)  chartDrawer.draw();
            }
            else
            {
                environment = new Environment(pnlContainer, WIDTH, HEIGHT);
                environment.showLastCounts = false;
            }
        }

        private int getNumber(TextBox txt) {
            return getNumber(txt, 0, int.MaxValue);
        }
        private int getNumber(TextBox txt, int max) { 
            return getNumber(txt, 0, max);
        }
        private int getNumber(TextBox txt, int min, int max)
        {
            return getNumber(txt, min, max, int.MinValue);
        }
        private int getNumber(TextBox txt, int min, int max, int errorcode) {
            try
            {
                int n = int.Parse(txt.Text);
                if (n < min || n > max) {
                    MessageBox.Show("Out of range! Please enter an integer between " + min + " and " + max);
                    return errorcode;
                }
                else return n;
            }
            catch (Exception ex) {
                MessageBox.Show("Bad value! Please enter an integer between " + min + " and " + max);
                return errorcode;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();

            int max = WIDTH * HEIGHT;
            int humans = getNumber(txtHumans, max);
            int mammoths = getNumber(txtMammoths, max);
            int stones = getNumber(txtObstacles, max);
            int plants = getNumber(txtPlants, max);

            environment.init(pnlContainer, WIDTH, HEIGHT);
            environment.initGrid(WIDTH, HEIGHT);
            environment.steps = 0;
            lblStep.Text = "0";

            initParams();

            bool success = true;
            success &= environment.addRandom(new Human(environment), humans);
            success &= environment.addRandom(new Mammoth(environment), mammoths);
            success &= environment.addRandom(new Stone(environment), stones);
            success &= environment.addRandom(new Plant(environment), plants);

            chartDrawer = new ChartDrawer(pnlGraph, environment);

            if (!success)
            {
                environment = null;
                MessageBox.Show("Failed to initialize environment.");
            }
            else
            {
                environment.draw();
                chartDrawer.draw();

                environment.lastCounts = "";
                environment.lastActions = "";
                showLastActions();
            }
        }

        private void initParams() {
            Mammoth.reproductionThreshold = getNumber(txtMammothRThreshold, 1, 500, 100);
            Mammoth.initialPayoff = 70;
            Mammoth.maxMammothAge = getNumber(txtMammothMaxAge, 1, 10000, 100);
            Plant.maxPlantVal = getNumber(txtMaxplantval, 1, 500, 50);
            Plant.maxPlantAge = getNumber(txtMaxPlantAge, 1, 10000, 100);
            Plant.plantRegrowAfter = getNumber(txtRegrow, 1, 500, 60);
            Plant.plantsReproduceAfter = getNumber(txtPlantsReproduce, 1, 500, 25) * 2;

            Human.reproductionThreshold = getNumber(txtHReproductionThreshold, 1, 500, 300);
            Human.reproductionCost = getNumber(txtHReproductionCost, 1, 500, 300);
            Human.maxHumanAge = getNumber(txtHMaxAge, 1, 1000, 200);
            Human.hunterMaxDamage = getNumber(txtHunterDmg, 1, 500, 50);
            try
            {
                Human.hunterDeathProbability = double.Parse(txtDeathProb.Text) / 100;
            }
            catch (Exception ex) {
                MessageBox.Show("Wrong death probability value!");
                Human.hunterDeathProbability = 0.01;
            }
            Human.huntPayoffMultiplier = getNumber(txtPayoffMultiplier, 1, 50, 7);
            Human.defectorPunishment = getNumber(txtPunishment, 1, 600, 300);
            Human.punisherCost = getNumber(txtPunishmentCost, 1, 300, 20);
            try
            {
                Human.strategyMutation = double.Parse(txtStrategyMutation.Text) / 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong strategy mutation rate value!");
                Human.strategyMutation = 0.005;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            step();
        }

        private void showLastActions() {
            txtActions.Text = environment.lastCounts;
        }

        private void step() {
            try
            {
                environment.step();
            }
            catch (Exception ex) { lblStep.Text = "ERROR"; }
            environment.draw();
            chartDrawer.draw();

            txtHumans.Text = environment.count(new Human(environment)).ToString();
            txtMammoths.Text = environment.count(new Mammoth(environment)).ToString();
            txtPlants.Text = environment.count(new Plant(environment)).ToString();
            txtObstacles.Text = environment.count(new Stone(environment)).ToString();

            showLastActions();

            lblStep.Text = environment.steps.ToString();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pnlContainer.Height = this.Height - 43;
            pnlContainer.Width = this.Width - 240;

            if (environment != null)
            {
                environment.init(pnlContainer, WIDTH, HEIGHT);
                environment.draw();
                if (chartDrawer != null) chartDrawer.draw();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int steps = getNumber(txtSteps, 1, 9999, 10);
            for (int i = 0; i < steps - 1; i++)
            {
                environment.step();
                if (chkDraw.Checked) environment.draw();
            }
            step();
        }

        private void pnlContainer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Entity add = null;
            if (cmbAdd.Text == "Mammoth") add = new Mammoth(environment);
            else if (cmbAdd.Text == "Human") add = new Human(environment);
            else if (cmbAdd.Text == "Plant") add = new Plant(environment);
            else if (cmbAdd.Text == "Stone") add = new Stone(environment);

            int x = (int)(e.X / (float)pnlContainer.Width * Environment.envW);
            int y = (int)(e.Y / (float)pnlContainer.Height * Environment.envH);

            if (add != null) add.init();
            if (environment.get(x, y) == null && add != null) environment.add(add, x, y);
            else if (environment.get(x, y) is Human)
            {
                Human h = (Human)environment.get(x, y);
                h.strategy = (Strategies)(((int)h.strategy + 1) % 4);
            }
            
            txtHumans.Text = environment.count(new Human(environment)).ToString();
            txtMammoths.Text = environment.count(new Mammoth(environment)).ToString();
            txtPlants.Text = environment.count(new Plant(environment)).ToString();
            txtObstacles.Text = environment.count(new Stone(environment)).ToString();

            environment.draw();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tn = 0;

            if (!running)
            {
                button4.Text = "Stop";
                running = true;
            }
            else
            {
                button4.Text = "Run";
                running = false;
            }

            if (chkDraw.Checked)
            {
                timer1.Interval = trackBar1.Value * 50 + 10;
                timer1.Enabled = running;
            }
            else {
                timer1.Enabled = false;

                if (running)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    backgroundWorker1.CancelAsync();
                    step();
                }
            }
            showLastActions();
        }

        int tn = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            environment.step();
            environment.draw();
            chartDrawer.draw();
            lblStep.Text = environment.steps.ToString();

            if (tn % 5 == 0)
            {
                txtHumans.Text = environment.count(new Human(environment)).ToString();
                txtMammoths.Text = environment.count(new Mammoth(environment)).ToString();
                txtPlants.Text = environment.count(new Plant(environment)).ToString();
                txtObstacles.Text = environment.count(new Stone(environment)).ToString();
            }
            showLastActions();

            tn++;
            Application.DoEvents();
        }

        private void SetSteps(string steps) { lblStep.Text = steps;  }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (running)
            {
                try
                {
                    environment.step();
                }
                catch (Exception ex) {
                    if (lblStep.InvokeRequired)
                    {
                        this.Invoke(new Action<string>(SetSteps), new object[] { "ERROR" });
                    }
                    else
                    {
                        lblStep.Text = "ERROR";
                    }
                }
                tn++;
                if (tn == 999) MessageBox.Show("999th step");
                if (tn % 50 == 0)
                {
                    if (lblStep.InvokeRequired)
                    {
                        this.Invoke(new Action<string>(SetSteps), new object[] { environment.steps.ToString() });
                    }
                    else
                    {
                        lblStep.Text = environment.steps.ToString();
                    }
                    environment.draw();
                    chartDrawer.draw();
                    if (environment.count(new Human(environment)) == 0) MessageBox.Show("Humans died out :(");
                }
            }
            environment.draw();
            chartDrawer.draw();
        }

        private void pnlContainer_MouseUp(object sender, MouseEventArgs e)
        {
            int x = (int)(e.X / (float)pnlContainer.Width * Environment.envW);
            int y = (int)(e.Y / (float)pnlContainer.Height * Environment.envH);

            if (e.Button == MouseButtons.Right)
            {
                environment.remove(x, y);

                txtHumans.Text = environment.count(new Human(environment)).ToString();
                txtMammoths.Text = environment.count(new Mammoth(environment)).ToString();
                txtPlants.Text = environment.count(new Plant(environment)).ToString();
                txtObstacles.Text = environment.count(new Stone(environment)).ToString();

                environment.draw();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                Entity add;
                if (cmbAdd.Text == "Mammoth") add = new Mammoth(environment);
                else if (cmbAdd.Text == "Human") add = new Human(environment);
                else if (cmbAdd.Text == "Plant") add = new Plant(environment);
                else if (cmbAdd.Text == "Stone") add = new Stone(environment);
                else return;

                Entity ent = environment.getNearest(x, y, add);
                if (ent != null)
                    MessageBox.Show(environment.getDistance(x, y, ent.x, ent.y).ToString());
            }
            else {
                Entity ent = environment.get(x, y);
                if (ent != null)
                {
                    txtActions.Text = ("Info:\r\n" + ent.getInfo() + "\r\n(x, y):(" + x + "," + y + ")");
                }
            }
        }

        private void pnlContainer_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtHumans.Text = "40";
            txtMammoths.Text = "8";
            txtPlants.Text = "30";
            txtObstacles.Text = "5";
        }

        private void BigPanelPaint(object sender, PaintEventArgs e) {
            bigPanel.Width = frminf.ClientRectangle.Width;
            bigPanel.Height = frminf.ClientRectangle.Height;
            bigPanel.Left = 0;
            bigPanel.Top = 0;
            bigChartDrawer.draw();
        }
        private void FormResize(object sender, EventArgs e) {
            bigPanel.Width = frminf.ClientRectangle.Width;
            bigPanel.Height = frminf.ClientRectangle.Height;
            bigPanel.Left = 0;
            bigPanel.Top = 0;
            bigChartDrawer.draw();
        }

        private void pnlGraph_Click(object sender, EventArgs e)
        {
            frminf = new Form();
            frminf.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            frminf.Width = Screen.PrimaryScreen.Bounds.Width;
            frminf.Height = 240;
            bigPanel = new Panel();
            bigPanel.Width = frminf.ClientRectangle.Width;
            bigPanel.Height = frminf.ClientRectangle.Height;
            bigPanel.Left = 0;
            bigPanel.Top = 0;
            bigPanel.Paint += new PaintEventHandler(BigPanelPaint);
            frminf.Controls.Add(bigPanel);
            frminf.StartPosition = FormStartPosition.CenterScreen;
            frminf.Visible = true;
            frminf.Resize += new EventHandler(FormResize);

            bigChartDrawer = new ChartDrawer(bigPanel, environment);

            List<int[]> countData = environment.strategyCountHistory;
            int[] prevalence = new int[4];
            prevalence[0] = prevalence[1] = prevalence[2] = prevalence[3] = 0;
            int i = 0;
            foreach (int[] data in countData) {
                i = 0;
                Utils.getMax(data, ref i);
                prevalence[i]++;
            }
            int max = countData.Count;
            MessageBox.Show("Prevalences:\nCooperators:" + (100.0 / max * prevalence[0]) + "\nDefectors:" + (100.0 / max * prevalence[1]) + "\nPunishers:" + (100.0 / max * prevalence[2]) + "\nLoners:" + (100.0 / max * prevalence[3]));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            timer1.Interval = trackBar1.Value * 50 + 10;
        }

        private void chkDraw_CheckedChanged(object sender, EventArgs e)
        {
            button4_Click(sender, e);
            button4_Click(sender, e);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Human.blockedStrategy = 3;
                txtPlantsReproduce.Text = "140";
            }
            else
            {
                Human.blockedStrategy = -1;
                txtPlantsReproduce.Text = "90";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            environment.exportCounts("counts" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "." + DateTime.Now.Minute + ".txt");
        }
    }
}