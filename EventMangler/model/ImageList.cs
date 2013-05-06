using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace EventMangler.model
{
    class ImageList : FTLImageComposite
    {        
        /// <summary>
        /// Name of this imageList
        /// </summary>
        private string name;
        public string Name { get { return name; } }

        /// <summary>
        /// XML file in which this textList is located
        /// </summary>
        private string eventFile;
        public string EventFile { get { return eventFile; } }

        /// <summary>
        /// The actual, no-shit images contained in this textList
        /// </summary>
        private ObservableCollection<FTLImage> images;
        public ObservableCollection<FTLImage> Images { get { return images; } }

        public ImageList(string eventFile, XElement listXML)
        {
            // Set fields
            this.eventFile = eventFile;
            this.name = listXML.Attribute("name").Value;
            this.images = new ObservableCollection<FTLImage>();

            // Add text strings to texts
            foreach (XElement image in listXML.Elements("img")) this.images.Add(new FTLImage(image));            

            // Set collection behavior
            images.CollectionChanged += HandleChange;
        }

        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            string xmlString = File.ReadAllText(eventFile);
            if (e.Action == NotifyCollectionChangedAction.Add)
            {                
                string newXml = "";
                foreach (var x in e.NewItems)
                {
                    newXml += String.Format("\t{0}\n", ((FTLImage)x).toXElement().ToString());
                }
                xmlString = xmlString.Insert(xmlString.IndexOf("</imageList>", xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), newXml);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var x in e.OldItems)
                {
                    string xElementString = ((FTLImage)x).toXElement().ToString();
                    int imageListIndex = xmlString.IndexOf(String.Format("name=\"{0}\">", this.name));
                    xmlString = xmlString.Remove(xmlString.IndexOf(xElementString, imageListIndex), String.Format("\t{0}", xElementString).Length);
                }                
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var y in e.OldItems)
                {
                    foreach (var x in e.OldItems)
                    {
                        xmlString = xmlString.Remove(xmlString.IndexOf(((FTLImage)x).toXElement().ToString(), xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), String.Format("\t{0}\n", ((FTLImage)x).toXElement()).Length);
                    }

                    string newXml = "";
                    foreach (var x in e.NewItems)
                    {
                        newXml += String.Format("\t{0}\n", ((FTLImage)x).toXElement().ToString());
                    }
                    xmlString = xmlString.Insert(xmlString.IndexOf("</imageList>", xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), newXml);
                }
            }
            File.WriteAllText(eventFile, xmlString);
        }

        public XElement toXElement()
        {
            XElement ret = new XElement("imageList", new XAttribute("name", name));
            foreach (FTLImage image in Images) ret.Add(image.toXElement());
            return ret;
        }
    }
}
