using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grabber
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap bmp = (Bitmap)Image.FromFile("pano3.jpg");
            ConvertToEquirecTangular(bmp, 3200, 2400);
            /*var doc = new HtmlDocument();
            doc.Load(args[0]);

            //var nodes = doc.DocumentNode.Descendants("img").Select(y => y.Descendants().Where(x => x.Attributes["id"].Value.StartsWith("Panorama"))).ToList();
            var a = doc.DocumentNode.SelectNodes("//img");

            //foreach(var i in a)
            List<string> files = new List<string>();
            for (int i = 0; i < a.Count; i++)
            {
                Base64ToImage(a[i].Attributes["id"].Value + ".jpg", a[i].Attributes["src"].Value);
                files.Add(a[i].Attributes["id"].Value + ".jpg");
                //Console.WriteLine(i.Attributes["src"].Value);
            }
            var vmp = CombineBitmap(files.ToArray());
            vmp.Save("final.jpg", ImageFormat.Jpeg);*/

        }
        public static void Base64ToImage(string fileName, string b64)
        {
            byte[] img = Convert.FromBase64String(b64.Remove(0, 23));
            MemoryStream ms = new MemoryStream(img, 0, img.Length);

            ms.Write(img, 0, img.Length);
            Image image = Image.FromStream(ms, true);
            Console.WriteLine(String.Format("{0}: {1}x{2}", fileName, image.Height, image.Width));
            image.Save(fileName);
        }
        public static byte[] ConvertToEquirecTangular(Bitmap input, int outWidth, int outHeight)
        {
            Bitmap bitmap = new Bitmap(outWidth, outHeight, PixelFormat.Format24bppRgb);
            float u, v;
            float phi, theta;
            int cubeFaceWidth, cubeFaceHeight;

            cubeFaceWidth = input.Width / 4;
            cubeFaceHeight = input.Height / 3;

            for (int j = 0; j < bitmap.Height; j++)
            {
                v = 1 - ((float)j / bitmap.Height);
                theta = v * (float)Math.PI;

                for(int i = 0; i < bitmap.Width; i++)
                {
                    u = ((float)i / bitmap.Width);
                    phi = u * 2 * (float)Math.PI;

                    float x, y, z;
                    x = (float)Math.Sin(phi) * (float)Math.Sin(theta) * -1;
                    y = (float)Math.Cos(theta);
                    z = (float)Math.Cos(phi) * (float)Math.Sin(theta) * -1;

                    float xa, ya, za;
                    float a;

                    a = Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
                    xa = x / a;
                    ya = y / a;
                    za = z / a;
                    Color color;
                    int xPixel, yPixel;
                    int xOffset, yOffset;

                    if (xa == 1)
                    {
                        //Right
                        xPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceWidth);
                        xOffset = 2 * cubeFaceWidth; //Offset
                        yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                        yOffset = cubeFaceHeight; //Offset
                    }
                    else if (xa == -1)
                    {
                        //Left
                        xPixel = (int)((((za + 1f) / 2f)) * cubeFaceWidth);
                        xOffset = 0;
                        yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                        yOffset = cubeFaceHeight;
                    }
                    else if (ya == 1)
                    {
                        //Up
                        xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                        xOffset = cubeFaceWidth;
                        yPixel = (int)((((za + 1f) / 2f) - 1f) * cubeFaceHeight);
                        yOffset = 2 * cubeFaceHeight;
                    }
                    else if (ya == -1)
                    {
                        //Down
                        xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                        xOffset = cubeFaceWidth;
                        yPixel = (int)((((za + 1f) / 2f)) * cubeFaceHeight);
                        yOffset = 0;
                    }
                    else if (za == 1)
                    {
                        //Front
                        xPixel = (int)((((xa + 1f) / 2f)) * cubeFaceWidth);
                        xOffset = cubeFaceWidth;
                        yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                        yOffset = cubeFaceHeight;
                    }
                    else if (za == -1)
                    {
                        //Back
                        xPixel = (int)((((xa + 1f) / 2f) - 1f) * cubeFaceWidth);
                        xOffset = 3 * cubeFaceWidth;
                        yPixel = (int)((((ya + 1f) / 2f)) * cubeFaceHeight);
                        yOffset = cubeFaceHeight;
                    }
                    else
                    {
                        xPixel = 0;
                        yPixel = 0;
                        xOffset = 0;
                        yOffset = 0;
                    }

                    xPixel = Math.Abs(xPixel);
                    yPixel = Math.Abs(yPixel);
                    xPixel += xOffset;
                    yPixel += yPixel;

                    
                    color = input.GetPixel(xPixel, yPixel);
                    bitmap.SetPixel(i, j, color);
                    
                }
                
            }
            bitmap.Save("out.jpg", ImageFormat.Jpeg);
            ImageConverter imageConverter = new ImageConverter();
            return (byte[])imageConverter.ConvertTo(bitmap, typeof(byte[]));
        }
        public static System.Drawing.Bitmap CombineBitmap(string[] files)
        {
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                foreach (string image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);

                    //update the size of the final bitmap
                    width += bitmap.Width;
                    height = bitmap.Height > height ? bitmap.Height : height;

                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.Black);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                //throw ex;
                throw;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }
    }
}
