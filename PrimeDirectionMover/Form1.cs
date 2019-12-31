using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimeDirectionMover
{
    public partial class Form1 : Form
    {
        int moveDistance = 2;
        Prime prime;
        Image image;
        int iterCount = 1000; //default value of 1000

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        /*simulates creating the prime direction mover image to find out what the min, max, and origin of the image should be. 
        Also creates the image class with proper size. Returns what the origin coord should be.*/
        public int[] preSetup()
        {
            int[] min = { 0, 0 }; //finds out the minimum coordinate for the image
            int[] max = { 0, 0 }; //finds out the maximum coordinate for the image

            int[] coord = { 0, -moveDistance }; //the move modifier coordinate
            int[] currentCoord = { 0, 0 };

            int notificationTimer = 1000; //how often the backgroundworker notifies the main thread of its progress.
            Stopwatch timer = new Stopwatch(); //tracks time
            timer.Start();

            int primeNumber = prime.nextPrime(); //get the first prime, should be 2 always
            for (int i = 1; i <= iterCount; i++)
            {
                if ( timer.ElapsedMilliseconds > notificationTimer) //timer has expired, send notifications to the main thread
                {
                    backgroundWorker1.ReportProgress(50, ((double)i / (double)iterCount));
                    timer.Reset(); //reset and restart the timer.
                    timer.Start();
                }
                if( i == primeNumber )
                {
                    //find the next coordinate
                    currentCoord[0] = currentCoord[0] + coord[0];
                    currentCoord[1] = currentCoord[1] + coord[1];

                    //find out if the x coord is a new min or max
                    if (currentCoord[0] > max[0])
                        max[0] = currentCoord[0];
                    else if (currentCoord[0] < min[0])
                        min[0] = currentCoord[0];
                    //find out if the y coord is a new min or max
                    if (currentCoord[1] > max[1])
                        max[1] = currentCoord[1];
                    else if (currentCoord[1] < min[1])
                        min[1] = currentCoord[1];

                    primeNumber = prime.nextPrime(); //get the next prime
                    if (primeNumber == -1) //if the prime is -1, there must be no more prime numbers
                    {
                        break; //no need to continue the loop.
                    }
                }
                else //change the direction coord
                {
                    if (coord[1] == -moveDistance) //forward, change to right {d, 0}
                    {
                        coord[0] = moveDistance;
                        coord[1] = 0;
                    }
                    else if (coord[0] == moveDistance) //right, change to down {0, d}
                    {
                        coord[0] = 0;
                        coord[1] = moveDistance;
                    }
                    else if (coord[1] == moveDistance) //down, change to left {-d, 0}
                    {
                        coord[0] = -moveDistance;
                        coord[1] = 0;
                    }
                    else //left, change to up {0, -d}
                    {
                        coord[0] = 0;
                        coord[1] = -moveDistance;
                    }
                }
            }
            int sizeX = max[0] - min[0] + 1; //+1 because the 0,0 coord
            int sizeY = max[1] - min[1] + 1; //+1 because the 0,0 coord
            image = new Image(sizeX, sizeY);
            int[] originCoord = { -min[0], -min[1] };
            return originCoord;
        }

        //create the actual prime direction mover image. Box size is the size of the pictureBox.
        private void mainOperations(int[] currentCoord, int[] boxSize)
        {
            int[] coord = { 0, -moveDistance };
            int[] previousCoord = { 0, 0 };

            int notificationTimer = 1000; //how often the backgroundworker notifies the main thread of its progress.
            Stopwatch timer = new Stopwatch(); //tracks time
            timer.Start();

            prime.resetPrimeIndex();
            int primeNumber = prime.nextPrime();
            for (int i = 1; i <= iterCount; i++)
            {
                if (timer.ElapsedMilliseconds > notificationTimer) //if timer has expired, send notifications to the main thread
                {
                    backgroundWorker1.ReportProgress(50, ((double)i / (double)iterCount));
                    timer.Reset(); //restart the timer
                    timer.Start();
                }
                if (i != primeNumber)
                {
                    //find the next coordinate based on the direciton
                    previousCoord[0] = currentCoord[0];
                    previousCoord[1] = currentCoord[1];
                    currentCoord[0] = currentCoord[0] + coord[0];
                    currentCoord[1] = currentCoord[1] + coord[1];
                    image.drawStraightLine(previousCoord, currentCoord, image.getColorFromHSV((double)i / iterCount)); //draw a line from the old coord to the new one

                    primeNumber = prime.nextPrime(); //get the next prime
                    if (primeNumber == -1) //if the prime is -1, there must be no more prime numbers
                    {
                        break; //no need to continue the loop.
                    }
                }
                else //change the direction coord
                {
                    if (coord[1] == -moveDistance) //forward, change to right {d, 0}
                    {
                        coord[0] = moveDistance;
                        coord[1] = 0;
                    }
                    else if (coord[0] == moveDistance) //right, change to down {0, d}
                    {
                        coord[0] = 0;
                        coord[1] = moveDistance;
                    }
                    else if (coord[1] == moveDistance) //down, change to left {-d, 0}
                    {
                        coord[0] = -moveDistance;
                        coord[1] = 0;
                    }
                    else //left, change to up {0, -d}
                    {
                        coord[0] = 0;
                        coord[1] = -moveDistance;
                    }
                }
            }
            backgroundWorker1.ReportProgress(50, image.requestImage(boxSize[0], boxSize[1]));
        }


        //have the background worker make the image
        private void RunButton_Click(object sender, EventArgs e)
        {
            runButton.Enabled = false; //disable the buttons to avoid errors.
            saveButton.Enabled = false;

            int[] imageBoxSize = { imageBox.Width, imageBox.Height };
            backgroundWorker1.RunWorkerAsync(imageBoxSize); //run the prime array generator and the main operation.
        }

        //the second thread's work, this allows the form to be responsive while operations are done, the sender is the image box size.
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            backgroundWorker1.ReportProgress(0, "Generating Primes...");

            //Generate the prime numbers
            prime = new Prime(iterCount);

            int notificationTimer = 1000; //how often the backgroundworker notifies the main thread of its progress.
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (prime.generatePrime()) //generate primes until all the primes are generated
            {
                if (timer.ElapsedMilliseconds > notificationTimer) //if the timer has expired, send notifications to the main thread
                {
                    backgroundWorker1.ReportProgress(50, prime.getGenerationProgress() );
                    timer.Reset(); //reset the timer.
                    timer.Start();
                }
            }
            //END Generate the prime numbers

            backgroundWorker1.ReportProgress(50, "Prework...");
            int[] coordOrigin = preSetup();

            backgroundWorker1.ReportProgress(75, "Generating Image...");
            mainOperations(coordOrigin, (int[])(e.Argument) );

            backgroundWorker1.ReportProgress(100, "");
        }

        //when the iterCountText box is changed, we try to parse the text to the iterCount var (int type)
        private void IterCountText_TextChanged(object sender, EventArgs e)
        {
            int newIterCount = 1;
            try //try parsing the new input
            {
                newIterCount = Int32.Parse(iterCountText.Text);
            }
            catch (Exception) //we weren't able to parse the text as a string, reset it to what the count was
            {
                iterCountText.Text = iterCount.ToString();
            }

            if (newIterCount > 1 && newIterCount < Int32.MaxValue) //set the iterCount if the new value is within the right range.
            {
                iterCount = newIterCount;
            }
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 100)
            {
                runButton.Enabled  = true; //we finished the work, the run button can be re-enabled.
                saveButton.Enabled = true;
            }

            //handle the various types of userstates, this is how the worker passes stuff to the main thread
            if(e.UserState.GetType() == typeof(String) ) //if the info is a string it is made to display in the progressLabel.
            {
                progressLabel.Text = (e.UserState.ToString());
                progressBar.Value = 0; //a new stage just happened, reset the bar.
            }
            else if(e.UserState.GetType() == typeof(double) ) //if the info is a double it is made to update the progress bar.
            {
                progressBar.Value = (int)(((double)e.UserState) * progressBar.Maximum);
            }
            else if(e.UserState.GetType() == typeof(Bitmap)) //if the info is a bitmap, update the imageBox
            {
                imageBox.Image = (Bitmap)(e.UserState);
            }
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saver = new SaveFileDialog();
            saver.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";

            if ( saver.ShowDialog() == DialogResult.OK )
            {
                Clipboard.SetText( saver.FileName );
                image.requestOriginalImage().Save( saver.FileName );
            }
        }
    }

    public class Prime
    {
        //stores all the primes from 0 to whatever size is given as a parameter
        List<int> primeArray = new List<int>();
        int index = -1; //used to quickly find a prime for the caller, this index should only increase. -1 because next prime moves the index by 1.
        int maxNum; //the limit at which we stop generating primes.
        int generateIndex = 2; //the index we use to generate primes, starts at 2 ends at maxNum.

        public Prime(int maxNumber)
        {
            maxNum = maxNumber;
        }

        //make the array of primes from 0 - maxNum, this takes many runs of this function to complete, returns false when the generation is fully done.
        public bool generatePrime()
        {
            bool prime = true; //assume prime until proven otherwise.
            foreach (int n in primeArray) //if i is not divisible by any of the previous primes then it is prime.
            {
                if (generateIndex % n == 0) //i is divisble by n so it is not a factor.
                {
                    prime = false;
                    break;
                }
                else if ( n * n >= generateIndex) //if we are checking a factor that is greater than the sqrt of i, it can not be a factor and there is no need to keep checking..
                {
                    break;
                }
            }
            if (prime) //if the number is prime add it to the array.
            {
                primeArray.Add(generateIndex);
            }

            generateIndex++; //iterate the var for the next run of the function.

            if (generateIndex > maxNum) //we have finished generating all the primes we need.
            {
                primeArray.Add(-1); //sentinal value to mark the end of the prime array.
                return false; //we have finished the prime array.
            }
            return true;
        }

        //return [0-1] value that shows how much of the generation is completed
        public double getGenerationProgress()
        {
            return ((double)generateIndex) / maxNum;
        }

        //tell the caller the next prime number in the array, -1 is added to the end of the array to indicate that there are no more primes.
        public int nextPrime()
        {
            index++;
            return primeArray[index];
        }

        public void resetPrimeIndex()
        {
            index = -1; //-1 because nextPrime will shift the index to 0 when it is called the first time.
        }

    }

    public class Image
    {
        Bitmap picture;
        public Image(int width, int height)
        {
            picture = new Bitmap(width, height); //create the image with requested size
            for (int i = 0; i < width; i++) //set the bacground to black
            {
                for (int j = 0; j < height; j++)
                {
                    picture.SetPixel(i, j, Color.FromArgb(0, 0, 0)); //set each pixel to black.
                }
            }
        }

        //return the bitmap with a scaling. Fixes the blurry scale that the form uses, this makes sharp edges.
        public Bitmap requestImage(int width, int height)
        {
            //width/picture.Width is one possible scaling factor, height/picture.height is the other option.
            int scaleFactor = width / picture.Width;
            if (picture.Height * scaleFactor <= height) //check if the horizontal scale factor works.
            {
                width = (picture.Width * scaleFactor);
                height = (picture.Height * scaleFactor);
            }
            else //use the vertical scale factor instead.
            {
                scaleFactor = height / picture.Height; //vertical scale factor.
                width = (picture.Width * scaleFactor);
                height = (picture.Height * scaleFactor);
            }

            if (scaleFactor == 0) //if the original image is bigger than the target image, the default resizing works so return the original
            {
                return picture;
            }

            Bitmap image = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    image.SetPixel(x, y, picture.GetPixel(x / scaleFactor, y / scaleFactor ) ); //map the original to the new image.
                }
            }
            return image;
        }

        //return the image with original size.
        public Bitmap requestOriginalImage()
        {
            return picture;
        }

        //draw a line with the requested color from the startCoord to endCoord;
        public void drawStraightLine(int[] startCoord, int[] endCoord, Color color)
        {
            if (startCoord[0] == endCoord[0]) //draw vertical line
            {
                for (int i = 0; i <= Math.Abs(endCoord[1] - startCoord[1]); i++)
                {
                    if (endCoord[1] > startCoord[1])
                    {
                        picture.SetPixel(startCoord[0], startCoord[1] + i, color);
                    }
                    else
                    {
                        picture.SetPixel(startCoord[0], endCoord[1] + i, color);
                    }
                }
            }
            else //draw horizontal line
            {
                for (int i = 0; i <= Math.Abs(endCoord[0] - startCoord[0]); i++)
                {
                    if (endCoord[0] > startCoord[0])
                    {
                        picture.SetPixel(startCoord[0] + i, startCoord[1], color);
                    }
                    else
                    {
                        picture.SetPixel(endCoord[0] + i, startCoord[1], color);
                    }
                }
            }
        }

        //get an rgb color value from an Hue value when h is between 0-1, saturation and value is always 1, adapted from https://stackoverflow.com/questions/2941048/how-to-simplify-fractions-in-c
        public Color getColorFromHSV(double hue)
        {
            double saturation = 1;
            double value = 1;
            int hueCase = Convert.ToInt32(Math.Floor(hue * 6)) % 6; //different hue cases, 6 different cases.
            double f = hue * 6 - Math.Floor(hue * 6); //f is the floating point remainder of hue*6.

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hueCase == 0)
            {

                return Color.FromArgb(255, v, t, p);
            }
            else if (hueCase == 1)
            {
                return Color.FromArgb(255, q, v, p);
            }
            else if (hueCase == 2)
            {
                return Color.FromArgb(255, p, v, t);
            }
            else if (hueCase == 3)
            {
                return Color.FromArgb(255, p, q, v);
            }
            else if (hueCase == 4)
            {
                return Color.FromArgb(255, t, p, v);
            }
            else //hueCase == 5.
            {
                return Color.FromArgb(255, v, p, q);
            }
        }
    }
}
