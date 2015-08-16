using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Paleolithic_Cooperation.Objects;

namespace Paleolithic_Cooperation.Agents
{
    public enum Strategies { Cooperator = 0, Defector = 1, Punisher = 2, Loner = 3 };

    class Human : Entity
    {
        public static int blockedStrategy = -1;

        public static int reproductionThreshold = 300; 
        public static int reproductionCost = 200;
        
        public static int maxHumanAge = 400; 
        public static int foodPerStep = 3;
        public static int minInitialPayoff = 80; 

        public static int humansPerMammoth = 4;
        public static double hunterDeathProbability = 0.01; //percent
        public static int hunterMaxDamage = 50;
        public static double huntPayoffMultiplier = 7; 

        public static int defectorPunishment = 280;
        public static int punisherCost = 20;

        public static int hunterSpeed = 1;
        public static int punisherSpeed = 2;

        public static double strategyMutation = 0.005;

        public Strategies strategy;

        public bool signaling = false;
        public Mammoth targetMammoth;

        private Entity prevGoal;
        private double prevDistance;

        private double payoff;
        private int maxAge;
        private int diedAtStep;
        private Hashtable observedPayoffs;

        private List<Human> punisherGoals = null;
        private Human prevPunisherGoal = null;

        private Mammoth prevHunterGoal = null;
        private double prevHunterDistance = 1e6;
        private int prevMHunters = -1;

        public Human(Environment env) : base("caveman", env) {
        }

        public Human(int _x, int _y, Environment env)
            : base("caveman", env)
        {
            x = _x;
            y = _y;
        }

        public override void init()
        {
            strategy = (Strategies)Utils.rnd.Next(4);
            while (strategy == (Strategies)blockedStrategy) strategy = (Strategies)Utils.rnd.Next(4);

            payoff = minInitialPayoff + Utils.rnd.Next(minInitialPayoff);
            prevDistance = 1e9;
            prevGoal = null;
            maxAge = maxHumanAge / 2 + Utils.rnd.Next(maxHumanAge / 2);

            punisherGoals = new List<Human>();
            prevPunisherGoal = null;

            prevHunterGoal = null;
            prevHunterDistance = 1e6;
            prevMHunters = -1;

            diedAtStep = -1;

            observedPayoffs = new Hashtable();
        }

        public override Entity clone()
        {
            return (Human)this.MemberwiseClone();
        }

        public override string getInfo()
        {
            return base.src + "\n" + strategy.ToString() + " - Payoff: " + payoff.ToString();
        }

        public override System.Drawing.Color getColor()
        {
            switch (strategy) {
                case Strategies.Cooperator:
                    return System.Drawing.Color.LightBlue;
                case Strategies.Defector:
                    return System.Drawing.Color.Yellow;
                case Strategies.Punisher:
                    return System.Drawing.Color.SteelBlue;
                default:
                    return System.Drawing.Color.Chocolate;
            }
        }

        public override bool isAlive() { return (payoff > 0 && age <= maxAge); }

