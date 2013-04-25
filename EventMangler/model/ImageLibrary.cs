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

        /// <summary>
        /// Construct an FTLImage using the file at the passed path.
        /// Add the passed FTLImage to the ImageLibrary, and to the underlying events_imagelist.xml
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <param name="imageList"></param>
        /// <returns></returns>
        public FTLImage addImageFromPath(string imageFilePath, string imageList)
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
                FTLImage newImage = new FTLImage(Path.Combine("stars/", filename), (int)src.Width, (int)src.Height);

                // Add image to library & XML
                addFTLImage(newImage, imageList);
                return newImage;
            }
            else throw new FileNotFoundException();
        }

        /// <summary>
        /// Add the passed FTLImage to the ImageLibrary, and to the underlying events_imagelist.xml
        /// </summary>
        /// <param name="newImage"></param>
        /// <param name="imageList"></param>
        public void addFTLImage(FTLImage newImage, string imageList)
        {
            // Add image to dictionary
            imageLists[imageList].Add(newImage);

            // Add XElement for FTLImage to end of corresponding imagelist in events_imagelist
            System.Console.WriteLine(newImage.toXElement().ToString());
            string xmlString = File.ReadAllText(eventFilePath);
            // lol
            File.WriteAllText(eventFilePath, xmlString.Insert(xmlString.IndexOf("</imageList>", xmlString.IndexOf(String.Format("name=\"{0}\">", imageList))), String.Format("\t{0}\n", newImage.toXElement().ToString())));
        }

        /// <summary>
        /// Remove the passed FTLImage from the ImageLibrary, and from the underlying events_Imagelist.xml
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageList"></param>
        public void removeFTLImage(FTLImage image, string imageList)
        {
            // Remove image from dictionary
            imageLists[imageList].Remove(image);

            // Remove XElement for FTLImage from corresponding imagelist in events_imagelist            
            string xmlString = File.ReadAllText(eventFilePath);
            // lol
            File.WriteAllText(eventFilePath, xmlString.Remove(xmlString.IndexOf("\t" + image.toXElement().ToString(), xmlString.IndexOf(String.Format("name=\"{0}\">", imageList))), (String.Format("\t{0}\n", image.toXElement().ToString())).Length));
        }

        protected Dictionary<string, List<FTLImage>> getImageLists(string eventFilePath)
        {
            // Display the passed filepath
            System.Console.WriteLine("Loading imageLists from " + eventFilePath);
            string xmlString = File.ReadAllText(eventFilePath);

            // We have to artificially add a root node because fuckass
            XDocument events_imageList = XDocument.Parse(String.Format("<imageLists>{0}</imageLists>", xmlString));

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
