using System;
using System.Collections.Generic;
using System.Text;

namespace Paleolithic_Cooperation.Objects
{
    public class Plant : Entity
    {
        public static int maxPlantVal = 70;
        public static int plantsReproduceAfter = 130;
        public static int plantRegrowAfter = 60;
        public static int maxPlantAge = 100;
        public static int plantInterferenceRadius = 9;

        private int maxAge;
        private int val;
        public int Value {
            get { return val; }
            set { val = value; }
        }

        public Plant(Environment env)
            : base("bush", env)
        {
            init();
        }

        public Plant(int _x, int _y, Environment env) : base("bush", env)
        {
            x = _x;
            y = _y;
            init();
        }

        public override void init() {
            val = (int)(Utils.rnd.Next(10) * maxPlantVal / 10.0) + 10;
            maxAge = Utils.rnd.Next(maxPlantAge);
        }

        public override System.Drawing.Color getColor()
        {
            return System.Drawing.Color.LightGreen;
        }

        public override string getInfo()
        {
            return base.src + " - Value: " + val.ToString();
        }

        public override Entity clone()
        {
            Plant ret = (Plant)this.MemberwiseClone();
            ret.init();
            return ret;
        }

        public override bool Step()
        {
            base.Step();
            if (age > maxPlantAge) parentEnvironment.remove(this);

            if (val == 0)
            {
                val = -plantRegrowAfter;
                this.visible = false;
            }
            else
            {
                if (val < 0 && val >= -2)
                {
                    //regrow
                    init();
                    this.visible = true;
                    age += 50;
                }

                val += 2;

                if (val > plantsReproduceAfter) {
                    Plant p = (Plant)this.clone();
                    p.init();
                    p.age = 0;
                    parentEnvironment.addRandom(p, 1);

                    Plant nearplant = (Plant)parentEnvironment.getNearest(x, y, new Plant(parentEnvironment));
                    double dist;
                    if (nearplant != null && (dist = parentEnvironment.getDistance(this.x, this.y, nearplant.x, nearplant.y)) < plantInterferenceRadius) {
                        //next plant too near
                        if (Utils.rnd.Next(3) > 0)
                        {
                            parentEnvironment.remove(this);
                            return true;
                        }
                    }
                }
            }
            return true;
        }
    }
}
