using EventMangler.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace EventMangler
{

    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        ImageLibrary imageLib;

        public HomePage()
        {
            InitializeComponent();
            imageLib = new ImageLibrary(EventMangler.Properties.Resources.default_imagelist);
            foreach (var imageList in imageLib.getImageLists())
            {
                // Create label with name of imageList
                Label imageListLabel = new Label();
                imageListLabel.Content = imageList.Key;

                // Add the label to the vertical stack
                ImageListStack.Children.Add(imageListLabel);

                // Create horizontal stack panel to place images in
                StackPanel listPanel = new StackPanel();
                listPanel.Background = new SolidColorBrush(Colors.Transparent);
                listPanel.Orientation = Orientation.Horizontal;
                listPanel.Height = 108;
                listPanel.AllowDrop = true;
                listPanel.Drop += new DragEventHandler(
                    delegate(object sender, DragEventArgs e)
                    {
                        // Handle image drop
                        e.Handled = true;

                        // Check for files in the hovering data object.                        
                        if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                        {
                            string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                            if (fileNames != null)
                            {
                                foreach (string filename in fileNames)
                                {
                                    System.Console.WriteLine("Image dropped: " + filename);

                                    // Construct image from path
                                    FTLImage newImage = imageLib.addImageFromPath(filename);

                                    // Border & style image
                                    Border newImageBorder = getBorderedImage(newImage);

                                    // Add to stack panel
                                    listPanel.Children.Add(newImageBorder);
                                }
                            }
                        }
                    });

                // Add the horizontal stack to the vertical stack
                ImageListStack.Children.Add(listPanel);

                // Add listed images to the horizontal stack
                foreach (FTLImage image in imageList.Value) listPanel.Children.Add(getBorderedImage(image));
            };
        }

        Border getBorderedImage(FTLImage image)
        {
            // Create image source
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            if (File.Exists(Properties.Resources.mod_root_dir + "/img/" + image.path))
            {
                src.UriSource = new Uri(Properties.Resources.mod_root_dir + "/img/" + image.path, UriKind.Absolute);
            }
            else if (File.Exists(Properties.Resources.resources_root_dir + "/img/" + image.path))
            {
                src.UriSource = new Uri(Properties.Resources.resources_root_dir + "/img/" + image.path, UriKind.Absolute);
            }
            else
            {
                src.UriSource = new Uri("img/nofile.png", UriKind.Relative);
            }
            src.EndInit();

            Image img = new Image();
            img.Margin = new Thickness(5);
            img.Source = src;

            // Create & style border around image
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(1);
            border.Margin = new Thickness(10);
            border.CornerRadius = new CornerRadius(10);
            border.Background = new SolidColorBrush(Colors.Black);
            border.Child = img;
            //border.Style = Resources["imageBorderStyle"] as Style;
            return border;
        }
    }
}
