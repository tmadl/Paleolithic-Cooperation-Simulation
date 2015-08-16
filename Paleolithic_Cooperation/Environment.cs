using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace Paleolithic_Cooperation
{
    public class Environment
    {
        private GridDrawer drawer;

        private Entity[][] mainGrid;

        public static int envW, envH;

        public int steps = 0;

        public string lastActions = "";
        public string lastCounts = "";
        public bool showLastCounts = true;

        int[] objCounts;
        int[] strategyCounts;

        public List<int[]> countHistory;
        public List<int[]> strategyCountHistory;

        public Environment(DPanel target, int w, int h) {
            init(target, w, h);

            initGrid(w, h);
        }

        public void init(DPanel target, int w, int h) {
            envW = w;
            envH = h;
            objCounts = new int[4];
            objCounts[0] = objCounts[1] = objCounts[2] = objCounts[3] = 0;

            strategyCounts = new int[4];
            strategyCounts[0] = strategyCounts[1] = strategyCounts[2] = strategyCounts[3] = 0;

            countHistory = new List<int[]>();
            strategyCountHistory = new List<int[]>();

            drawer = new GridDrawer(target);
        }

        public void initGrid(int w, int h)
        {
            mainGrid = new Entity[w][];
            int i, j;
            for (i = 0; i < w; i++)
            {
                mainGrid[i] = new Entity[h];
                for (j = 0; j < h; j++)
                {
                    mainGrid[i][j] = null;
                }
            }
        }

        public static void normalizeCoords(ref int x, ref int y) {
            normalizeX(ref x);
            normalizeY(ref y);
        }
        public static void normalizeX(ref int x)
        {
            if (x < 0) x += envW;
            if (x >= envW) x -= envW;
        }
        public static void normalizeY(ref int y)
        {
            if (y < 0) y += envH;
            if (y >= envH) y -= envH;
        }

        public void storeCounts() {
            try
            {
                if (showLastCounts) lastCounts += steps + "\t";
                int[] entry = new int[4];
                entry[0] = entry[1] = entry[2] = entry[3] = 0;
                int[] entry2 = new int[4];
                entry2[0] = entry2[1] = entry2[2] = entry2[3] = 0;

                countEntities();

                entry[0] = objCounts[0];
                entry[1] = objCounts[1];
                entry[2] = objCounts[2];
                entry[3] = objCounts[3];
                countHistory.Add(entry);

                entry2[0] = strategyCounts[0];
                entry2[1] = strategyCounts[1];
                entry2[2] = strategyCounts[2];
                entry2[3] = strategyCounts[3];
                strategyCountHistory.Add(entry2);
            }
            catch (Exception ex) { }
            if (showLastCounts) lastCounts += "\n";
        }

        public void exportCounts(string path) {
            StreamWriter sw = new StreamWriter(path, false);

            if (Agents.Human.blockedStrategy == 3) sw.WriteLine("Cooperation is compulsory (no Loners)!");
            else sw.WriteLine("Cooperation is optional (Loners).");
            sw.WriteLine("Steps\n" + countHistory.Count + "\t" + strategyCountHistory.Count + "\n");
            sw.WriteLine("Entity counts\nHuman\tMammoth\tPlant\tStone\t\n----------------------------------------------------");

            int[] sums = new int[4];
            sums[0] = sums[1] = sums[2] = sums[3] = 0;
            int l = countHistory.Count;
            for (int i = 0; i < l; i++)
            {
                sums[0] += countHistory[i][0];
                sums[1] += countHistory[i][1];
                sums[2] += countHistory[i][2];
                sums[3] += countHistory[i][3];
                sw.WriteLine(countHistory[i][0] + "\t" + countHistory[i][1] + "\t" + countHistory[i][2] + "\t" + countHistory[i][3]);
            }
            double humanavg = (double)sums[0] / countHistory.Count;

            sw.WriteLine("\nAverages:");
            sw.WriteLine((double)sums[0] / l + "\t" + (double)sums[1] / l + "\t" + (double)sums[2] / l + "\t" + (double)sums[3] / l);

            sw.WriteLine("\n");
            sw.WriteLine("Strategy counts\nCooper.\tDefect.\tPunish.\tLoner\t\n----------------------------------------------------");

            sums[0] = sums[1] = sums[2] = sums[3] = 0;
            l = strategyCountHistory.Count;
            for (int i = 0; i < l; i++)
            {
                sums[0] += strategyCountHistory[i][0];
                sums[1] += strategyCountHistory[i][1];
                sums[2] += strategyCountHistory[i][2];
                sums[3] += strategyCountHistory[i][3];
                sw.WriteLine(strategyCountHistory[i][0] + "\t" + strategyCountHistory[i][1] + "\t" + strategyCountHistory[i][2] + "\t" + strategyCountHistory[i][3]);
            }

            sw.WriteLine("\nAverages:");
            sw.WriteLine((double)sums[0] / l + "\t" + (double)sums[1] / l + "\t" + (double)sums[2] / l + "\t" + (double)sums[3] / l);

            int[] prevalence = new int[4];
            prevalence[0] = prevalence[1] = prevalence[2] = prevalence[3] = 0;
            int j = 0;
            foreach (int[] data in strategyCountHistory)
            {
                j = 0;
                Utils.getMax(data, ref j);
                prevalence[j]++;
            }
            int max = strategyCountHistory.Count;
            sw.WriteLine("Prevalences:\n" + (100.0 / max * prevalence[0]) + "\t" + (100.0 / max * prevalence[1]) + "\t" + (100.0 / max * prevalence[2]) + "\t" + (100.0 / max * prevalence[3]));

            sw.Close();
        }

        public void countEntities() { 
            int i,j;
            strategyCounts[0] = strategyCounts[1] = strategyCounts[2] = strategyCounts[3] = 0;
            objCounts[0] = objCounts[1] = objCounts[2] = objCounts[3] = 0;
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (get(i, j) is Agents.Human) {
                        strategyCounts[(int)((Agents.Human)get(i, j)).strategy]++;
                    }

                    Entity d = get(i, j);
                    int k = -1;
                    if (d is Agents.Human)
                        k = 0;
                    else if (d is Agents.Mammoth)
                        k = 1;
                    else if (d is Objects.Plant)
                        k = 2;
                    else if (d is Objects.Stone)
                        k = 3;

                    if (k >= 0)
                        objCounts[k]++;
                }
            }
        }

        public bool add(Entity d) {
            return add(d, d.x, d.y);
        }

        private void setObjCounts(Entity d, int addOrSubstract) {
            int i = 0;
            if (d is Agents.Human)
                i = 0;
            else if (d is Agents.Mammoth)
                i = 1;
            else if (d is Objects.Plant)
                i = 2;
            else if (d is Objects.Stone)
                i = 3;

            if (addOrSubstract >= 0 || objCounts[i] > 0)
                objCounts[i] = (objCounts[i] >= 1 ? objCounts[i] + addOrSubstract : 1);

            if (d is Agents.Human)
            {
                strategyCounts[(int)((Agents.Human)d).strategy] += addOrSubstract;
                if (strategyCounts[(int)((Agents.Human)d).strategy] < 0)
                {
                    strategyCounts[(int)((Agents.Human)d).strategy] = 0;
                }
            }
        }

        public bool add(Entity d, int x, int y) {
            normalizeCoords(ref x, ref y);

            lastActions = "add(" + d.getInfo() + ", " + x + ", " + y + ")" + "\r\n" + lastActions;
            d.x = x;
            d.y = y;
            if (mainGrid[x][y] == null) {
                setObjCounts(d, 1);
                d.age = 0;
                mainGrid[x][y] = d;
                return true;
            }
            return false;
        }

        public bool addRandom(Entity d, int number)
        {
            lastActions = "addRandom(" + d.getInfo() + ", " + number + ")" + "\r\n" + lastActions;
            if (number < 0 || number > envW * envH) return false;
            int i, j;
            List<int> emptyX = new List<int>();
            List<int> emptyY = new List<int>();
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] == null)
                    {
                        emptyX.Add(i);
                        emptyY.Add(j);
                    }
                }
            }

            Utils.Shuffle<int>(ref emptyX, ref emptyY);

            for (i = 0; i < emptyX.Count && i < number; i++) {
                Entity o = d.clone();
                o.init();
                o.x = emptyX[i];
                o.y = emptyY[i];
                add(o);
            }

            return true;
        }

        public Entity get(int x, int y) {
            normalizeCoords(ref x, ref y);
            return mainGrid[x][y];
        }

        public void remove(int x, int y) {
            normalizeCoords(ref x, ref y);
            lastActions = "remove(" + x + ", " + y + ")" + "\r\n" + lastActions;            
            if (mainGrid[x][y] != null) {
                setObjCounts(mainGrid[x][y], -1);
            }
            mainGrid[x][y] = null;
        }
        public void remove(Entity ent) {
            remove(ent.x, ent.y);
        }

        public List<Entity> getNeighbors(int centerX, int centerY) {
            List<Entity> results = new List<Entity>();
            Entity e;

            e = get(centerX - 1, centerY);
            if (e != null) results.Add(e);
            e = get(centerX - 1, centerY - 1);
            if (e != null) results.Add(e);
            e = get(centerX, centerY - 1);
            if (e != null) results.Add(e);
            e = get(centerX + 1, centerY - 1);
            if (e != null) results.Add(e);
            e = get(centerX + 1, centerY);
            if (e != null) results.Add(e);
            e = get(centerX + 1, centerY + 1);
            if (e != null) results.Add(e);
            e = get(centerX , centerY + 1);
            if (e != null) results.Add(e);
            e = get(centerX - 1, centerY + 1);
            if (e != null) results.Add(e);

            return results;
        }

        public List<Entity> getNeighborsOfType(int centerX, int centerY, Entity type) {
            List<Entity> neighbors = getNeighbors(centerX, centerY);
            List<Entity> results = new List<Entity>();

            foreach (Entity ent in neighbors) {
                if (ent.GetType() == type.GetType() && ent.visible == true) results.Add(ent);
            }

            return results;
        }

        public List<Entity> getDistantNeighborsOfType(int centerX, int centerY, Entity type)
        {
            List<Entity> results = new List<Entity>();
            List<Entity> temp;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    temp = getNeighborsOfType(centerX + i, centerY + j, type);
                    foreach (Entity e in temp) if (!results.Contains(e)) results.Add(e);
                }
            }

            return results;
        }

        public double getDistance(int x1, int y1, int x2, int y2) {
            int dx = Math.Min(x2 - x1, x1 + envW - x2);
            int dy = Math.Min(y2 - y1, y1 + envH - y2);
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist;
        }

        public Entity getNearest(int centerX, int centerY, Entity type)
        {
            Entity result = null;
            int i, j;

            double smallestDistance = 1e6, distance;
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null)
                    {
                        if (mainGrid[i][j].GetType() == type.GetType() && (i != centerX || j != centerY) && mainGrid[i][j].visible)
                        {
                            double xdist = i - centerX;
                            double ydist = j - centerY;

                            distance = getDistance(centerX, centerY, i, j);

                            if (distance < smallestDistance)
                            {
                                result = mainGrid[i][j];
                                smallestDistance = distance;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public Entity getNearestInLOS(int centerX, int centerY, Entity type)
        {
            Entity result = null;
            int i, j;

            double smallestDistance = 1e6, distance;
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null && mainGrid[i][j].GetType() == type.GetType() && (i != centerX || j != centerY)
                        && mainGrid[i][j].visible)
                    {
                        double xdist = i - centerX;
                        double ydist = j - centerY;

                        distance = getDistance(centerX, centerY, i, j);

                        bool obstructedLOS = isObstructedLos(centerX, centerY, i, j);
                        if (distance < smallestDistance && !obstructedLOS)
                        {
                            result = mainGrid[i][j];
                            smallestDistance = distance;
                        }
                    }
                }
            }
            return result;
        }

        public Entity getNearestInLOS(int centerX, int centerY, Entity type, Entity exclude, int mindist)
        {
            Entity result = null;
            int i, j;

            double smallestDistance = 1e6, distance;
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null && mainGrid[i][j].GetType() == type.GetType() && (i != centerX || j != centerY)
                        && mainGrid[i][j].visible && mainGrid[i][j] != exclude)
                    {
                        double xdist = i - centerX;
                        double ydist = j - centerY;

                        distance = getDistance(centerX, centerY, i, j);

                        bool obstructedLOS = isObstructedLos(centerX, centerY, i, j);
                        if (distance < smallestDistance && distance > mindist && !obstructedLOS)
                        {
                            result = mainGrid[i][j];
                            smallestDistance = distance;
                        }
                    }
                }
            }
            return result;
        }

        public bool isObstructedLos(int x1, int y1, int x2, int y2) {
            double distance = getDistance(x1, y1, x2, y2);
            bool obstructedLOS = false;
            int steps = (int)Math.Round(distance);
            if (steps == 0) return false;
            double dx = (double)(x2 - x1) / steps;
            double dy = (double)(y2 - y1) / steps;
            double cx = x1, cy = y1;
            Entity ent;
            for (int k = 0; k < steps; k++)
            {
                cx += dx;
                cy += dy;
                if (((int)(cx) != x1 || (int)(cy) != y1) && ((int)cx != x2 || (int)cy != y2) &&
                    (ent = get((int)(cx), (int)(cy))) != null)
                { //entity in line of sight
                    if (ent is Objects.Stone) //stone in line of sight
                        obstructedLOS = true;

                }
            }
            return obstructedLOS;
        }

        public List<Entity> getAllInLOS(int centerX, int centerY, Entity type)
        {
            List<Entity> result = new List<Entity>();
            int i, j;

            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null)
                    {
                        try
                        {
                            if (mainGrid[i][j].GetType() == type.GetType() && (i != centerX || j != centerY)
                                && mainGrid[i][j].visible)
                            {
                                bool obstructedLOS = isObstructedLos(centerX, centerY, i, j);
                                if (!obstructedLOS)
                                {
                                    result.Add(mainGrid[i][j]);
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                }
            }
            return result;
        }

        public bool move(Entity ent, int newX, int newY) {
            return move(ent, newX, newY, false);
        }

        public bool move(Entity ent, int newX, int newY, bool force) {
            normalizeCoords(ref newX, ref newY);
            lastActions = "move(" + ent.getInfo() + ", " + newX + ", " + newY + ")" + "\r\n" + lastActions;
            if (get(newX, newY) != null)
            {
                if (force && get(newX, newY) is Agents.Human)
                {
                    mainGrid[ent.x][ent.y] = get(newX, newY);
                    mainGrid[ent.x][ent.y].x = ent.x;
                    mainGrid[ent.x][ent.y].y = ent.y;
                }
                else return false;
            }
            else {
                mainGrid[ent.x][ent.y] = null;
            }
            ent.x = newX;
            ent.y = newY;
            mainGrid[newX][newY] = ent;
            return true;
        }

        public void getBestDirection(Entity e, Entity target, ref int dx, ref int dy, ref double dist)
        {
            if (e == null || target == null) return;
            dx = 0;
            dy = 0;
            double cdist, mindist = dist;
            if (mindist == 0) mindist = 1e9;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (get(e.x + i, e.y + j) == null)
                    {
                        cdist = getDistance(e.x + i, e.y + j, target.x, target.y);
                        if (cdist <= mindist)
                        {
                            mindist = cdist;
                            dx = i;
                            dy = j;
                        }
                    }
                }
            }
            dist = mindist;
        }            

        public int count(Entity type) { 
            int i = 0;
            if (type is Agents.Human)
                i = 0;
            else if (type is Agents.Mammoth)
                i = 1;
            else if (type is Objects.Plant)
                i = 2;
            else if (type is Objects.Stone)
                i = 3;

            return objCounts[i];
        }

        public void draw() {
            int i, j;
            drawer.clear();
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null)
                    {
                        drawer.draw(mainGrid[i][j]);
                    }
                }
            }
            drawer.drawGrid();
            drawer.render();
        }

        public void step() {
            Utils.StartClock();

            int i, j;
            List<Entity> entities = new List<Entity>();

            steps++;

            List<Agents.Human> humans = new List<Agents.Human>();
            for (i = 0; i < envW; i++)
            {
                for (j = 0; j < envH; j++)
                {
                    if (mainGrid[i][j] != null) { 
                        entities.Add(mainGrid[i][j]);
                        if (mainGrid[i][j] is Agents.Human) humans.Add((Agents.Human)mainGrid[i][j]);
                    }
                }
            }
            foreach (Entity ent in entities)
            {
                if (ent is Agents.Mammoth)
                    ((Agents.Mammoth)ent).Step(((steps % 2 == 0) ? true : false));
                else
                    ent.Step();
            }

            if (humans != null && humans.Count > 0)
                humans[Utils.rnd.Next(humans.Count)].tryStrategyChange();
                
            if (count(new Objects.Plant(this)) < 6) addRandom(new Objects.Plant(this), 1 + Utils.rnd.Next(3));

            storeCounts();

            Utils.debugList.Add(Utils.EndClock().ToString());
        }
    }
}
