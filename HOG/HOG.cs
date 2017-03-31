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
            int[,] magnitudes = new int[imgWidth, imgHeight];
            int[,] directions = new int[imgWidth, imgHeight];

            for(int i = 0; i < imgWidth; i++)
            {
                for(int j = 0; j < imgHeight; j++)
                {
                    var pixelUp = j - 1 < 0 ? Color.Black : Image.GetPixel(i, j-1);
                    var pixelRight = i + 1 >= imgWidth ? Color.Black : Image.GetPixel(i + 1, j);
                    var pixelDown = j + 1 >= imgHeight ? Color.Black : Image.GetPixel(i, j + 1);
                    var pixelLeft = i - 1 < 0 ? Color.Black : Image.GetPixel(i - 1, j);

                    int maxGradX = Math.Max(Math.Abs(pixelRight.R-pixelLeft.R), Math.Abs(pixelRight.G - pixelLeft.G));
                    maxGradX = Math.Max(maxGradX, Math.Abs(pixelRight.B - pixelLeft.B));
                    int maxGradY = Math.Max(Math.Abs(pixelDown.R - pixelUp.R), Math.Abs(pixelDown.G - pixelUp.G));
                    maxGradX = Math.Max(maxGradX, Math.Abs(pixelDown.B - pixelUp.B));

                    magnitudes[i, j] = (int) Math.Sqrt(maxGradX*maxGradX+maxGradY*maxGradY);
                    directions[i, j] = (int) Math.Atan2(maxGradY, maxGradX);
                }
            }

            if(!SignedOrientations)
            {
                double[,] histograms = new double[(imgWidth/CellWidth)*(imgHeight/CellHeight),9];

                for (int i = 0; i < imgWidth; i++)
                {
                    for (int j = 0; j < imgHeight; j++)
                    {
                        int cell = i / CellWidth + j / CellHeight * (imgWidth / CellWidth);
                        if (cell >= histograms.GetLength(0)) continue;
                        int directionFirstIndex = (directions[i, j] / 20) % 9;
                        int directionSecondIndex = (directionFirstIndex+1) % 9;
                        double directionFirstValue = (1 - (directions[i, j] % 20) / 20) * magnitudes[i, j];
                        double directionSecondValue = magnitudes[i, j] - directionFirstValue;

                        histograms[cell, directionFirstIndex] += directionFirstValue;
                        histograms[cell, directionSecondIndex] += directionSecondValue;
                    }
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

    }
}
