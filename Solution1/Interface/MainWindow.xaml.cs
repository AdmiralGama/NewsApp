using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Microsoft.Maps.MapControl.WPF;
using System.Threading.Tasks;
using System.Threading;

namespace Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly FeedManager manager;
        readonly FileManager fileManager;

        public List<TreeViewItem> mainItems;
        public List<TreeViewItem> topicItems;

        Feed? currFeed;
        List<Article> articles;
        readonly DataTable articleData;

        string file = "";

        public MainWindow()
        {
            InitializeComponent();

            // Instantiates stuff
            manager = new();
            fileManager = new(this, manager);
            articleData = new();
            articles = new();

            // Binds data grids to articleData
            dataGrid.DataContext = articleData;
            topicDataGrid.DataContext = articleData;

            // Creates the columns
            articleData.Columns.Add("Title", typeof(string));
            articleData.Columns.Add("Date", typeof(string));

            mainItems = new();
            topicItems = new();

            CommandSetup();

            RunPeriodicAsync(manager.CheckUpdate, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        private static async Task RunPeriodicAsync(Action onTick, TimeSpan dueTime, TimeSpan interval, CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick?.Invoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        private void SetUpdate(object sender, RoutedEventArgs e)
        {
            // Creates the popup window
            UpdatePopup popup = new()
            {
                parent = this
            };

            popup.Show();
        }

        public void UpdateReturn(UpdatePopup popup, string updatePeriod)
        {
            popup.Hide();

            TreeViewItem selected = (TreeViewItem) tree.SelectedItem;

            if (selected != null && Double.TryParse(updatePeriod, out double period)) {
                Feed? feed = manager.GetFeed(selected.Header.ToString());

                // Removes the feed from the manager's list
                if (feed != null)
                {
                    feed.interval = period * 60000; // Converts minutes to milliseconds
                }
            }
        }

        private void AddFeed(object sender, RoutedEventArgs e)
        {
            // Creates the popup window
            InputPopup popup = new()
            {
                parent = this
            };

            popup.Show();
        }

        /// <summary>
        /// Runs once the user clicks Create in the popup window
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="name"></param>
        /// <param name="url"></param>
        public void AddFeedReturn(InputPopup popup, string name, string url)
        {
            popup.Hide();

            TreeViewItem item = new();

            // Checks if it's a feed or a folder
            if (url != "")
            {
                currFeed = manager.CreateFeed(url, name);
                if (currFeed.name == "")
                {
                    return;
                }
                item.Header = currFeed.name;
            }
            else
            {
                item.Header = name;
            }

            // Adds the new item to the treeview
            TreeViewItem selected = (TreeViewItem)tree.SelectedItem;

            // Checks if it should be a child or root node
            if (selected != null)
            {
                selected.Items.Add(item);
            }
            else
            {
                // Adds the item to the tree
                tree.Items.Add(item);

                // Adds it to the respective lists
                if (mainTab.IsSelected)
                {
                    mainItems.Add(item);
                }
                else if (topicTab.IsSelected)
                {
                    topicItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Deletes a feed/folder/topic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFeed(object sender, RoutedEventArgs e)
        {
            TreeViewItem selected = (TreeViewItem)tree.SelectedItem;

            // If a tree item is selected, remove it
            if (selected != null)
            {
                Feed? feed = manager.GetFeed(selected.Header.ToString());

                // Removes the feed from the manager's list
                if (feed != null)
                {
                    manager.feeds.Remove(feed);
                }

                // Handles if it's a root node or a child
                if (tree.Items.Contains(selected))
                {
                    tree.Items.Remove(selected);
                }
                else
                {
                    TreeViewItem parent = (TreeViewItem)selected.Parent;
                    parent.Items.Remove(selected);
                }

                // Clears the article and browser display
                articleData.Rows.Clear();
                articles.Clear();
                mainBrowser.Source = new Uri("about:blank");

                ResetList();
            }
        }

        private void ResetList()
        {
            // Re-creates item lists from tree views
            if (mainTab.IsSelected)
            {
                mainItems = new();

                foreach (TreeViewItem item in tree.Items)
                {
                    mainItems.Add(item);
                }
            }
            else if (topicTab.IsSelected)
            {
                topicItems = new();

                foreach (TreeViewItem item in tree.Items)
                {
                    topicItems.Add(item);
                }
            }
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (file != "")
            {
                fileManager.Save(file);
            }
            else
            {
                SaveAsButton_Click(sender, e);
            }
        }

        // Writes the tree to an XML file
        private void SaveAsButton_Click(object? sender, RoutedEventArgs? e)
        {
            SaveFileDialog dialog = new();

            if (dialog.ShowDialog() == true)
            {
                file = dialog.FileName;
            }

            if (file == "")
            {
                return;
            }

            fileManager.Save(file);
        }

        private void LoadButton_Click(object? sender, RoutedEventArgs? e)
        {
            OpenFileDialog dialog = new();

            if (dialog.ShowDialog() == true)
            {
                file = dialog.FileName;
            }

            if (file == "")
            {
                return;
            }

            fileManager.Load(file);

            tree.Items.Clear();

            // Builds trees
            if (mainTab.IsSelected)
            {
                foreach (TreeViewItem item in mainItems)
                {
                    tree.Items.Add(item);
                }
            }
            else if (topicTab.IsSelected)
            {
                foreach (TreeViewItem item in topicItems)
                {
                    tree.Items.Add(item);
                }
            }
        }

        // When the user changed the item they have selected
        private void Tree_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem selected = (TreeViewItem)tree.SelectedItem;

            if (mainTab.IsSelected)
            {
                // If nothing selected, clear the display, otherwise try to get a feed
                if (selected != null)
                {
                    currFeed = manager.GetFeed(selected.Header.ToString());
                }
                else
                {
                    articleData.Rows.Clear();
                    articles.Clear();
                    mainBrowser.Source = new Uri("about:blank");
                }

                // If the item is a feed, display it
                if (currFeed != null)
                {
                    articles = currFeed.articles;
                    UpdateGrid();

                    // Displays first article in list
                    if (articles.Count != 0)
                    {
                        mainBrowser.Source = new Uri(articles[0].url);
                    }
                    else
                    {
                        mainBrowser.Source = new Uri("about:blank");
                    }

                    SuppressBrowser();
                }
                // Otherwise clear the display
                else
                {
                    articleData.Rows.Clear();
                    articles.Clear();
                    mainBrowser.Source = new Uri("about:blank");
                }
            }
            else if (topicTab.IsSelected && selected != null)
            {
                // Gets the topic and all its children recursively
                List<string> topicList = TopicListBuilder(selected);
                articles.Clear();

                // Gets a list of articles that match the topics
                foreach (string topic in topicList)
                {
                    foreach (Article article in manager.GetArticlesFromTopic(topic))
                    {
                        if (!articles.Contains(article)) {
                            articles.Add(article);
                            Title = "add";
                        }
                    }
                }

                // Adds the articles to the data grid
                UpdateGrid();

                // Displays the first in the list
                if (articles.Count != 0)
                {
                    topicBrowser.Source = new Uri(articles[0].url);
                }
                else
                {
                    topicBrowser.Source = new Uri("about:blank");
                }

                SuppressBrowser();
            }
        }

        private List<string> TopicListBuilder(TreeViewItem item)
        {
            // Creates a list with the current item
            List<string> result = new()
            {
                item.Header.ToString()
            };

            // Adds children recursively
            foreach (TreeViewItem child in item.Items)
            {
                foreach (string str in TopicListBuilder(child))
                {
                    result.Add(str);
                }
            }

            return result;
        }

        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                tree.Items.Clear();
                articles.Clear();
                UpdateGrid();

                // Builds tree view
                if (mainTab.IsSelected)
                {
                    foreach (TreeViewItem item in mainItems)
                    {
                        tree.Items.Add(item);
                    }
                }
                // Builds tree view
                else if (topicTab.IsSelected)
                {
                    foreach (TreeViewItem item in topicItems)
                    {
                        tree.Items.Add(item);
                    }
                }
                // Sets up map
                else if (mapTab.IsSelected)
                {
                    mapControl.Children.Clear();
                    PopulateMap();
                }
            }
        }

        private void PopulateMap()
        {
            articles.Clear();

            // Gets a list of all articles
            foreach (Feed feed in manager.feeds)
            {
                foreach (Article article in feed.articles)
                {
                    articles.Add(article);
                }
            }

            // Adds all of them to the map
            foreach (Article article in articles)
            {
                Tuple<double, double> coord = fileManager.FindCoordinates(article.title);

                if (coord != null)
                {
                    Pushpin pin = new();
                    pin.Location = new Location(coord.Item1, coord.Item2); // Sets coordinates
                    pin.ToolTip = article.title; // Sets title
                    pin.MouseDoubleClick += OpenArticle; // Sets double-click event to open selected article
                    mapControl.Children.Add(pin); // Adds it to the map
                }
            }
        }

        private void OpenArticle(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            // Gets the title from the clicked pin
            Pushpin pin = (Pushpin) sender;
            String title = pin.ToolTip.ToString();

            Article selected = articles[0];

            // Gets the article by the title
            foreach (Article test in articles)
            {
                if (test.title == title)
                {
                    selected = test;
                    break;
                }
            }

            // Displays the article
            mainBrowser.Source = new Uri(selected.url);
            SuppressBrowser();
            //Switch to mainTab
            tabControl.SelectedIndex = 0;
        }

        // Code for selecting articles
        private void Grid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                // Updates main datagrid
                if (dataGrid.CurrentCell.Column != null)
                {
                    int index = dataGrid.Items.IndexOf(dataGrid.SelectedItem); // Gets index of selected cell
                    mainBrowser.Source = new Uri(articles[index].url); // Displays article with that index
                    SuppressBrowser();
                }
                // Updates topic datagrid
                if (topicDataGrid.CurrentCell.Column != null)
                {
                    int index = topicDataGrid.Items.IndexOf(topicDataGrid.SelectedItem); // Gets index of selected cell
                    topicBrowser.Source = new Uri(articles[index].url); // Displays article with that index
                    SuppressBrowser();
                }
            }
            catch { }
        }

        private void UpdateGrid()
        {
            // Clears old data
            articleData.Rows.Clear();

            // Adds articles to the display one-by-one
            foreach (Article article in articles)
            {
                DataRow row = articleData.NewRow();
                row["Title"] = article.title;
                row["Date"] = article.date; // Placeholder
                articleData.Rows.Add(row);
            }
        }

        // Found online, stops the browser from popping up a billion errors cuz javascript doesn't work well in WPF
        private void SuppressBrowser()
        {
            // Suppresses main browser
            dynamic? activeX = mainBrowser.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, mainBrowser, Array.Empty<object>());

            if (activeX != null)
                activeX.Silent = true;

            // Suppresses topic browser
            activeX = topicBrowser.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, topicBrowser, Array.Empty<object>());

            if (activeX != null)
                activeX.Silent = true;
        }

        private void CommandSetup()
        {
            saveBinding.Command = SaveCommand;
            saveCommand = new RelayCommand<object>(SaveFunc);

            saveAsBinding.Command = SaveAsCommand;
            saveAsCommand = new RelayCommand<object>(SaveAsFunc);

            loadBinding.Command = LoadCommand;
            loadCommand = new RelayCommand<object>(LoadFunc);
        }

        // Keybinds
        private ICommand? saveCommand;
        public ICommand? SaveCommand { get { return saveCommand; } }
        private void SaveFunc(object o) { SaveButton_Click(null, null); }

        private ICommand? saveAsCommand;
        public ICommand? SaveAsCommand { get { return saveAsCommand; } }
        private void SaveAsFunc(object o) { SaveAsButton_Click(null, null); }

        private ICommand? loadCommand;
        public ICommand? LoadCommand { get { return loadCommand; } }
        private void LoadFunc(object o) { LoadButton_Click(null, null); }
    }
}

