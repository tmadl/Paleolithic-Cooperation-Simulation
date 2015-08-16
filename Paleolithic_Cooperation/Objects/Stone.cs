using System;
using System.Collections.Generic;
using System.Text;

namespace Paleolithic_Cooperation.Objects
{
    public class Stone : Entity
    {
        public Stone(Environment env) : base("stone", env) {
        }

        public Stone(int _x, int _y, Environment env) : base("stone", env) {
            x = _x;
            y = _y;
        }

        public override Entity clone()
        {
            return (Stone)this.MemberwiseClone();
        }

        public override void init()
        {
        }

        public override string getInfo()
        {
            return base.src;
        }

        public override bool Step() {
            base.Step();
            return true;
        }
    }
}