        static int[] times = new int[2];
        public override bool Step()
        {
            base.Step();
            payoff -= 1; 

            bool d = true;
            if (age > maxAge && payoff > 0)
            {
                payoff = 0; //die
                diedAtStep = parentEnvironment.steps;
                if (!base.src.Contains("caveman_dead"))
                    parentEnvironment.lastCounts = ("Died from old age " + this.strategy.ToString() + "\r\n") + parentEnvironment.lastCounts;
                d = false;
            }

            if (payoff <= 0)
            {
                if (parentEnvironment.steps - diedAtStep <= 2)
                {
                    base.setImg("caveman_dead");
                    payoff--;
                }
                else
                {
                    if (d) 
                        parentEnvironment.lastCounts = ("Died from malnourishment " + this.strategy.ToString() + "\r\n") + parentEnvironment.lastCounts;
                    parentEnvironment.remove(this);
                }
                return true;
            }
            else if (diedAtStep > 0) {
                parentEnvironment.remove(this);
                return true;
            }
            else
            {
                bool success;
                switch (strategy)
                {
                    case Strategies.Cooperator:
                        success = hunterStep();
                        break;
                    case Strategies.Defector:
                        success = hunterStep();
                        break;
                    case Strategies.Punisher:
                        success = punisherStep();
                        break;
                    default:
                        success = lonerStep();
                        break;
                }

                if (payoff > reproductionThreshold)
                {
                    //reproduce and reset payoff
                    Strategies strat = strategy;
                    payoff -= reproductionCost;
                    Human child = new Human(parentEnvironment);
                    child.init();
                    child.strategy = strat;
                    bool reproduced = false;
                    for (int i = -1; i <= 1 && !reproduced; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (parentEnvironment.get(x + i, y + j) == null)
                            {
                                success &= parentEnvironment.add(child, x + i, y + j);
                                reproduced = true;
                                break;
                            }
                        }
                    }
                }
                
                //update average observed payoffs of all humans
                List<Entity> humansInSight = parentEnvironment.getAllInLOS(x, y, this);
                if (humansInSight.Count > 0)
                    foreach (Human h in humansInSight)
                    {
                        if (h == null) continue;
                        int[] entry;
                        if (!observedPayoffs.ContainsKey(h))
                        {
                            entry = new int[4];
                            entry[0] = (int)h.payoff;
                            entry[1] = 1;
                            entry[2] = (int)h.strategy;
                            entry[3] = h.signaling ? 1 : 0;
                            observedPayoffs[h] = entry;
                        }
                        else
                        {
                            entry = (int[])observedPayoffs[h];
                            entry[3] = h.signaling ? 1 : 0;
                            if (entry[2] != (int)h.strategy)
                            { //human changed strategy
                                entry[2] = (int)h.strategy;
                                entry[1] = 1;
                                entry[0] = (int)h.payoff;
                            }
                            else
                            {
                                int n = entry[1];
                                int sum = entry[0];
                                entry[1] = n + 1;
                                entry[0] = (int)(sum * n + h.payoff) / (n + 1);
                            }
                            observedPayoffs[h] = entry;
                        }
                    }
                
                if (Utils.rnd.Next((int)(1.0 / strategyMutation)) == 0) {
                    Strategies strat = (Strategies)Utils.rnd.Next(4);
                    while (strat == strategy || strat == (Strategies)blockedStrategy) strat = (Strategies)Utils.rnd.Next(4);
                    strategy = strat;
                }

                return success;
            }
        }

        static string probs = "";
        public void tryStrategyChange() {
            if (observedPayoffs == null)  return;
            try
            {
                int target = Utils.rnd.Next(observedPayoffs.Values.Count);
                int i = 0;
                object[] keys = new object[observedPayoffs.Count];
                observedPayoffs.Keys.CopyTo(keys, 0);
                foreach (int[] entry in observedPayoffs.Values)
                {
                    if (i == target)
                    {
                        if (keys[i] is Human && ((Human)keys[i]).isAlive() && parentEnvironment.get(((Human)keys[i]).x, ((Human)keys[i]).y) is Human)
                        {
                            if (entry[0] > payoff)
                            {
                                int diff = entry[0] - (int)payoff;
                                if (diff > reproductionThreshold) diff = reproductionThreshold;

                                double probability = 100.0 * diff / reproductionThreshold;

                                probs += ((int)(probability * 10) / 10.0).ToString() + " ";

                                if (Utils.rnd.Next(80) < (int)probability)
                                {
                                    parentEnvironment.lastCounts = ("Strategy change " + this.strategy.ToString() + "-> " + ((Strategies)entry[2]).ToString() + "\r\n") + parentEnvironment.lastCounts;
                                    strategy = (Strategies)entry[2];
                                }
                            }
                        }
                        else {
                            observedPayoffs.Remove(keys[i]);
                            tryStrategyChange();
                        }
                        break;
                    }
                    i++;
                }
            }
            catch (Exception ex) { }
        }

