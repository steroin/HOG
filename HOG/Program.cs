using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOG
{
    class Program
    {
        static void Main(string[] args)
        {
            var image = new Bitmap(@"C:\Users\Sergiusz\Desktop\test.jpg");
            var sw = new Stopwatch();
            sw.Start();
            var hog = new HOG(image, 8, 8, 2, 2);
            double[] d = hog.Describe();
            //foreach (double db in d) Console.Write(db+" ");

            using (var writer = new StreamWriter(@"C: \Users\Sergiusz\Desktop\test1.txt"))
            {
                foreach (double db in d) writer.Write(db + " ");
            }
                Console.WriteLine();
            sw.Stop();
            Console.WriteLine("Vector size: "+d.Length);
            Console.WriteLine("Time elapsed "+sw.Elapsed.TotalMilliseconds+" ms");
            Console.ReadLine();
        }
    }
}
