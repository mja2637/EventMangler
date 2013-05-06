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
    class TextList : FTLTextComposite
    {        
        /// <summary>
        /// Name of this textList
        /// </summary>
        private string name;
        public string Name { get { return name; } }

        /// <summary>
        /// XML file in which this textList is located
        /// </summary>
        private string eventFile;
        public string EventFile { get { return eventFile; } }

        /// <summary>
        /// The actual, no-shit texts contained in this textList
        /// </summary>
        private ObservableCollection<FTLText> texts;
        public ObservableCollection<FTLText> Texts { get { return texts; } }

        public TextList(string eventFile, XElement listXML)
        {
            // Set fields
            this.eventFile = eventFile;
            this.name = listXML.Attribute("name").Value;
            this.texts = new ObservableCollection<FTLText>();

            // Add text strings to texts
            foreach (XElement text in listXML.Elements("test")) this.texts.Add(new FTLText(text));            

            // Set collection behavior
            texts.CollectionChanged += HandleChange;
        }

        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {                
                string newXml = "";
                foreach (var x in e.NewItems)
                {
                    newXml += String.Format("\t<text>{0}</text>\n", x);
                }
                string xmlString = File.ReadAllText(eventFile);
                File.WriteAllText(eventFile, xmlString.Insert(xmlString.IndexOf("</textList>", xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), newXml));
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                string xmlString = File.ReadAllText(eventFile);
                foreach (var x in e.OldItems)
                {
                    xmlString.Remove(xmlString.IndexOf(String.Format("\t<text>{0}</text>\n", x), xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), String.Format("\t<text>{0}</text>\n", x).Length);
                }                
                File.WriteAllText(eventFile, xmlString);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var y in e.OldItems)
                {
                    string xmlString = File.ReadAllText(eventFile);
                    foreach (var x in e.OldItems)
                    {
                        xmlString.Remove(xmlString.IndexOf(String.Format("\t<text>{0}</text>\n", x), xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), String.Format("\t<text>{0}</text>\n", x).Length);
                    }                    

                    string newXml = "";
                    foreach (var x in e.NewItems)
                    {
                        newXml += String.Format("\t<text>{0}</text>\n", x);
                    }                    
                    File.WriteAllText(eventFile, xmlString.Insert(xmlString.IndexOf("</textList>", xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), newXml));
                }
            }
        }

        public override XElement toXElement()
        {
            XElement ret = new XElement("textList", new XAttribute("name", name));
            foreach (FTLText text in Texts) ret.Add(text.toXElement());
            return ret;
        }
    }
}
