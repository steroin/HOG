using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOG
{
    class HOG
    {
        public Bitmap Image { get; set; }
        int _CellWidth;
        public int CellWidth
        {
            get
            {
                return _CellWidth;
            }
            set
            {
                if (value <= 0) throw new InappropriateDimensionException(string.Format("Wrong CellWidth attribute: {0}", value));
                else _CellWidth = value;
            }
        }

        int _CellHeight;
        public int CellHeight
        {
            get
            {
                return _CellHeight;
            }
            set
            {
                if (value <= 0) throw new InappropriateDimensionException(string.Format("Wrong CellHeight attribute: {0}", value));
                else _CellHeight = value;
            }
        }
        public int _BlockWidth;
        public int BlockWidth
        {
            get
            {
                return _BlockWidth;
            }
            set
            {
                if (value <= 0) throw new InappropriateDimensionException(string.Format("Wrong BlockWidth attribute: {0}", value));
                else _BlockWidth = value;
            }
        }
        public int _BlockHeight;
        public int BlockHeight
        {
            get
            {
                return _BlockHeight;
            }
            set
            {
                if (value <= 0) throw new InappropriateDimensionException(string.Format("Wrong BlockHeight attribute: {0}", value));
                else _BlockHeight = value;
            }
        }

        public bool SignedOrientations { get; set; }

        public HOG(Bitmap img, int cellWidth, int cellHeight, int blockWidth, int blockHeight, bool signedOrientations=false)
        {
            Image = img;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            SignedOrientations = signedOrientations;
        }

        public double[] Describe()
        {
            double[] ret = null;
            int imgWidth = Image.Width;
            int imgHeight = Image.Height;
            int[,] magnitudes = new int[imgHeight, imgWidth];
            int[,] directions = new int[imgHeight, imgWidth];

            Console.WriteLine("Directions: ");
            for (int i = 0; i < imgHeight; i++)
            {
                for (int j = 0; j < imgWidth; j++)
                {
                
                    var pixelLeft = j - 1 < 0 ? Color.Black : Image.GetPixel(j-1, i);
                    var pixelUp = i + 1 >= imgHeight ? Color.Black : Image.GetPixel(j, i+1);
                    var pixelRight = j + 1 >= imgWidth ? Color.Black : Image.GetPixel(j+1, i);
                    var pixelDown = i - 1 < 0 ? Color.Black : Image.GetPixel(j, i-1);

                    int maxGradX = GetMaximalGradient(pixelLeft, pixelRight);
                    int maxGradY = GetMaximalGradient(pixelUp, pixelDown);

                    magnitudes[i, j] = (int) Math.Sqrt(maxGradX*maxGradX+maxGradY*maxGradY);
                    //directions[i, j] = (int) (maxGradX == 0 ? 90 : Math.Atan(maxGradY/maxGradX)*180.0/Math.PI);
                    directions[i, j] = (int)((Math.Atan2(maxGradY, maxGradX) * 180.0 / Math.PI)+180)%180;
                    //if (directions[i, j] < 0) directions[i, j] += 180;
                    Console.Write(directions[i, j]+" ");
                }
                Console.WriteLine();
            }

            if(!SignedOrientations)
            {
                double[,] histograms = new double[(imgWidth/CellWidth)*(imgHeight/CellHeight),9];

                for (int i = 0; i < imgHeight; i++)
                {
                    for (int j = 0; j < imgWidth; j++)
                    {
                        int cell = j / CellWidth + i / CellHeight * (imgWidth / CellWidth);
                        if (cell >= histograms.GetLength(0)) continue;
                        int directionFirstIndex = (directions[i, j] / 20) % 9;
                        int directionSecondIndex = (directionFirstIndex+1) % 9;
                        double directionFirstValue = (1 - (directions[i, j] % 20) / 20) * magnitudes[i, j];
                        double directionSecondValue = magnitudes[i, j] - directionFirstValue;

                        histograms[cell, directionFirstIndex] += directionFirstValue;
                        histograms[cell, directionSecondIndex] += directionSecondValue;
                    }
                }
                Console.WriteLine("Histograms:");

                for(int i=0;i<histograms.GetLength(0);i++)
                {
                    for(int j=0;j<histograms.GetLength(1);j++)
                    {
                        Console.Write(histograms[i,j]+" ");
                    }
                    Console.WriteLine();
                }

                double[] finalVector = new double[((imgWidth / CellWidth)-BlockWidth+1)*((imgHeight / CellHeight)-_BlockHeight+1) * BlockWidth * BlockHeight * 9];
                int finalVectorCounter = 0;

                for (int i = 0; i + BlockWidth < (imgWidth / CellWidth); i++)
                {
                    for(int j = 0; j + BlockHeight < (imgHeight / CellHeight); j++)
                    {
                        double[] vector = new double[BlockWidth * BlockHeight * 9];
                        double vectorLength = 0;

                        for (int k = 0; k < vector.Length; k++)
                        {
                            int myCellNum = k / 9;
                            int cellNum = (j + myCellNum / BlockWidth) * (imgWidth / CellWidth) + i + myCellNum % BlockWidth;
                            vector[k] = histograms[cellNum, k % 9];
                            vectorLength += vector[k] * vector[k];
                        }

                        vectorLength = Math.Sqrt(vectorLength);

                        for(int k = 0; k < vector.Length; k++)
                        {
                            vector[k] /= vectorLength;
                            finalVector[finalVectorCounter] = vector[k];
                            finalVectorCounter++;
                        }
                    }
                }
                ret = finalVector;
            }

            return ret;
        }

        private int GetMaximalGradient(Color firstPixel, Color secondPixel)
        {
            int max = secondPixel.R - firstPixel.R;

            if (Math.Abs(secondPixel.G - firstPixel.G) > Math.Abs(max)) max = secondPixel.G - firstPixel.G;
            if (Math.Abs(secondPixel.B - firstPixel.B) > Math.Abs(max)) max = secondPixel.B - firstPixel.B;
            return max;
        }

        //private int GetMaximalGradient(Color firstPixel, Color secondPixel)
        //{
        //    int max = Math.Abs(secondPixel.R - firstPixel.R);

        //    if (Math.Abs(secondPixel.G - firstPixel.G) > max) max = Math.Abs(secondPixel.G - firstPixel.G);
        //    if (Math.Abs(secondPixel.B - firstPixel.B) > max) max = Math.Abs(secondPixel.B - firstPixel.B);
        //    return max;
        //}

    }
}
