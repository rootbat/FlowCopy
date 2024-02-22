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

namespace FlowCopy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            FillListBoxWithFiles(System.IO.Path.Combine(basePath, "Templates"), listBox_templates);
            FillComboBoxWithFiles(System.IO.Path.Combine(basePath, "Data"), comboBox_tags);
            loadCliboard();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            loadCliboard();
        }

        private void loadCliboard()
        {
            // Check if there is text on the clipboard
            if (Clipboard.ContainsText())
            {
                // Get the text from the clipboard
                string clipboardText = Clipboard.GetText();
                textBlock_clipboard.Text = clipboardText;
            }
            else
            {
                textBlock_clipboard.Text = "No text on clipboard.";
            }
        }
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (listBox_templates.SelectedIndex != -1)
            {
                try
                {
                    // Assuming the ListBox contains just the file names,
                    // you need to know the directory path to construct the full file path
                    //string directoryPath = @"C:\YourDirectoryPath"; // The same directory path used earlier
                    string relativeDirectoryPath = "Templates"; // Example relative path
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string fileName = listBox_templates.SelectedItem.ToString();
                    string fullPath = System.IO.Path.Combine(basePath, relativeDirectoryPath, fileName);

                    // Read all text from the selected file
                    string fileContent = File.ReadAllText(fullPath);

                    // Display the content in the TextBox (or TextBlock)
                    textBlock_template.Text = fileContent;
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
                    listBoxFiles.Items.Add(System.IO.Path.GetFileName(file)); // Adds only the file name to the ListBox
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
            string template = textBlock_template.Text;

            string relativeDirectoryPath = "Data"; // Example relative path
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (comboBox_tags.SelectedItem != null)
            {
                string fileName = comboBox_tags.SelectedItem.ToString();
                string fullPath = System.IO.Path.Combine(basePath, relativeDirectoryPath, fileName);

                Dictionary<string, string> tagContentpairs = ReadFileAndFillDictionary(fullPath);

                // Replace each tag in the template with its content
                foreach (KeyValuePair<string, string> pair in tagContentpairs)
                {
                    template = template.Replace(pair.Key, pair.Value);
                }

                // Display the result in textBlock_clipboard
                textBlock_clipboard.Text = template;

                Clipboard.SetText(template);
            }
        }

        private void button_tags_Click(object sender, RoutedEventArgs e)
        {
            applyTags();
        }

        private void comboBox_tags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyTags();
        }
    }
}