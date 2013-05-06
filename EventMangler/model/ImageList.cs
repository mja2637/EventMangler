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
    class ImageList : FTLList<FTLImage>, FTLImageComposite
    {
        public ImageList(string eventFile, XElement listXML)
            : base(eventFile, listXML)
        { }

        protected override ObservableCollection<FTLImage> itemsFromListXElement(XElement listXML)
        {
            ObservableCollection<FTLImage> textItems = new ObservableCollection<FTLImage>();
            foreach (XElement item in listXML.Elements())
            {
                textItems.Add(new FTLImage(item));
            }
            return textItems;
        }
    }
}
