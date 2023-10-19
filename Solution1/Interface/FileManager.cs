using System.Collections.Generic;
using System;
using System.Windows.Controls;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace Interface
{
    public class FileManager
    {
        readonly MainWindow window;
        readonly FeedManager manager;
        Dictionary<String, Tuple<double, double>> locations;

        public FileManager(MainWindow window, FeedManager manager)
        {
            this.window = window;
            this.manager = manager;

            BuildLocationDictionary();
        }

        public void Save(string file)
        {
            XDocument doc = new();

            XElement root = new("root");

            XElement main = new("main");
            XElement topic = new("topic");

            root.Add(main);
            root.Add(topic);

            foreach (TreeViewItem item in window.mainItems)
            {
                main.Add(XmlTreeConstructor(item));
            }

            foreach (TreeViewItem item in window.topicItems)
            {
                topic.Add(XmlTreeConstructor(item));
            }

            doc.Add(root);

            doc.Save(file);
        }

        private XElement XmlTreeConstructor(TreeViewItem root)
        {
            string info = root.Header.ToString();
            XElement self;

            if (manager.GetFeed(info) != null)
            {
                Feed feed = manager.GetFeed(info);
                string url = feed.url;
                self = new XElement("Feed", info);
                self.SetAttributeValue("url", url);
            }
            else
            {
                self = new XElement("Folder", info);
            }

            foreach (TreeViewItem item in root.Items)
            {
                self.Add(XmlTreeConstructor(item));
            }

            return self;
        }

        public void Load(string file)
        {
            window.mainItems = new();
            window.topicItems = new();

            XDocument doc = XDocument.Load(file);

            foreach (XElement element in doc.Root.Elements())
            {
                if (element.Name == "main")
                {
                    foreach (XElement mainElement in element.Elements())
                    {
                        if (mainElement.Name == "Feed")
                        {
                            string name = mainElement.Value.ToString();

                            manager.CreateFeed(mainElement.Attribute("url").Value.ToString(), name);

                            TreeViewItem item = new()
                            {
                                Header = name
                            };

                            window.mainItems.Add(item);
                        }
                        else if (mainElement.Name == "Folder")
                        {
                            TreeViewItem item = new()
                            {
                                Header = mainElement.FirstNode.ToString()
                            };

                            item = TreeViewConstructor(mainElement, item);

                            window.mainItems.Add(item);
                        }
                    }
                }
                else if (element.Name == "topic")
                {
                    foreach (XElement topicElement in element.Elements())
                    {
                        TreeViewItem item = new()
                        {
                            Header = topicElement.FirstNode.ToString()
                        };

                        item = TreeViewConstructor(topicElement, item);

                        window.topicItems.Add(item);
                    }
                }
            }
        }

        private TreeViewItem TreeViewConstructor(XElement xRoot, TreeViewItem viewRoot)
        {
            foreach (XElement mainElement in xRoot.Elements())
            {
                if (mainElement.Name == "Feed")
                {
                    string name = mainElement.Value.ToString();

                    manager.CreateFeed(mainElement.Attribute("url").Value.ToString(), name);

                    TreeViewItem item = new()
                    {
                        Header = name
                    };

                    viewRoot.Items.Add(item);
                }
                else if (mainElement.Name == "Folder")
                {
                    string name = mainElement.Value.ToString();

                    TreeViewItem item = new()
                    {
                        Header = mainElement.FirstNode.ToString()
                    };

                    item = TreeViewConstructor(mainElement, item);

                    viewRoot.Items.Add(item);
                }
            }

            return viewRoot;
        }

        public void BuildLocationDictionary()
        {
            List<String[]> lines = File.ReadLines("uslocations_for_Project.csv").Select(line => line.Split(',')).ToList();
            locations = new();

            foreach (var line in lines)
            {
                if (!locations.ContainsKey(line[3]))
                {
                    if (double.TryParse(line[5], out double x) && double.TryParse(line[6], out double y))
                    {
                        locations.Add(line[3], new Tuple<double, double>(x, y));
                    }
                }
            }
        }

        public Tuple<double, double> FindCoordinates(string articleTitle)
        {
            // Split the article title into a list of words
            var words = articleTitle.Split();

            // Iterate through the list of words and check if each word is in the locations dictionary
            foreach (var word in words)
            {
                if (locations.TryGetValue(word, out var coord))
                {
                    return coord; // returns first match
                }
            }

            // No matches
            return null;
        }
    }
}
