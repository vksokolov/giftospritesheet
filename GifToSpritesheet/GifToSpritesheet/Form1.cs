using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GifToSpritesheet
{
    public partial class Form1 : Form
    {
        string pathToGif;
        Bitmap spriteSheet;
        AnimatedGif gifImg;
        int gifWidth;
        int gifHeight;
        int rows;
        int cols;
        int frameCount;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pathToGif = openFileDialog1.FileName;
                button2.Enabled = true;


                gifImg = new AnimatedGif(pathToGif);
                frameCount = gifImg.Images.Count;
                gifWidth = gifImg.Images[0].Image.Width;
                gifHeight = gifImg.Images[0].Image.Height;
                rows = (int)Math.Sqrt(frameCount);
                cols = rows;
                if ((float)gifWidth / gifHeight > 1)
                {
                    rows++;
                }
                else
                {
                    cols++;
                }
            }
            else
            {
                button2.Enabled = false;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            spriteSheet = new Bitmap(gifWidth * rows, gifHeight * cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i + j * rows < frameCount)
                        CopyRegionIntoImage(new Bitmap(gifImg.Images[i + j * rows].Image), new Rectangle(0, 0, gifWidth, gifHeight), ref spriteSheet, new Rectangle(gifWidth * i, gifHeight * j, gifWidth, gifHeight));
                }
            }
            System.Drawing.Imaging.Encoder pointFilter = System.Drawing.Imaging.Encoder.Quality;
            ImageCodecInfo pngEncoder = GetEncoder(ImageFormat.Png);
            EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(pointFilter, 0L);
            spriteSheet.Save((new FileInfo(pathToGif)).Name + "_spriteSheet.png", pngEncoder, parameters);
            spriteSheet.Dispose();
            richTextBox1.AppendText("Finished!");
        }

        private static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = radioButton2.Checked;
            label2.Enabled = radioButton2.Checked;
            rowUpDown.Enabled = radioButton2.Checked;
            colUpDown.Enabled = radioButton2.Checked;
            if (radioButton1.Checked)
            {
                rowUpDown.Value = rows;
                colUpDown.Value = cols;
            }
        }

        private void upDown_ValueChanged(object sender, EventArgs e)
        {
            int c = (int)colUpDown.Value;
            int r = (int)rowUpDown.Value;
            if(radioButton2.Checked)
            if (c * r < frameCount)
            {
                richTextBox1.AppendText("\nOnly " + (c * r) + " frames will be saved. Add columns/rows if you want to save all animation");
            }
        }
    }   
        public class AnimatedGif
        {
            private List<AnimatedGifFrame> mImages = new List<AnimatedGifFrame>();
            PropertyItem mTimes;
            public AnimatedGif(string path)
            {
                try
                {
                    Image img = Image.FromFile(path);
                    int frames = img.GetFrameCount(FrameDimension.Time);
                    if (frames <= 1) throw new ArgumentException("Image not animated");
                    byte[] times = img.GetPropertyItem(0x5100).Value;
                    int frame = 0;
                    for (;;)
                    {
                        int dur = BitConverter.ToInt32(times, 4 * frame);
                        mImages.Add(new AnimatedGifFrame(new Bitmap(img), dur));
                        if (++frame >= frames) break;
                        img.SelectActiveFrame(FrameDimension.Time, frame);
                    }
                    img.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
            public List<AnimatedGifFrame> Images { get { return mImages; } }
        }

        public class AnimatedGifFrame
        {
            private int mDuration;
            private Image mImage;
            internal AnimatedGifFrame(Image img, int duration)
            {
                mImage = img; mDuration = duration;
            }
            public Image Image { get { return mImage; } }
            public int Duration { get { return mDuration; } }
        }
    }

