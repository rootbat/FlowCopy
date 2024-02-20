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
            FillListBoxWithFiles($"C:\\Users\\pauli\\source\\repos\\FlowCopy\\FlowCopy\\Templates", listBox_templates);
            FillComboBoxWithFiles($"C:\\Users\\pauli\\source\\repos\\FlowCopy\\FlowCopy\\Data", comboBox_tags);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // Check if there is text on the clipboard
            if (Clipboard.ContainsText())
            {
                // Get the text from the clipboard
                string clipboardText = Clipboard.GetText();
                textBlock.Text = clipboardText;
                if (Clipboard.ContainsAudio()) { 
                }
                if (Clipboard.ContainsImage())
                {
                }
                if (Clipboard.ContainsText())
                {
                }
                if (Clipboard.ContainsFileDropList())
                {
                }
                //Clipboard.
            }
            else
            {
                textBlock.Text = "No text on clipboard.";
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
                    string directoryPath = @"C:\Users\pauli\source\repos\FlowCopy\FlowCopy\Templates";
                    string fileName = listBox_templates.SelectedItem.ToString();
                    string fullPath = System.IO.Path.Combine(directoryPath, fileName);

                    // Read all text from the selected file
                    string fileContent = File.ReadAllText(fullPath);

                    // Display the content in the TextBox (or TextBlock)
                    textBlock_template.Text = fileContent;
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

        private void button_tags_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}