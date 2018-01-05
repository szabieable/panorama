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
            var doc = new HtmlDocument();
            doc.Load(args[0]);

            //var nodes = doc.DocumentNode.Descendants("img").Select(y => y.Descendants().Where(x => x.Attributes["id"].Value.StartsWith("Panorama"))).ToList();
            var a = doc.DocumentNode.SelectNodes("//img");

            //foreach(var i in a)
            List<string> files = new List<string>();
            for(int i =0; i < a.Count; i ++)
            {
                Base64ToImage(a[i].Attributes["id"].Value + ".jpg", a[i].Attributes["src"].Value);
                files.Add(a[i].Attributes["id"].Value + ".jpg");
                //Console.WriteLine(i.Attributes["src"].Value);
            }
            var vmp = CombineBitmap(files.ToArray());
            vmp.Save("final.jpg",ImageFormat.Jpeg);
            
        }
        public static void Base64ToImage(string fileName, string b64)
        {
            byte[] img = Convert.FromBase64String(b64.Remove(0,23));
            MemoryStream ms = new MemoryStream(img, 0, img.Length);

            ms.Write(img, 0, img.Length);
            Image image = Image.FromStream(ms, true);
            Console.WriteLine(String.Format("{0}: {1}x{2}", fileName, image.Height, image.Width));
            image.Save(fileName);
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

                foreach (string image in files )
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
