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
                
            }
            else
            {
                textBlock.Text = "No text on clipboard.";
            }
        }
    }
}