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
    abstract class FTLList<T> : XMLable where T : XMLable
    {        
        /// <summary>
        /// Name of this FTLList
        /// </summary>
        protected string name;
        public string Name { get { return name; } }

        /// <summary>
        /// XML file in which this FTLList is located
        /// </summary>
        protected string eventFile;
        public string EventFile { get { return eventFile; } }

        protected string tag;

        /// <summary>
        /// The actual, no-shit content contained in this FTLList
        /// </summary>
        protected ObservableCollection<T> texts;
        public ObservableCollection<T> Items { get { return texts; } }

        public FTLList(string eventFile, XElement listXML)
        {
            // Set fields
            this.eventFile = eventFile;
            this.name = listXML.Attribute("name").Value;
            this.tag = listXML.Name.ToString();
            this.texts = itemsFromListXElement(listXML);
            
            // Set collection behavior
            texts.CollectionChanged += HandleChange;
        }

        abstract protected ObservableCollection<T> itemsFromListXElement(XElement listXML);

        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            string xmlString = File.ReadAllText(eventFile);
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                insert(e.NewItems.Cast<T>(), ref xmlString);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                remove(e.OldItems.Cast<T>(), ref xmlString);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                remove(e.OldItems.Cast<T>(), ref xmlString);
                insert(e.NewItems.Cast<T>(), ref xmlString);
            }
            File.WriteAllText(eventFile, xmlString);
        }

        private void insert(IEnumerable<T> newItems, ref string xmlString)
        {
            string newXml = "";
            foreach (var x in newItems)
            {
                newXml += ((T)x).toXElement().ToString();
            }
            xmlString = xmlString.Insert(xmlString.IndexOf(String.Format("</{0}>", tag), xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), String.Format("\t{0}\n", newXml));
        }

        private void remove(IEnumerable<T> oldItems, ref string xmlString)
        {
            foreach (var x in oldItems)
            {
                string remTag = ((T)x).toXElement().ToString();
                xmlString = xmlString.Remove(xmlString.IndexOf(remTag, xmlString.IndexOf(String.Format("name=\"{0}\">", this.name))), remTag.Length);
            }
        }

        public XElement toXElement()
        {
            XElement ret = new XElement(tag, new XAttribute("name", name));
            foreach (T item in Items) ret.Add(item.toXElement());
            return ret;
        }
    }
}
