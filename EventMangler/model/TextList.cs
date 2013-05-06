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
    class TextList : FTLList<FTLText>, FTLTextComposite
    {
        public TextList(string eventFile, XElement listXML) : base(eventFile, listXML)
        { }

        protected override ObservableCollection<FTLText> itemsFromListXElement(XElement listXML)
        {
            ObservableCollection<FTLText> textItems = new ObservableCollection<FTLText>();
            foreach (XElement item in listXML.Elements())
            {
                textItems.Add(new FTLText(item));
            }
            return textItems;
        }
    } 
}
