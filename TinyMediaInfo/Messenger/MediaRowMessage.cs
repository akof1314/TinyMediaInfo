namespace TinyMediaInfo.Messenger;

public class MediaRowMessage
{
    public int ItemIndex { get; set; }

    public bool IsSuccess { get; set; }

    public MediaRowMessage(int itemIndex, bool isSuccess)
    {
        ItemIndex = itemIndex;
        IsSuccess = isSuccess;
    }
}