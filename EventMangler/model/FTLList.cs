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
        public string Name { get; protected set; }

        /// <summary>
        /// XML file in which this FTLList is located
        /// </summary>
        public string EventFile { get; protected set; }

        public string Tag { get; protected set; }

        /// <summary>
        /// The actual, no-shit content contained in this FTLList
        /// </summary>
        public ObservableCollection<T> Items { get; protected set; }

        public FTLList(string eventFile, XElement listXML)
        {
            // Set fields
            EventFile = eventFile;
            Name = listXML.Attribute("name").Value;
            Tag = listXML.Name.ToString();
            Items = itemsFromListXElement(listXML);
            
            // Set collection behavior
            Items.CollectionChanged += HandleChange;
        }

        abstract protected ObservableCollection<T> itemsFromListXElement(XElement listXML);

        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            string xmlString = File.ReadAllText(EventFile);
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
            File.WriteAllText(EventFile, xmlString);
        }

        private void insert(IEnumerable<T> newItems, ref string xmlString)
        {
            string newXml = "";
            foreach (var x in newItems)
            {
                newXml += ((T)x).toXElement().ToString();
            }
            xmlString = xmlString.Insert(xmlString.IndexOf(String.Format("</{0}>", Tag), xmlString.IndexOf(String.Format("name=\"{0}\">", Name))), String.Format("\t{0}\n", newXml));
        }

        private void remove(IEnumerable<T> oldItems, ref string xmlString)
        {
            foreach (var x in oldItems)
            {
                string remTag = ((T)x).toXElement().ToString();
                xmlString = xmlString.Remove(xmlString.IndexOf(remTag, xmlString.IndexOf(String.Format("name=\"{0}\">", Name))), remTag.Length);
            }
        }

        public XElement toXElement()
        {
            XElement ret = new XElement(Tag, new XAttribute("name", Name));
            foreach (T item in Items) ret.Add(item.toXElement());
            return ret;
        }
    }
}
