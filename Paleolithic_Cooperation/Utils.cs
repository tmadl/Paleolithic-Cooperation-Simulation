using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;

namespace Paleolithic_Cooperation
{
    public static class Utils
    {
        public static int imgW, imgH;
        public static Random rnd = new Random();

        private static List<string> imagenames = new List<string>();
        private static List<Image> images = new List<Image>();

        public static Image getImg() { return getImg("empty"); }

        public static List<string> debugList = new List<string>();

        private static DateTime dt;

        public static void StartClock() { dt = DateTime.Now; }
        public static int EndClock() {
            DateTime d = DateTime.Now;
            return ((d.Minute * 60000 + d.Second * 1000 + d.Millisecond) - (dt.Minute * 60000 + dt.Second * 1000 + dt.Millisecond));
        }

        private static Image img;

        public static void initImgs() {
            DirectoryInfo dir = new DirectoryInfo("img");
            foreach (FileInfo f in dir.GetFiles("*.gif"))
            {
                //long size = f.Length;
                //DateTime creationTime = f.CreationTime;
                Image oimg = Image.FromFile("img/" + f.Name);
                img = oimg.GetThumbnailImage(imgW, imgH, null, IntPtr.Zero);

                imagenames.Add(f.Name.Replace(".gif", ""));
                images.Add(img);
            }
        }

        public static Image getImg(string src)
        {
            /*
            if (!src.Contains("img/"))
                src = "img/" + src;
            if (!src.Contains(".")) src += ".gif";
            if (!System.IO.File.Exists(src))
                return null;
            Image oimg = Image.FromFile(src);
            img = oimg.GetThumbnailImage(imgW, imgH, null, IntPtr.Zero);
            oimg.Dispose();
            return img;*/
            int i = imagenames.FindIndex(delegate(string str) { return str == src; });
            if (i == -1 || i > images.Count) return null;
            else return images[i];
        }

        public static int getMax(int[] data, ref int index) {
            int i = 0;
            int max = -1;
            index = -1;
            for (i = 0; i < data.Length; i++) {
                if (data[i] > max)
                {
                    max = data[i];
                    index = i;
                }
            }
            return max;
        }

        public static void Shuffle<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rnd.Next(n); 
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(ref List<T> list1, ref List<T> list2)
        {
            int n = list1.Count;

            while (n > 1)
            {
                int k = rnd.Next(n); 
                n--;
                T value = list1[k];
                list1[k] = list1[n];
                list1[n] = value;
                value = list2[k];
                list2[k] = list2[n];
                list2[n] = value;
            }
        }
    }
}
