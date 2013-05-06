using EventMangler.model;
using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EventMangler
{

    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class ImageGallery : Page
    {
        ImageLibrary imageLib;

        public ImageGallery()
        {
            InitializeComponent();
            this.imageLib = new ImageLibrary(Path.Combine(Properties.Resources.mod_root_dir, "data/"));
            foreach (var imageList in imageLib.Lists.Values.SelectMany(x => x))
            {
                // Create label with name of imageList
                Label imageListLabel = new Label();
                imageListLabel.Content = imageList.EventFile;

                // Add the label to the vertical stack
                ImageListStack.Children.Add(imageListLabel);

                // Create horizontal stack panel to place images in
                StackPanel listPanel = new StackPanel();
                // listPanel.Name = imageList.EventFile;
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
                                    FTLImage newImage = imageLib.addImageFromPath(filename, imageList.Name);

                                    // Border & style image
                                    Border newImageBorder = getBorderedImage(newImage);

                                    // Add click handlers to remove image to border
                                    Object downOn = null;
                                    newImageBorder.MouseRightButtonDown += new MouseButtonEventHandler(
                                    delegate(object mouseEventSender, MouseButtonEventArgs e1)
                                    {
                                        downOn = mouseEventSender;
                                    });
                                    newImageBorder.MouseRightButtonUp += new MouseButtonEventHandler(
                                    delegate(object mouseEventSender, MouseButtonEventArgs e1)
                                    {
                                        if (downOn == mouseEventSender)
                                        {
                                            imageLib.removeFTLImage(newImage, imageList.Name);
                                            listPanel.Children.Remove(newImageBorder);
                                        }
                                        downOn = null;
                                    });

                                    // Add to stack panel
                                    listPanel.Children.Add(newImageBorder);
                                }
                            }
                        }
                    });

                // Add the horizontal stack to the vertical stack
                ImageListStack.Children.Add(listPanel);

                // Add listed images to the horizontal stack
                foreach (FTLImage image in imageList.Items)
                {
                    Border border = getBorderedImage(image);

                    // Add click handlers to remove image to border
                    Object downOn = null;
                    border.MouseRightButtonDown += new MouseButtonEventHandler(
                    delegate(object sender, MouseButtonEventArgs e)
                    {
                        downOn = sender;
                    });
                    border.MouseRightButtonUp += new MouseButtonEventHandler(
                    delegate(object sender, MouseButtonEventArgs e)
                    {
                        if (downOn == sender)
                        {
                            listPanel.Children.Remove(border);
                            imageLib.removeFTLImage(image, imageList.Name);                            
                        }
                        downOn = null;
                    });

                    listPanel.Children.Add(border);
                }
            };
        }

        private Border getBorderedImage(FTLImage image)
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

            return border;
        }
    }
}
