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
        TextListLibrary library;

        public TextListGallery()
        {
            InitializeComponent();
            
            System.Console.WriteLine("Loading files from " + Path.Combine(Properties.Resources.mod_root_dir, "data/"));
            this.library = new TextListLibrary(Path.Combine(Properties.Resources.mod_root_dir, "data/"));

            foreach (List<TextList> eventFile in library.TextLists.Values)
            {
                foreach (TextList textList in eventFile)
                {
                    Label textListLabel = new Label();
                    textListLabel.Content = String.Format("{0}: {1}", Path.GetFileNameWithoutExtension(textList.EventFile), textList.Name);
                    TextListStack.Children.Add(textListLabel);
                }
            }
        }
    }
}