        private bool hunterStep() {
            bool success = true;
            bool hunted = false;
            base.setImg("caveman");
            List<Entity> nearmammoths = parentEnvironment.getNeighborsOfType(x, y, new Mammoth(parentEnvironment));
            if (nearmammoths.Count > 0 && (nearmammoths.Contains(prevHunterGoal) || prevHunterGoal == null))
            {
                foreach (Mammoth m in nearmammoths) {
                    if (m.isAlive() && (m == prevHunterGoal || prevHunterGoal == null))
                    {
                        List<Entity> nearhumans = parentEnvironment.getDistantNeighborsOfType(m.x, m.y, this);
                        int hunters = 0;
                        int defectors = 0;
                        foreach (Human h in nearhumans)
                        {
                            if (h.strategy != Strategies.Loner) hunters++;
                            if (h.strategy == Strategies.Defector) defectors++;
                        }
                        if (hunters >= humansPerMammoth)
                        {
                            hunted = true;
                            //try to kill mammoth
                            int mpayoff = 0;
                            if (hunters - defectors >= humansPerMammoth)
                            {
                                mpayoff = m.Kill();
                            }
                            foreach (Human h in nearhumans)
                            {
                                if (h.strategy != Strategies.Loner)
                                {
                                    if (mpayoff == 0 && h.src.Contains("hunting")) { 
                                        //previous hunt of this mammoth failed
                                        prevHunterGoal = (Mammoth)parentEnvironment.getNearestInLOS(x, y, new Mammoth(parentEnvironment), prevHunterGoal, 2);
                                        if (prevHunterGoal != null) prevHunterDistance = parentEnvironment.getDistance(x, y, prevHunterGoal.x, prevHunterGoal.y);
                                        h.setImg("caveman_punished");
                                    }
                                    h.payoff += (int)(huntPayoffMultiplier * mpayoff / nearhumans.Count);
                                    h.setImg("caveman_hunting");
                                    if (h.strategy != Strategies.Defector)
                                    {
                                        h.payoff -= Utils.rnd.Next(hunterMaxDamage);
                                        if (h.payoff < 0) 
                                            h.payoff = 10; //no death, only injury
                                        double p = h.payoff;
                                        if (p > reproductionThreshold) p = reproductionThreshold;
                                        double deathProbability = hunterDeathProbability - (p / reproductionThreshold) * hunterDeathProbability;
                                        if (Utils.rnd.Next(100) < deathProbability * 100)
                                        {
                                            h.setImg("caveman_dead");
                                            h.age = maxAge;

                                            parentEnvironment.lastCounts = ("Died from hunting " + h.strategy.ToString() + "\r\n") + parentEnvironment.lastCounts;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            if (!hunted)
            {
                int hunters = 0;
                Mammoth mammoth = getTargetMammoth(ref hunters);
                

                if (prevHunterGoal != null)
                {
                    try
                    {
                        if (parentEnvironment.get(prevHunterGoal.x, prevHunterGoal.y) == null
                        || !(parentEnvironment.get(prevHunterGoal.x, prevHunterGoal.y) is Mammoth)
                        || prevHunterGoal.visible == false || !prevHunterGoal.isAlive())
                        {
                            prevHunterGoal = null;
                            prevHunterDistance = 1e9;
                        }
                    }
                    catch (Exception ex) { }
                }

                if (mammoth != null || prevHunterGoal != null)
                {
                    if (prevHunterGoal == null || mammoth != null && (hunters == -1 || hunters > prevMHunters))
                    {
                        prevHunterGoal = mammoth;
                        prevHunterDistance = parentEnvironment.getDistance(x, y, mammoth.x, mammoth.y);
                        prevMHunters = hunters;
                    }

                    int mindx = 0, mindy = 0;
                    double mindist = 1e9;
                    parentEnvironment.getBestDirection(this, prevHunterGoal, ref mindx, ref mindy, ref mindist);
                    if (mindist <= 2 || parentEnvironment.get(x + mindx * hunterSpeed, y + mindy * hunterSpeed) != null)
                        success = parentEnvironment.move(this, x + mindx, y + mindy);
                    else
                        success = parentEnvironment.move(this, x + mindx * hunterSpeed, y + mindy * hunterSpeed);
                }
            }

            return success;
        }

        private bool defectorStep() { return true; }

        private bool punisherStep() {
            int i, j;
            if (punisherGoals.Count > 0) 
                prevPunisherGoal = punisherGoals[0];

            for (i = 0; i < Environment.envW; i++) {
                for (j = 0; j < Environment.envH; j++) {
                    if (parentEnvironment.get(i, j) is Human && parentEnvironment.get(i, j).src.Contains("hunting")) {
                        if (prevPunisherGoal != parentEnvironment.get(i, j) && !parentEnvironment.isObstructedLos(x, y, i, j) &&
                            ((Human)parentEnvironment.get(i, j)).strategy == Strategies.Defector) {
                            //prevPunisherGoal = punisherGoals;
                                if (!punisherGoals.Contains((Human)parentEnvironment.get(i, j))) 
                                    punisherGoals.Add((Human)parentEnvironment.get(i, j));
                        }
                    }
                }
            }

            if (punisherGoals == null || punisherGoals.Count == 0 || payoff < minInitialPayoff)
            {
                if (base.src.Contains("punishing")) base.setImg("caveman");
                return hunterStep();
            }
            else
            {
                if (parentEnvironment.getDistance(x, y, punisherGoals[0].x, punisherGoals[0].y) > 2)
                {
                    int dx = 0, dy = 0;
                    double dist = 1e9;
                    parentEnvironment.getBestDirection(this, punisherGoals[0], ref dx, ref dy, ref dist);
                    //return parentEnvironment.move(this, x + dx, y + dy);

                    bool success = true;
                    if (dist > 2)
                    {
                        if (parentEnvironment.get(x + dx, y + dy) == null || parentEnvironment.get(x + dx, y + dy) is Human)
                            success = parentEnvironment.move(this, x + dx * punisherSpeed, y + dy * punisherSpeed, true);
                        else
                            success = parentEnvironment.move(this, x + dx * punisherSpeed, y + dy * punisherSpeed);
                    }
                    if (dist <= 2 || !success)
                    {
                        if (parentEnvironment.get(x + dx, y + dy) == null || parentEnvironment.get(x + dx, y + dy) is Human)
                            return parentEnvironment.move(this, x + dx, y + dy, true);
                        else
                            return parentEnvironment.move(this, x + dx, y + dy);
                    }                    
                }
                else if (punisherGoals != null && punisherGoals.Count > 0) {
                    base.setImg("caveman_punishing");
                    parentEnvironment.lastCounts = ("Defector punished " + punisherGoals[0].payoff + "->" + (punisherGoals[0].payoff - defectorPunishment) + "\r\n") + parentEnvironment.lastCounts;
                    punisherGoals[0].setImg("caveman_punished");
                    punisherGoals[0].payoff -= defectorPunishment;
                    punisherGoals.RemoveAt(0);

                    payoff -= punisherCost;
                }
            }
            return true; 
        }

        private bool lonerStep() {
            if (payoff > 0)
            {
                List<Entity> nearplants = parentEnvironment.getNeighborsOfType(x, y, new Plant(parentEnvironment));
                base.setImg("caveman");
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
                        base.setImg("caveman_eating");
                        payoff += foodPerStep; //increase payoff by 10

                        prevGoal = null;
                        prevDistance = 1e9;
                    }
                }
                else
                {
                    DateTime dt = DateTime.Now;
                    Plant nearplant = getNearestMammothFreePlant();
                    if (nearplant == null) nearplant = (Plant)parentEnvironment.getNearestInLOS(x, y, new Plant(parentEnvironment));
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
                        if (prevGoal == null || nearplant != null && parentEnvironment.getDistance(x, y, nearplant.x, nearplant.y) < prevDistance - 2)
                        {
                            prevGoal = nearplant;
                            prevDistance = parentEnvironment.getDistance(x, y, nearplant.x, nearplant.y);
                        }

                        //get direction with greatest distance gain to plant
                        int mindx = 0, mindy = 0;
                        double mindist = prevDistance;

                        parentEnvironment.getBestDirection(this, prevGoal, ref mindx, ref mindy, ref mindist);

                        if (mindist < prevDistance) success = parentEnvironment.move(this, x + mindx, y + mindy);
                    }
                }
            }
            return true;
        }

        public Mammoth getTargetMammoth(ref int prevHunters)
        {
            Entity result = null;
            int i, j;
            int centerX = x, centerY = y;

            //look for signalling hunter
            double distance;
            double mindist = 1e6;
            for (i = 0; i < Environment.envW; i++)
            {
                for (j = 0; j < Environment.envH; j++)
                {
                    if (parentEnvironment.get(i, j) != null && parentEnvironment.get(i, j) is Human && (i != centerX || j != centerY)
                        && parentEnvironment.get(i, j).visible)
                    {
                        double xdist = i - centerX;
                        double ydist = j - centerY;

                        distance = parentEnvironment.getDistance(centerX, centerY, i, j);

                        bool obstructedLOS = parentEnvironment.isObstructedLos(centerX, centerY, i, j);
                        if (!obstructedLOS)
                        {
                            Human h = (Human)parentEnvironment.get(i, j);
                            if (h != null && h.signaling)
                            {
                                if (h.targetMammoth != null && h.targetMammoth.isAlive())
                                {
                                    if (distance < mindist)
                                    {
                                        result = h.targetMammoth;
                                        mindist = distance;
                                    }
                                }
                                else
                                {
                                    h.signaling = false;
                                    h.setImg("caveman");
                                }
                            }
                        }
                    }
                }
            }

            if (result != null) { //signaling human targeting mammoth found
                prevHunters = -1;
                return (Mammoth)result;
            }

            //if no signalling hunter in LOS, get nearest mammoth with most hunters
            prevHunters = -1;
            int hunters;
            double smallestDistance = 1e6;
            for (i = 0; i < Environment.envW; i++)
            {
                for (j = 0; j < Environment.envH; j++)
                {
                    if (parentEnvironment.get(i, j) != null && parentEnvironment.get(i, j) is Mammoth && (i != centerX || j != centerY)
                        && parentEnvironment.get(i, j).visible)
                    {
                        double xdist = i - centerX;
                        double ydist = j - centerY;

                        distance = parentEnvironment.getDistance(centerX, centerY, i, j);

                        bool obstructedLOS = parentEnvironment.isObstructedLos(centerX, centerY, i, j);
                        if (!obstructedLOS)
                        {
                            List<Entity> humans = parentEnvironment.getNeighborsOfType(i, j, this);
                            hunters = 0;
                            foreach (Human h in humans) if (h.strategy != Strategies.Loner) hunters++;
                            if (hunters > prevHunters) {
                                result = parentEnvironment.get(i, j);
                                smallestDistance = distance;
                                prevHunters = hunters;
                            }
                            else if (distance < smallestDistance)
                            {
                                result = parentEnvironment.get(i, j);
                                smallestDistance = distance;
                            }
                        }
                    }
                }
            }


            bool signalingFound = false;
            try
            {
                foreach (int[] entry in observedPayoffs.Values)
                {
                    if (entry[3] == 1) signalingFound = true;
                }
            }
            catch (Exception ex) { }

            if (result != null && !signalingFound && (strategy == Strategies.Cooperator || strategy == Strategies.Punisher))
            {
                targetMammoth = (Mammoth)result;
                signaling = true;
                base.setImg("caveman_signaling");
            }
            else {
                signaling = false;
                base.setImg("caveman");
            }

            return (Mammoth)result;
        }

        public Plant getNearestMammothFreePlant()
        {
            Entity result = null;
            int i, j;
            int centerX = x, centerY = y;

            double smallestDistance = 1e6, distance;
            for (i = 0; i < Environment.envW; i++)
            {
                for (j = 0; j < Environment.envH; j++)
                {
                    if (parentEnvironment.get(i, j) != null && parentEnvironment.get(i, j) is Plant && parentEnvironment.get(i, j).visible)
                    {
                        double xdist = i - centerX;
                        double ydist = j - centerY;

                        distance = parentEnvironment.getDistance(centerX, centerY, i, j);

                        bool obstructedLOS = false;
                        int steps = (int)Math.Round(distance);
                        double dx = xdist / steps;
                        double dy = ydist / steps;
                        double cx = centerX, cy = centerY;
                        Entity ent;
                        for (int k = 0; k < steps; k++)
                        {
                            cx += dx;
                            cy += dy;
                            if ((int)(cx) != centerX && (int)(cy) != centerY && (ent = parentEnvironment.get((int)(cx), (int)(cy))) != null)
                            { //entity in line of sight
                                if (ent is Objects.Stone) //stone in line of sight
                                    obstructedLOS = true;

                            }
                        }
                        if (distance < smallestDistance && !obstructedLOS && parentEnvironment.getNeighborsOfType(i, j, new Mammoth(parentEnvironment)).Count == 0)
                        {
                            result = parentEnvironment.get(i,j);
                            smallestDistance = distance;
                        }
                    }
                }
            }
            return (Plant)result;
        }
    }
}
