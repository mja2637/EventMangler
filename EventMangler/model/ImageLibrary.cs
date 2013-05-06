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
    class ImageLibrary : Library<ImageList>
    {
        public ImageLibrary(string path) : base(path, "imageList")
        { }

        /// <summary>
        /// Construct an FTLImage using the file at the passed path.
        /// Move the passed image into the mod directory
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
            // Identify correct imageList in dictionary            
            // Add image to list
            getImageListByName(imageList).Images.Add(newImage);
        }

        private ImageList getImageListByName(string imageListName)
        {
            var destList = from list in Lists.Values.SelectMany(x => x) where list.Name == imageListName select list;
            return (ImageList)destList;
        }

        /// <summary>
        /// Remove the passed FTLImage from the ImageLibrary, and from the underlying events_Imagelist.xml
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageList"></param>
        public void removeFTLImage(FTLImage image, string imageList)
        {
            // Remove image from dictionary
            getImageListByName(imageList).Images.Remove(image);

            removeUnusedImages();
        }

        public void removeUnusedImages()
        {
            // Query our imageLists dictionary for all unique image paths
            HashSet<string> uniqueImageFiles;
            var uniqueImages =
                from imagelist in Lists.Values.SelectMany(x => x)
                from image in imagelist.Images
                select image.path;
            uniqueImageFiles = new HashSet<string>(uniqueImages.Distinct<string>());

            // Iterate over image files in /stars, removing all unused            
            string imageFilePath = Path.Combine(Properties.Resources.mod_root_dir, "img/stars");
            foreach (string imageFile in Directory.EnumerateFiles(imageFilePath))
            {
                if (!uniqueImageFiles.Contains<string>(String.Format("stars/{0}", Path.GetFileName(imageFile))))
                {
                    Console.WriteLine(String.Format("Removing unused image file {0}", imageFile));
                    // IO Exception when file is in use?
                    File.Delete(Path.Combine(imageFilePath, imageFile));
                }
            }
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

        protected override ImageList listFromXML(string eventFilePath, XElement list)
        {
            return new ImageList(eventFilePath, list);
        }
    }
}
