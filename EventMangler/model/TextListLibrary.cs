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
    class TextListLibrary
    {
        private Dictionary<string, List<TextList>> textLists;
        public Dictionary<string, List<TextList>> TextLists
        {
            get 
            { 
                return textLists; 
            }
        }

        private string eventFileDirectory;
        public string EventFileDirectory
        {
            get
            {
                return eventFileDirectory;
            }
        }

        public TextListLibrary(string path)
        {
            eventFileDirectory = path;
            textLists = new Dictionary<string, List<TextList>>();
            foreach (string eventFile in Directory.GetFiles(path, "*.xml")) updateTextLists(eventFile);
        }


        /// <summary>
        /// Add the passed TextList to the Library, and to the underlying events file
        /// </summary>
        /// <param name="eventFile"></param>
        /// <param name="newTextList"></param>        
        public void addTextList(string eventFile, TextList newTextList)
        {            
            if (textLists[eventFile] != null)
            {
                textLists[eventFile].Add(newTextList);
            } else {
                textLists.Add(eventFile, new List<TextList> { newTextList });
            }

            // Add XElement for new TextList to end of corresponding textList in corresponding event file
            if (Properties.Resources.debug == "true") Console.WriteLine(String.Format("Adding text list element {0}", newTextList.toXElement().ToString()));
            string xmlString = File.ReadAllText(eventFileDirectory);
            File.WriteAllText(eventFileDirectory, xmlString += "\n" + newTextList.toXElement().ToString());
        }

        /// <summary>
        /// Remove the passed TextList from theLibrary, and from the underlying events file
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageList"></param>
        public void removeTextList(string eventFilePath, TextList textList)
        {
            // Remove image from dictionary
            textLists[eventFilePath].Remove(textList);

            // Remove XElement for TextList from corresponding events file           
            string xmlString = File.ReadAllText(eventFilePath);
            xmlString = xmlString.Remove(xmlString.IndexOf(textList.toXElement().ToString()), textList.toXElement().ToString().Length);
            File.WriteAllText(eventFilePath, xmlString);
        }

        /// <summary>
        /// Retrieve the TextLists stored in one event file
        /// </summary>
        /// <param name="eventFilePath"></param>
        /// <returns></returns>
        protected void updateTextLists(string eventFilePath)
        {
            // Display the passed filepath
            if (Properties.Resources.debug == "true") Console.WriteLine(String.Format("Scanning {0} for textLists", eventFilePath));
            string xmlString = File.ReadAllText(eventFilePath);

            // We have to artificially add a root node because fuckass
            // We have to artificially add a root node
            xmlString = String.Format("<textLists>{0}</textLists>", xmlString);
            // Also, remove all comments (since some contain invalid characters)
            Regex rComments = new Regex("<!--.*?-->", RegexOptions.Singleline);
            xmlString = rComments.Replace(xmlString, "");
            XDocument eventFileDocument = XDocument.Parse(xmlString);

            var textListQuery =
                from
                    tl in eventFileDocument.Descendants("textList")
                select tl;

            List<TextList> lists = new List<TextList>();
            foreach (var textList in textListQuery)
            {                
                lists.Add(new TextList(eventFilePath, textList));
            }
            if (lists.Count > 0) Console.WriteLine(String.Format("{0} textLists loaded from {1}.", lists.Count, eventFilePath));
            textLists.Add(eventFilePath, lists);
        }
    }
}
