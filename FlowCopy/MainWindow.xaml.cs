using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Diagnostics;

namespace FlowCopy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public class DictionaryEntry
        {
            public string Tag { get; set; }
            public string Content { get; set; }
        }



        public MainWindow()
        {
            InitializeComponent();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            FillListBoxWithFiles(System.IO.Path.Combine(basePath, "Templates"), listBox_templates);
            FillComboBoxWithFiles(System.IO.Path.Combine(basePath, "Data"), comboBox_tags);
            loadClipboard();
        }

        public ObservableCollection<DictionaryEntry> dataItems { get; set; }

        private ObservableCollection<DictionaryEntry> ConvertDictionaryToOCollection(Dictionary<string, string> dictionary)
        {
            var list = new ObservableCollection<DictionaryEntry>();
            foreach (var pair in dictionary)
            {
                list.Add(new DictionaryEntry { Tag = pair.Key, Content = pair.Value });
            }
            return list;
        }



        private List<DictionaryEntry> ConvertDictionaryToList(Dictionary<string, string> dictionary)
        {
            var list = new List<DictionaryEntry>();
            foreach (var pair in dictionary)
            {
                list.Add(new DictionaryEntry { Tag = pair.Key, Content = pair.Value });
            }
            return list;
        }

        private Dictionary<string, string> GetDictionaryFromDataGrid(DataGrid dataGrid)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var item in dataGrid.Items)
            {
                if (item is DictionaryEntry tagContent)
                {
                    dictionary[tagContent.Tag] = tagContent.Content;
                }
            }

            return dictionary;
        }


        private void BindDictionaryToDataGridView(Dictionary<string, string> dictionary)
        {
            // Convert the dictionary to a list
            //var list = ConvertDictionaryToList(dictionary);
            // Bind the list to the DataGridView
            //TagsdataGrid.ItemsSource = list;
            TagsdataGrid.ItemsSource = ConvertDictionaryToOCollection(dictionary);
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            loadClipboard();
        }

        private void loadClipboard()
        {
            // Check if there is text on the clipboard
            if (Clipboard.ContainsText())
            {
                // Get the text from the clipboard
                string clipboardText = Clipboard.GetText();
                textBox_clipboard.Text = clipboardText;
            }
            else
            {
                textBox_clipboard.Text = "No text on clipboard.";
            }
        }
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (listBox_templates.SelectedIndex != -1)
            {
                try
                {
                    string fileName = listBox_templates.SelectedItem.ToString();
                    string fullPath = System.IO.Path.Combine(datapath("Templates"), fileName) + ".txt";

                    // Read all text from the selected file
                    string fileContent = File.ReadAllText(fullPath);

                    // Display the content in the TextBox (or textBox)
                    textBox_template.Text = fileContent;
                    applyTags();
                }
                catch (Exception ex)
                {
                    // Handle potential errors, such as file not found
                    MessageBox.Show($"Could not read file: {ex.Message}");
                }
            }
        }

        private void FillListBoxWithFiles(string directoryPath, ListBox listBoxFiles)
        {
            try
            {
                // Clear the ListBox items
                listBoxFiles.Items.Clear();

                // Get all file paths from the specified directory
                string[] files = Directory.GetFiles(directoryPath);

                // Loop through the files and add them to the ListBox
                foreach (string file in files)
                {
                    listBoxFiles.Items.Add(System.IO.Path.GetFileName(file).Split(".")[0]); // Adds only the file name to the ListBox
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., directory does not exist)
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void FillComboBoxWithFiles(string directoryPath, ComboBox comboBoxFiles)
        {
            try
            {
                // Clear the ListBox items
                comboBoxFiles.Items.Clear();

                // Get all file paths from the specified directory
                string[] files = Directory.GetFiles(directoryPath);

                // Loop through the files and add them to the ListBox
                foreach (string file in files)
                {
                    comboBoxFiles.Items.Add(System.IO.Path.GetFileName(file)); // Adds only the file name to the ListBox
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., directory does not exist)
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        
        private Dictionary<string, string> ReadFileAndFillDictionary(string filePath)
        {
            var tagContentDictionary = new Dictionary<string, string>();

            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                // Split on the first comma
                int commaIndex = line.IndexOf(',');
                if (commaIndex != -1)
                {
                    string tag = line.Substring(0, commaIndex);
                    string content = line.Substring(commaIndex + 1);
                    tagContentDictionary[tag] = content;
                }
            }

            return tagContentDictionary;
        }

        

        private void applyTags() {
            string template = textBox_template.Text;

            if (comboBox_tags.SelectedItem != null)
            {
                string fileName = comboBox_tags.SelectedItem.ToString();
                string fullPath = System.IO.Path.Combine(datapath("Data"), fileName);

                Dictionary<string, string> tagContentpairs = ReadFileAndFillDictionary(fullPath);

                BindDictionaryToDataGridView(tagContentpairs);

                // Replace each tag in the template with its content
                foreach (KeyValuePair<string, string> pair in tagContentpairs)
                {
                    template = template.Replace(pair.Key, pair.Value);
                }

                // Display the result in textBox_clipboard
                textBox_clipboard.Text = template;

                Clipboard.SetText(template);
            }
        }

        private void setclipboard(string newclipboard)
        {
            try
            {
                Clipboard.SetText(newclipboard);
            }
            catch (Exception ex)
            {
            }
        }
        private void textBox_lostFocus(object sender, EventArgs e)
        {
            setclipboard(textBox_clipboard.Text);
        }
        private void Template_LostFocus(object sender, EventArgs e)
        {
            if (listBox_templates.SelectedItem != null)
            {
                string filename = listBox_templates.SelectedItem + ".txt";
                string filePath = System.IO.Path.Combine(datapath("Templates"), filename);
                File.WriteAllText(filePath, textBox_template.Text);
                setclipboard(textBox_clipboard.Text);
                applyTags();
            }

        }

        private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            string fileName = comboBox_tags.SelectedItem.ToString();
            string fullPath = System.IO.Path.Combine(datapath("Data"), fileName);

            try
            {
                Dictionary<string, string> tagDictionary = GetDictionaryFromDataGrid(TagsdataGrid);
                SaveDictionaryAsCsv(tagDictionary, fullPath);

                setclipboard(textBox_clipboard.Text);
                applyTags();
            }
            catch (Exception ex)
            {

            }
        }

        private void SaveDictionaryAsCsv(Dictionary<string, string> dictionary, string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            foreach (var pair in dictionary)
            {
                csvContent.AppendLine($"{pair.Key},{pair.Value}");
            }

            File.WriteAllText(filePath, csvContent.ToString());
        }


        private void button_tags_Click(object sender, RoutedEventArgs e)
        {
            applyTags();
        }

        
        private void comboBox_tags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_tags.SelectedItem != null)
            { 
                string fileName = comboBox_tags.SelectedItem.ToString();
                string fullPath = System.IO.Path.Combine(datapath("Data"), fileName);
                Dictionary<string, string> tagContentpairs = ReadFileAndFillDictionary(fullPath);

                BindDictionaryToDataGridView(tagContentpairs);
            }
            applyTags();
        }
        

        private string datapath(string relativeDirectoryPath) {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(basePath, relativeDirectoryPath);
            return filePath;
        }


        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            string filename = textBox_new_template.Text + ".txt";
            string basepath = datapath("Templates");
            string filePath = System.IO.Path.Combine(basepath, filename);
            File.WriteAllText(filePath, string.Empty);
            refreshTemplates();

        }

        private void refreshTemplates() {
            listBox_templates.Items.Clear();
            FillListBoxWithFiles(datapath("Templates"), listBox_templates);
        }

        private void refreshDataBox()
        {
            comboBox_tags.Items.Clear();
            FillComboBoxWithFiles(datapath("Data"), comboBox_tags);
        }

        private void button_remove_template_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_templates.SelectedIndex != -1)
            {
                string fileName = listBox_templates.SelectedItem.ToString();
                string filePath = System.IO.Path.Combine(datapath("Templates"), fileName) + ".txt";

                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // Delete the file
                    File.Delete(filePath);
                    Console.WriteLine("File deleted successfully.");
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }
                refreshTemplates();
            }
        }

        private void new_data_button_Click(object sender, RoutedEventArgs e)
        {
            string filename = textBox_new_data.Text + ".csv";
            string basepath = datapath("Data");
            string filePath = System.IO.Path.Combine(basepath, filename);

            if (comboBox_tags.SelectedIndex != -1)
            {
                string oldfileName = comboBox_tags.SelectedItem.ToString();
                string fullPath = System.IO.Path.Combine(basepath, oldfileName);
                Dictionary<string, string> tagContentpairs = ReadFileAndFillDictionary(fullPath);
                string tagsonlyfile = "";

                foreach (var tagcontent in tagContentpairs) { 
                    tagsonlyfile += tagcontent.Key.ToString();
                    tagsonlyfile += ",\n";
                }
                File.WriteAllText(filePath, tagsonlyfile);

                //BindDictionaryToDataGridView(tagContentpairs);
                //comboBox_tags.SelectedItem = filename;

            }
            else
            {
                File.WriteAllText(filePath, string.Empty);

            }

            refreshDataBox();
            applyTags();
        }

        private void TagsdataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagsdataGrid.SelectedItem != null)
            {
                var selectedRow = TagsdataGrid.SelectedItem as DictionaryEntry; // Replace MyRowType with the actual type of your data item
                if (selectedRow != null)
                {
                    var tagValue = selectedRow.Tag;
                    if (tagValue != null) { 
                        textBox_clipboard.Text = tagValue.ToString();
                        setclipboard(tagValue.ToString());
                    }
                }
            }
        }

        private void button_csv_open_Click(object sender, RoutedEventArgs e)
        {
            string fileName = comboBox_tags.SelectedItem.ToString();
            string fullPath = System.IO.Path.Combine(datapath("Data"), fileName);

            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = fullPath,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

        }
    }
}