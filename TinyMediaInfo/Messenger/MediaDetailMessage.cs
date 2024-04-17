namespace TinyMediaInfo.Messenger;

public class MediaDetailMessage
{
    public int ItemIndex { get; set; }

    public MediaDetailMessage(int itemIndex)
    {
        ItemIndex = itemIndex;
    }
}