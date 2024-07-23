using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FlowCopy
{
    public partial class MainWindow : Window
    {
        public class DictionaryEntry
        {
            public string Tag { get; set; }
            public string Content { get; set; }
        }

        private Dictionary<string, string> lastDictionary;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            FillListBoxWithFiles(Path.Combine(basePath, "Templates"), listBox_templates1);
            FillListBoxWithFiles(Path.Combine(basePath, "Data"), listBox_versions);
            LoadClipboard();
            lastDictionary = null;
        }

        public ObservableCollection<DictionaryEntry> DataItems { get; set; }

        private ObservableCollection<DictionaryEntry> ConvertDictionaryToObservableCollection(Dictionary<string, string> dictionary)
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
                if (item is DictionaryEntry entry)
                {
                    dictionary[entry.Tag] = entry.Content;
                }
            }
            return dictionary;
        }

        private void BindDictionaryToDataGridView(Dictionary<string, string> dictionary)
        {
            TagsdataGrid1.ItemsSource = ConvertDictionaryToList(dictionary);
            lastDictionary = dictionary;
        }

        private void LoadClipboard()
        {
            textBox_clipboard1.Text = Clipboard.ContainsText() ? Clipboard.GetText() : "No text on clipboard.";
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_templates1.SelectedIndex != -1)
            {
                LoadTemplate(listBox_templates1.SelectedItem.ToString());
            }
        }

        private void LoadTemplate(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(DataPath("Templates"), fileName) + ".txt";
                string fileContent = File.ReadAllText(fullPath);
                textBox_template1.Text = fileContent;
                ApplyTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not read file: {ex.Message}");
            }
        }

        private void FillListBoxWithFiles(string directoryPath, ListBox listBoxFiles)
        {
            try
            {
                listBoxFiles.Items.Clear();
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    listBoxFiles.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private Dictionary<string, string> ReadFileAndFillDictionary(string filePath)
        {
            var dictionary = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                int commaIndex = line.IndexOf(',');
                if (commaIndex != -1)
                {
                    string tag = line.Substring(0, commaIndex);
                    string content = line.Substring(commaIndex + 1);
                    dictionary[tag] = content;
                }
            }
            return dictionary;
        }

        private void ApplyTags()
        {
            string template = textBox_template1.Text;
            if (listBox_templates1.SelectedItem != null)
            {
                string fileName = listBox_templates1.SelectedItem.ToString();
                string fullPath = Path.Combine(DataPath("Templates"), fileName) + ".txt";
                Dictionary<string, string> tagContentPairs = ReadFileAndFillDictionary(fullPath);
                BindDictionaryToDataGridView(tagContentPairs);

                foreach (KeyValuePair<string, string> pair in tagContentPairs)
                {
                    template = template.Replace(pair.Key, pair.Value);
                }

                textBox_clipboard1.Text = template;
                Clipboard.SetText(template);
            }
        }

        private void SetClipboard(string newClipboard)
        {
            try
            {
                Clipboard.SetText(newClipboard);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SetClipboard(textBox_clipboard1.Text);
        }

        private void Template_LostFocus(object sender, RoutedEventArgs e)
        {
            if (listBox_templates1.SelectedItem != null)
            {
                string fileName = listBox_templates1.SelectedItem + ".txt";
                string filePath = Path.Combine(DataPath("Templates"), fileName);
                File.WriteAllText(filePath, textBox_template1.Text);
                SetClipboard(textBox_clipboard1.Text);
                ApplyTags();
            }
        }

        private void SaveDataGridToCsv(string fullPath)
        {
            try
            {
                Dictionary<string, string> tagDictionary = GetDictionaryFromDataGrid(TagsdataGrid1);
                SaveDictionaryAsCsv(tagDictionary, fullPath);
                SetClipboard(textBox_clipboard1.Text);
                ApplyTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private bool DataChanged()
        {
            Dictionary<string, string> tagDictionary = GetDictionaryFromDataGrid(TagsdataGrid1);
            foreach (var pair in tagDictionary)
            {
                if (!lastDictionary.ContainsKey(pair.Key) || lastDictionary[pair.Key] != pair.Value)
                {
                    return true;
                }
            }
            return false;
        }

        private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataChanged())
            {
                string fileName = listBox_versions.SelectedItem.ToString();
                string fullPath = Path.Combine(DataPath("Data"), fileName);
                SaveDataGridToCsv(fullPath);
            }
        }

        private void SaveDictionaryAsCsv(Dictionary<string, string> dictionary, string filePath)
        {
            lastDictionary = dictionary;
            StringBuilder csvContent = new StringBuilder();
            foreach (var pair in dictionary)
            {
                csvContent.AppendLine($"{pair.Key},{pair.Value}");
            }
            File.WriteAllText(filePath, csvContent.ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_new_template1.Text))
            {
                string fileName = textBox_new_template1.Text + ".txt";
                string filePath = Path.Combine(DataPath("Templates"), fileName);
                File.WriteAllText(filePath, string.Empty);
                RefreshTemplates();
            }
        }

        private void RefreshTemplates()
        {
            listBox_templates1.Items.Clear();
            FillListBoxWithFiles(DataPath("Templates"), listBox_templates1);
        }

        private void RefreshDataBox()
        {
            listBox_versions.Items.Clear();
            FillListBoxWithFiles(DataPath("Data"), listBox_versions);
        }

        private void Button_Remove_Template_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_templates1.SelectedIndex != -1)
            {
                string fileName = listBox_templates1.SelectedItem.ToString();
                string filePath = Path.Combine(DataPath("Templates"), fileName) + ".txt";

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                RefreshTemplates();
            }
        }

        private void New_Data_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_new_data1.Text))
            {
                string fileName = textBox_new_data1.Text + ".csv";
                string basePath = DataPath("Data");
                string filePath = Path.Combine(basePath, fileName);

                if (listBox_versions.SelectedIndex != -1)
                {
                    string oldFileName = listBox_versions.SelectedItem.ToString();
                    string oldFilePath = Path.Combine(basePath, oldFileName);
                    Dictionary<string, string> tagContentPairs = ReadFileAndFillDictionary(oldFilePath);
                    string tagsOnlyFile = string.Join(",\n", tagContentPairs.Keys);
                    File.WriteAllText(filePath, tagsOnlyFile);
                    BindDictionaryToDataGridView(tagContentPairs);
                    listBox_versions.SelectedItem = fileName;
                }
                else
                {
                    File.WriteAllText(filePath, string.Empty);
                }

                RefreshDataBox();
                ApplyTags();
            }
        }

        private void ListBox_Versions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_versions.SelectedIndex != -1)
            {
                string fileName = listBox_versions.SelectedItem.ToString();
                string fullPath = Path.Combine(DataPath("Data"), fileName) + ".csv";

                try
                {
                    BindDictionaryToDataGridView(ReadFileAndFillDictionary(fullPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read file: {ex.Message}");
                }
            }
        }

        private void Button_Csv_Open_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_versions.SelectedIndex != -1)
            {
                string fileName = listBox_versions.SelectedItem.ToString();
                string fullPath = Path.Combine(DataPath("Data"), fileName);

                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private string DataPath(string relativeDirectoryPath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeDirectoryPath);
        }
    }
}
