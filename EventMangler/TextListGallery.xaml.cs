using EventMangler.model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace EventMangler
{
    /// <summary>
    /// Interaction logic for ListPage.xaml
    /// </summary>
    public partial class TextListGallery : Page
    {
        List<TextList> textLists;

        public TextListGallery()
        {
            InitializeComponent();
            this.textLists = new List<TextList>();

            // Load all of the textLists in the mod directory
            System.Console.WriteLine("Loading files from " + Path.Combine(Properties.Resources.mod_root_dir, "data/"));
            foreach (string eventFile in Directory.EnumerateFiles(Path.Combine(Properties.Resources.mod_root_dir, "data/"), "*.xml"))
            {
                Console.WriteLine("Trying to parse XML in file {0}", eventFile);
                string xmlString = File.ReadAllText(eventFile);

                // We have to artificially add a root node
                xmlString = String.Format("<textLists>{0}</textLists>", xmlString);
                // Also, remove all comments (since some contain invalid characters)
                Regex rComments = new Regex("<!--.*?-->", RegexOptions.Singleline);
                xmlString = rComments.Replace(xmlString, "");

                try
                {
                    XDocument events_textList = XDocument.Parse(xmlString);
                    var textListQuery =
                    from
                        tl in events_textList.Descendants("textList")
                    select tl;
                    foreach (var textList in textListQuery)
                    {
                        //System.Console.WriteLine(String.Format("Adding text list {0}:{1}", eventFile, textList));
                        textLists.Add(new TextList(eventFile, textList));
                    }
                }
                catch (XmlException e)
                {                    
                    Console.WriteLine(e);
                    Console.WriteLine(xmlString);
                }
            }

            foreach (TextList textList in textLists)
            {
                Label textListLabel = new Label();
                textListLabel.Content = String.Format("{0}: {1}", Path.GetFileNameWithoutExtension(textList.EventFile), textList.Name);
                TextListStack.Children.Add(textListLabel);
            }
        }
    }
}
