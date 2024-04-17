using Sdcb.FFmpeg.Raw;
using System.Collections.Generic;

namespace TinyMediaInfo.ViewModels;

public class MediaDetailModel
{
    public string? Desc { get; set; }

    public List<MediaDetailModel>? Children { get; set; }

    public void AddChild(string childName, string childValue)
    {
        Children?.Add(new MediaDetailModel
        {
            Desc = $"{childName}={childValue}"
        });
    }

    public void AddChild(string childName, int childValue)
    {
        AddChild(childName, childValue.ToString());
    }

    public void AddChild(string childName, long childValue)
    {
        AddChild(childName, childValue.ToString());
    }

    public void AddChild(string childName, double childValue)
    {
        AddChild(childName, childValue.ToString("F6"));
    }

    public void AddChild(string childName, AVRational childValue)
    {
        AddChild(childName, childValue.ToString());
    }

    public void AddChildTs(string childName, long ts, bool isDuration = false)
    {
        if ((!isDuration && ts == ffmpeg.AV_NOPTS_VALUE) || (isDuration && ts == 0))
        {
            AddChild(childName, "N/A");
        }
        else
        {
            AddChild(childName, ts);
        }
    }

    public void AddChildTime(string childName, long ts, AVRational timeBase, bool isDuration = false)
    {
        if ((!isDuration && ts == ffmpeg.AV_NOPTS_VALUE) || (isDuration && ts == 0))
        {
            AddChild(childName, "N/A");
        }
        else
        {
            AddChild(childName, ts * timeBase.ToDouble());
        }
    }
}