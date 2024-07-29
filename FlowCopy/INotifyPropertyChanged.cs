using System.ComponentModel;

public class DictionaryEntry : INotifyPropertyChanged
{
    private string tag;
    private string content;

    public string Tag
    {
        get => tag;
        set
        {
            if (tag != value)
            {
                tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }
    }

    public string Content
    {
        get => content;
        set
        {
            if (content != value)
            {
                content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
