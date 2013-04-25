using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace EventMangler.model
{
    class ImageLibrary
    {
        private Dictionary<string, List<FTLImage>> imageLists;
        private string eventFilePath;

        public ImageLibrary(string path)
        {            
            imageLists = getImageLists(path);
            eventFilePath = path;
        }

        public Dictionary<string, List<FTLImage>> getImageLists()
        {
            return imageLists;
        }

        public FTLImage addImageFromPath(string imageFilePath, string imageList = "PLANET_CLOSE")
        {
            if (File.Exists(imageFilePath))
            {
                // Move image at imageFilePath to mod image directory
                string filename = Path.GetFileName(imageFilePath);
                string destFolder = Path.Combine(Properties.Resources.mod_root_dir, "img/stars/");
                if (!File.Exists(Path.Combine(destFolder, filename))) File.Copy(imageFilePath, Path.Combine(destFolder, filename));
                
                // Get source for deriving image dimensions
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(Path.Combine(destFolder, filename), UriKind.Absolute);
                src.EndInit();

                // Create new FTLImage
                FTLImage newImage = new FTLImage(Path.Combine("stars/", filename), (int) src.Width, (int) src.Height);
                imageLists[imageList].Add(newImage);

                // Add XElement for FTLImage to end of corresponding imagelist in events_imagelist
                System.Console.WriteLine(newImage.toXElement().ToString());
                string xmlString = File.ReadAllText(eventFilePath);               
                // lol
                File.WriteAllText(eventFilePath, xmlString.Insert(xmlString.IndexOf("</imageList>", xmlString.IndexOf("name=\"" + imageList + "\">")), newImage.toXElement().ToString() + "\n"));
                return newImage;
            } else throw new FileNotFoundException();            
        }

        protected Dictionary<string, List<FTLImage>> getImageLists(string eventFilePath)
        {
            // Display the passed filepath
            System.Console.WriteLine("Loading imageLists from " + eventFilePath);
            string xmlString = File.ReadAllText(eventFilePath);
            XDocument events_imageList = XDocument.Parse("<imageLists>" + xmlString + "</imageLists>");

            Dictionary<string, List<FTLImage>> imageLists = new Dictionary<string, List<FTLImage>>();
            var imageListQuery =
                from
                    il in events_imageList.Descendants("imageList")
                select il;
            foreach (var imageList in imageListQuery)
            {
                var imageQuery =
                    from
                        i in imageList.Descendants("img")
                    select i;
                List<FTLImage> images = new List<FTLImage>();
                foreach (var image in imageQuery) images.Add(new FTLImage(
                        image.Value, // Image path
                        (int)image.Attribute("w"),
                        (int)image.Attribute("h")));
                imageLists.Add(imageList.Attribute("name").Value, images);
            }
            foreach (string list in imageLists.Keys) Console.WriteLine(list);
            return imageLists;
        }
    }

    public struct FTLImage
    {
        public readonly int w, h;
        public readonly string path;

        public FTLImage(string path, int w, int h)
        {
            this.path = path;
            this.w = w;
            this.h = h;
        }

        public XElement toXElement()
        {
            return new XElement("img", new XAttribute("w", w), new XAttribute("h", h), path);
        }
    }
}
