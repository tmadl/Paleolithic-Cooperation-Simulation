using System;
using System.Collections.Generic;
using System.Text;

namespace Paleolithic_Cooperation
{
    public abstract class Entity
    {
        public string src;
        public int x, y;
        public int age = 0;
        public bool visible = true;

        protected Environment parentEnvironment;

        private System.Drawing.Image img;
        public System.Drawing.Image Image {
            get {
                if (visible)
                    return img;
                else return Utils.getImg();
            }
        }

        public Entity(string imgsrc, Environment env) {
            parentEnvironment = env;
            setImg(imgsrc);
        }

        public void setImg(string imgsrc) {
            src = imgsrc;
            img = Utils.getImg(src);
        }

        public virtual string getInfo() {
            return "";
        }

        public virtual System.Drawing.Color getColor() {
            return System.Drawing.Color.White;
        }

        public virtual Entity clone() {
            return (Entity)this.MemberwiseClone();
        }

        public abstract void init();

        public virtual bool Step() {
            age++;
            return true;
        }

        public virtual bool isAlive() { return true; }
    }
}
