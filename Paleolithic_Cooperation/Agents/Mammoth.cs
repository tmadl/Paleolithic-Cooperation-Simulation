using System;
using System.Collections.Generic;
using System.Text;

using Paleolithic_Cooperation.Objects;

namespace Paleolithic_Cooperation.Agents
{
    class Mammoth : Entity
    {
        public static int reproductionThreshold = 110;
        public static int maxMammothAge = 190;
        public static int foodPerStep = 10;
        public static int initialPayoff = 70;

        public static int ignoreDangerHungerThreshold = 50;

        public static int dangerRadius = 5;

        private Entity prevGoal;
        private double prevDistance;

        private int payoff;
        private int maxAge;

        private bool killed = false;

        public Mammoth(Environment env)
            : base("mammoth", env)
        {
            init();
        }

        public Mammoth(int _x, int _y, Environment env)
            : base("mammoth", env)
        {
            x = _x;
            y = _y;

            init();
        }

        public override void init() {
            payoff = initialPayoff;
            prevDistance = 1e9;
            prevGoal = null;
            maxAge = Utils.rnd.Next(Mammoth.maxMammothAge / 2) + Mammoth.maxMammothAge / 2;
        }

        public override Entity clone()
        {
            return (Mammoth)this.MemberwiseClone();
        }

        public override string getInfo()
        {
            return base.src + " - Payoff: " + payoff.ToString();
        }

        public override System.Drawing.Color getColor()
        {
            return (age != maxAge ? System.Drawing.Color.Red : System.Drawing.Color.Magenta);
        }

        public int Kill() {
            age = maxAge;
            killed = true;
            return payoff;
        }

        public override bool isAlive() { return (payoff > 0 && age <= maxAge); }

        public override bool Step() {
            return Step(true);
        }

        public bool isDanger(ref int dx, ref int dy) {
            int i, j;
            int minx = x - dangerRadius;
            int miny = y - dangerRadius;
            int maxx = x + dangerRadius;
            int maxy = y + dangerRadius;

            Environment.normalizeCoords(ref minx, ref miny);
            Environment.normalizeCoords(ref maxx, ref maxy);

            for (i = minx; i < maxx; i++)
            {
                for (j = miny; j < maxy; j++)
                {
                    if (parentEnvironment.get(i, j) is Human) {
                        List<Entity> nearhumans = parentEnvironment.getNeighborsOfType(i, j, new Human(parentEnvironment));
                        if (nearhumans.Count >= Human.humansPerMammoth - 1) {
                            dx = i > x ? -1 : 1;
                            dy = j > y ? -1 : 1;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Step(bool canMove)
        {
            try
            {
                base.Step();
                payoff--;

                if (age > maxAge && payoff > 0)
                {
                    payoff = 0; //die
                }

                if (payoff <= 0)
                {
                    if (parentEnvironment.count(new Mammoth(parentEnvironment)) > 2 )
                    {
                        if (payoff >= -2)
                        {
                            base.setImg("mammoth_dead");
                            payoff--;
                        }
                        else
                        {
                            parentEnvironment.remove(this);
                        }
                    }
                    else
                    {  //save mammoths from dying out ?
                        init();
                        parentEnvironment.addRandom(this.clone(), 2);
                        init();
                    }
                }
                else if (payoff > 0)
                {
                    if (payoff > ignoreDangerHungerThreshold) { 
                        //enough food to flee - check for danger
                        int dx = 0, dy = 0;
                        bool danger = isDanger(ref dx, ref dy);
                        if (danger && canMove) {
                            if (parentEnvironment.move(this, x + dx, y + dy)) return true;
                        }
                    }

                    List<Entity> nearplants = parentEnvironment.getNeighborsOfType(x, y, new Plant(parentEnvironment));
                    base.setImg("mammoth");
                    if (nearplants.Count > 0)
                    {
                        bool hasEaten = false;
                        foreach (Plant cPlant in nearplants)
                        {
                            if (cPlant.Value > 0)
                            {
                                cPlant.Value -= foodPerStep;
                                hasEaten = true;
                                if (cPlant.Value < 0)
                                {
                                    cPlant.Value = 0;
                                }
                                break;
                            }
                            else
                            {

                            }
                        }
                        if (hasEaten)
                        {
                            base.setImg("mammoth_eating");
                            payoff += foodPerStep; //increase payoff by 10

                            prevGoal = null;
                            prevDistance = 1e9;
                        }
                    }
                    else
                    {
                        DateTime dt = DateTime.Now;
                        Plant nearplant = (Plant)parentEnvironment.getNearestInLOS(x, y, new Plant(parentEnvironment));
                        int ms = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - dt.Second * 1000 - dt.Millisecond;
                        bool success = false;
                        if (prevGoal != null)
                        {
                            if (parentEnvironment.get(prevGoal.x, prevGoal.y) == null
                            || !(parentEnvironment.get(prevGoal.x, prevGoal.y) is Plant)
                            || prevGoal.visible == false)
                            {
                                prevGoal = null;
                                prevDistance = 1e9;
                            }
                        }
                        if (nearplant != null || prevGoal != null)
                        {
                            if (prevGoal == null || nearplant != null && parentEnvironment.getDistance(x, y, nearplant.x, nearplant.y) < prevDistance - 1)
                            {
                                prevGoal = nearplant;
                                prevDistance = parentEnvironment.getDistance(x, y, nearplant.x, nearplant.y);
                            }

                            //get direction with greatest distance gain to plant
                            int mindx = 0, mindy = 0;
                            double mindist = 1e9;
                            parentEnvironment.getBestDirection(this, prevGoal, ref mindx, ref mindy, ref mindist);

                            if (mindist < prevDistance && canMove) success = parentEnvironment.move(this, x + mindx, y + mindy);
                        }
                    }

                    if (payoff > reproductionThreshold)
                    {
                        //reproduce and reset payoff
                        reproduce();
                    }
                }
            }
            catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            return true;
        }

        private void reproduce() {
            init();
            bool reproduced = false;
            for (int i = -1; i <= 1 && !reproduced; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (parentEnvironment.get(x + i, y + j) == null)
                    {
                        parentEnvironment.add(this.clone(), x + i, y + j);
                        reproduced = true;
                        break;
                    }
                }
            }
            init();
        }
    }
}
