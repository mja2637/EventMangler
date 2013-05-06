using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace EventMangler.model
{
    abstract class Library<T> where T : XMLable
    {
        protected Dictionary<string, List<T>> lists;
        public Dictionary<string, List<T>> Lists
        {
            get 
            { 
                return lists; 
            }
        }

        protected string eventFileDirectory;
        public string EventFileDirectory
        {
            get
            {
                return eventFileDirectory;
            }
        }

        public Library(string path, string listTag)
        {
            eventFileDirectory = path;
            lists = new Dictionary<string, List<T>>();
            foreach (string eventFile in Directory.GetFiles(path, "*.xml")) updateLists(eventFile, listTag);
        }


        /// <summary>
        /// Add the passed TextList to the Library, and to the underlying events file
        /// </summary>
        /// <param name="eventFile"></param>
        /// <param name="newList"></param>        
        public void addList(string eventFile, T newList)
        {            
            if (lists[eventFile] != null)
            {
                lists[eventFile].Add(newList);
            } else {
                lists.Add(eventFile, new List<T> { newList });
            }

            // Add XElement for new TextList to end of corresponding textList in corresponding event file
            if (Properties.Resources.debug == "true") Console.WriteLine(String.Format("Adding text list element {0}", newList.toXElement().ToString()));
            string xmlString = File.ReadAllText(eventFileDirectory);
            File.WriteAllText(eventFileDirectory, String.Format("{0}\n{1}", xmlString, newList.toXElement().ToString()));
        }

        /// <summary>
        /// Remove the passed TextList from theLibrary, and from the underlying events file
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageList"></param>
        public void removeTextList(string eventFilePath, T list)
        {
            // Remove image from dictionary
            lists[eventFilePath].Remove(list);

            // Remove XElement for TextList from corresponding events file           
            string xmlString = File.ReadAllText(eventFilePath);
            xmlString = xmlString.Remove(xmlString.IndexOf(list.toXElement().ToString()), list.toXElement().ToString().Length);
            File.WriteAllText(eventFilePath, xmlString);
        }

        /// <summary>
        /// Retrieve the TextLists stored in one event file
        /// </summary>
        /// <param name="eventFilePath"></param>
        /// <returns></returns>
        protected void updateLists(string eventFilePath, string listTag)
        {
            // Display the passed filepath
            if (Properties.Resources.debug == "true") Console.WriteLine(String.Format("Scanning {0} for lists", eventFilePath));
            string xmlString = File.ReadAllText(eventFilePath);

            // We have to artificially add a root node because fuckass
            // We have to artificially add a root node
            xmlString = String.Format("<lists>{0}</lists>", xmlString);
            // Also, remove all comments (since some contain invalid characters)
            Regex rComments = new Regex("<!--.*?-->", RegexOptions.Singleline);
            xmlString = rComments.Replace(xmlString, "");
            XDocument eventFileDocument = XDocument.Parse(xmlString);

            var listQuery =
                from
                    tl in eventFileDocument.Descendants(listTag)
                select tl;

            List<T> subLists = new List<T>();
            foreach (var list in listQuery)
            {
                subLists.Add(listFromXML(eventFilePath, list));
            }
            if (subLists.Count > 0)
            {
                Console.WriteLine(String.Format("{0} lists loaded from {1}.", subLists.Count, eventFilePath));
                lists.Add(eventFilePath, subLists);
            }            
        }

        abstract protected T listFromXML(string eventFilePath, XElement list);
    }

    class TextListLibrary : Library<TextList>
    {
        public TextListLibrary(string path) : base(path, "textList") { }

        protected override TextList listFromXML(string eventFilePath, XElement list)
        {
            return new TextList(eventFilePath, list);
        }
    }
}
