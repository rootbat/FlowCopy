using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

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
        private ObservableCollection<DictionaryEntry> dataItems;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            FillListBoxWithFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"), listBox_templates1);
            FillListBoxWithFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"), listBox_versions);
            LoadClipboard();
            dataItems = new ObservableCollection<DictionaryEntry>();
            TagsdataGrid1.ItemsSource = dataItems;
        }

        private ObservableCollection<DictionaryEntry> ConvertDictionaryToObservableCollection(Dictionary<string, string> dictionary)
        {
            var list = new ObservableCollection<DictionaryEntry>();
            foreach (var pair in dictionary)
            {
                list.Add(new DictionaryEntry { Tag = pair.Key, Content = pair.Value });
            }
            return list;
        }

        private void BindDictionaryToDataGridView(Dictionary<string, string> dictionary)
        {
            dataItems = ConvertDictionaryToObservableCollection(dictionary);
            TagsdataGrid1.ItemsSource = dataItems;
            lastDictionary = new Dictionary<string, string>(dictionary);
        }

        private Dictionary<string, string> GetDictionaryFromDataGrid(DataGrid dataGrid)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var item in dataGrid.Items)
            {
                if (item is DictionaryEntry entry)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Tag) && !string.IsNullOrWhiteSpace(entry.Content))
                    {
                        dictionary[entry.Tag] = entry.Content;
                    }
                }
            }
            return dictionary;
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
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Templates");
                string fullPath = Path.Combine(dataPath, fileName) + ".txt";
                string fileContent = File.ReadAllText(fullPath);
                textBox_template1.Text = fileContent;
                ApplyTags();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not read file: {ex.Message}");
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

        private async void ApplyTags()
        {
            string template = textBox_template1.Text;
            if (listBox_versions.SelectedItem != null)
            {
                try
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string dataPath = Path.Combine(basePath, "Data");
                    string fileName = listBox_versions.SelectedItem.ToString();
                    string fullPath = Path.Combine(dataPath, fileName) + ".csv";
                    Dictionary<string, string> tagContentPairs = ReadFileAndFillDictionary(fullPath);
                    BindDictionaryToDataGridView(tagContentPairs);
                    foreach (KeyValuePair<string, string> pair in tagContentPairs)
                    {
                        template = template.Replace(pair.Key, pair.Value);
                    }

                    if (fileName.EndsWith(".gpt"))
                    {
                        string apiKey = textBox_gpt_key.Text;
                        if (string.IsNullOrEmpty(apiKey))
                        {
                            MessageBox.Show("Please enter your GPT API key.");
                            return;
                        }

                        string response = await GetChatGPTResponse(template, apiKey);
                        textBox_clipboard1.Text = response;
                        Clipboard.SetText(response);
                    }
                    else
                    {
                        textBox_clipboard1.Text = template;
                        Clipboard.SetText(template);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read file: {ex.Message}");
                }
            }
        }

        private async Task<string> GetChatGPTResponse(string prompt, string apiKey)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "text-davinci-003",
                prompt = prompt,
                max_tokens = 150
            };

            var content = new StringContent(JObject.FromObject(requestBody).ToString(), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseString);

            return responseJson["choices"][0]["text"].ToString().Trim();
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
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Templates");
                string filePath = Path.Combine(dataPath, fileName);
                File.WriteAllText(filePath, textBox_template1.Text);
                SetClipboard(textBox_clipboard1.Text);
                ApplyTags();
            }
        }

        private void SaveDataGridToCsv(string fullPath)
        {
            try
            {
                var dictionary = new Dictionary<string, string>();
                foreach (var item in dataItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.Tag) && !string.IsNullOrWhiteSpace(item.Content))
                    {
                        dictionary[item.Tag] = item.Content;
                    }
                }
                SaveDictionaryAsCsv(dictionary, fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void SaveDictionaryAsCsv(Dictionary<string, string> dictionary, string filePath)
        {
            lastDictionary = new Dictionary<string, string>(dictionary);
            StringBuilder csvContent = new StringBuilder();
            foreach (var pair in dictionary)
            {
                csvContent.AppendLine($"{pair.Key},{pair.Value}");
            }
            File.WriteAllText(filePath, csvContent.ToString());
        }

        private void TagsdataGrid1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                CommitDataGridChanges();
            }
        }

        private void TagsdataGrid1_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                CommitDataGridChanges();
            }
        }

        private void TagsdataGrid1_CurrentCellChanged(object sender, EventArgs e)
        {
            CommitDataGridChanges();
        }

        private void CommitDataGridChanges()
        {
            if (listBox_versions.SelectedItem != null)
            {
                string fileName = listBox_versions.SelectedItem.ToString();
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Data");
                string fullPath = Path.Combine(dataPath, fileName) + ".csv";
                SaveDataGridToCsv(fullPath);
            }
        }

        private void TagsdataGrid1_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new DictionaryEntry { Tag = string.Empty, Content = string.Empty };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_new_template1.Text))
            {
                string fileName = textBox_new_template1.Text + ".txt";
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Templates");
                string filePath = Path.Combine(dataPath, fileName);
                File.WriteAllText(filePath, string.Empty);
                RefreshTemplates();
            }
        }

        private void RefreshTemplates()
        {
            listBox_templates1.Items.Clear();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dataPath = Path.Combine(basePath, "Templates");
            FillListBoxWithFiles(dataPath, listBox_templates1);
        }

        private void RefreshDataBox()
        {
            listBox_versions.Items.Clear();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dataPath = Path.Combine(basePath, "Data");
            FillListBoxWithFiles(dataPath, listBox_versions);
        }

        private void Button_Remove_Template_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_templates1.SelectedIndex != -1)
            {
                string fileName = listBox_templates1.SelectedItem.ToString();
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Templates");
                string filePath = Path.Combine(dataPath, fileName) + ".txt";

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
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Data");
                string filePath = Path.Combine(dataPath, fileName);

                if (listBox_versions.SelectedIndex != -1)
                {
                    string oldFileName = listBox_versions.SelectedItem.ToString();
                    string oldFilePath = Path.Combine(dataPath, oldFileName);
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
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Data");
                string fullPath = Path.Combine(dataPath, fileName) + ".csv";

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
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataPath = Path.Combine(basePath, "Data");
                string fullPath = Path.Combine(dataPath, fileName);

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
    }
}
