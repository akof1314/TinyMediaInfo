using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls.Notifications;
using ByteSizeLib;
using CommunityToolkit.Mvvm.Messaging;
using Sdcb.FFmpeg.Codecs;
using Sdcb.FFmpeg.Utils;
using Sdcb.FFmpeg.Formats;
using Sdcb.FFmpeg.Raw;
using TinyMediaInfo.Messenger;
using TinyMediaInfo.ViewModels;

namespace TinyMediaInfo.FFMpeg;

internal static class FFmpegHelper
{
    private static int _initState;

    public static bool IsEnabled()
    {
        IsInit();
        return _initState == 2;
    }

    private static void IsInit()
    {
        if (_initState > 0)
        {
            return;
        }

        _initState = 1;
        try
        {
            FFmpegLogger.LogWriter = (level, msg) => Debug.WriteLine(msg);
            _initState = 2;
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(
                App.GetLocalizedString("Local.ErrorTitle"),
                e.Message, NotificationType.Error, TimeSpan.Zero)));
        }
    }

    private static string ConvertSizeBytes(long size)
    {
        return ByteSize.FromBytes(size).ToBinaryString();
    }

    private static string ConvertBitRateBytes(long size)
    {
        return (size / 1000f).ToString("F0");
    }

    private static long ConvertBitRateKBytes(long size)
    {
        return (long)Math.Round(size / 1000f);
    }

    private static string ConvertTimeSpan(long microseconds)
    {
        double seconds = (double)microseconds / ffmpeg.AV_TIME_BASE;
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }

    private static string ConvertTimeSpan2(long ts)
    {
        if (ts == ffmpeg.AV_NOPTS_VALUE)
        {
            return String.Empty;
        }

        TimeSpan timeSpan = new TimeSpan((long)(decimal.Divide(ts, ffmpeg.AV_TIME_BASE) * TimeSpan.TicksPerSecond));
        return timeSpan.ToString(@"hh\:mm\:ss\.fff");
    }

    private static unsafe string GetCodecName(AVCodecID codecId)
    {
        // https://github.com/lucacicada/FFmpeg.NET/blob/main/FFmpeg.NET/AVFormatContextVisitorRes.cs

        AVCodecDescriptor* cd = ffmpeg.avcodec_descriptor_get(codecId);
        if (cd is not null)
        {
            return Marshal.PtrToStringAnsi((IntPtr)cd->name)!;
        }
        return String.Empty;
    }

    private static unsafe string GetCodecTagString(uint codecTag)
    {
        byte[] buffer = new byte[ffmpeg.AV_FOURCC_MAX_STRING_SIZE];
        fixed (byte* pBuffer = buffer)
        {
            return Marshal.PtrToStringAnsi((IntPtr)ffmpeg.av_fourcc_make_string(pBuffer, codecTag))!;
        }
    }

    private static string GetCodecTagFixString(uint codecTag)
    {
        var tag = GetCodecTagString(codecTag);
        return tag == "[0][0][0][0]" ? "----" : tag;
    }

    private static unsafe string GetDisplayAspectRatio(AVRational sar, int width, int height)
    {
        if (sar.Num == 0)
        {
            // 为了避免为空，但是又能真正播放，在ffprobe上为N/A
            //return "N/A";
            sar = new AVRational(1, 1);
        }

        // 算法：https://blog.csdn.net/ywl5320/article/details/88586054
        AVRational dar;
        ffmpeg.av_reduce(&dar.Num, &dar.Den,
            width * sar.Num,
            height * sar.Den,
            1024 * 1024);
        return dar.ToString();
    }

    public static void ParseMedia(MediaViewModel mediaView, MediaCheckboxFilterColumnsModel mediaGroup)
    {
        try
        {
            using FormatContext inFc = FormatContext.OpenInputUrl(mediaView.FilePath);
            inFc.LoadStreamInfo();

            if (inFc.InputFormat.HasValue)
            {
                mediaView.FormatName = inFc.InputFormat.Value.Name;
                mediaGroup.FormatName.AddFilterCount(mediaView.FormatName);
            }

            mediaView.DurationLong = inFc.Duration;
            mediaView.Duration = ConvertTimeSpan2(inFc.Duration);
            if (inFc.Pb != null)
            {
                mediaView.SizeLong = inFc.Pb.Size;
                mediaView.Size = ConvertSizeBytes(inFc.Pb.Size);
            }

            mediaView.BitRateLong = ConvertBitRateKBytes(inFc.BitRate);
            mediaView.BitRate = ConvertBitRateBytes(inFc.BitRate);
            mediaView.NbStreamsLong = inFc.Streams.Count;
            mediaView.NbStreams = inFc.Streams.Count.ToString();
            mediaGroup.NbStreams.AddFilterCount(mediaView.NbStreams);

            var inVideoStream = inFc.FindBestStreamOrNull(AVMediaType.Video);
            if (inVideoStream.HasValue)
            {
                if (inVideoStream.Value.Codecpar != null)
                {
                    var codecpar = inVideoStream.Value.Codecpar;
                    mediaView.VideoWidthLong = codecpar.Width;
                    mediaView.VideoWidth = codecpar.Width.ToString();
                    mediaView.VideoHeightLong = codecpar.Height;
                    mediaView.VideoHeight = codecpar.Height.ToString();
                    mediaView.VideoBitRateLong = ConvertBitRateKBytes(codecpar.BitRate);
                    mediaView.VideoBitRate = ConvertBitRateBytes(codecpar.BitRate);

                    mediaView.DisplayAspectRatio =
                        GetDisplayAspectRatio(inVideoStream.Value.SampleAspectRatio, codecpar.Width, codecpar.Height);

                    mediaView.VideoCode = GetCodecName(codecpar.CodecId);
                    mediaView.VideoCodeTag = GetCodecTagFixString(codecpar.CodecTag);

                    mediaGroup.VideoCode.AddFilterCount(mediaView.VideoCode);
                    mediaGroup.VideoCodeTag.AddFilterCount(mediaView.VideoCodeTag);
                    mediaGroup.VideoWidth.AddFilterCount(mediaView.VideoWidth);
                    mediaGroup.VideoHeight.AddFilterCount(mediaView.VideoHeight);
                    mediaGroup.DisplayAspectRatio.AddFilterCount(mediaView.DisplayAspectRatio);
                }

                mediaView.VideoNbFramesLong = inVideoStream.Value.NbFrames;
                mediaView.VideoNbFrames = inVideoStream.Value.NbFrames.ToString();
                mediaView.VideoModeFrameRate =
                    inVideoStream.Value.RFrameRate != inVideoStream.Value.AvgFrameRate ? "VFR" : "CFR";
                mediaGroup.VideoModeFrameRate.AddFilterCount(mediaView.VideoModeFrameRate);
                mediaView.VideoFrameRate = inVideoStream.Value.AvgFrameRate.ToDouble().ToString("F3");

                mediaGroup.VideoFrameRate.AddFilterCount(mediaView.VideoFrameRate);
            }

            var inAudioStream = inFc.FindBestStreamOrNull(AVMediaType.Audio);
            if (inAudioStream.HasValue)
            {
                if (inAudioStream.Value.Codecpar != null)
                {
                    var codecpar = inAudioStream.Value.Codecpar;
                    mediaView.AudioSampleRateLong = codecpar.SampleRate;
                    mediaView.AudioSampleRate = codecpar.SampleRate.ToString();
                    mediaView.AudioChannelsLong = codecpar.ChLayout.nb_channels;
                    mediaView.AudioChannels = codecpar.ChLayout.nb_channels.ToString();
                    mediaView.AudioBitRateLong = ConvertBitRateKBytes(codecpar.BitRate);
                    mediaView.AudioBitRate = ConvertBitRateBytes(codecpar.BitRate);
                    mediaView.AudioCode = GetCodecName(codecpar.CodecId);
                    mediaView.AudioCodeTag = GetCodecTagFixString(codecpar.CodecTag);

                    mediaGroup.AudioCode.AddFilterCount(mediaView.AudioCode);
                    mediaGroup.AudioCodeTag.AddFilterCount(mediaView.AudioCodeTag);
                    mediaGroup.AudioSampleRate.AddFilterCount(mediaView.AudioSampleRate);
                    mediaGroup.AudioChannels.AddFilterCount(mediaView.AudioChannels);
                }
            }
        }
        catch (Exception e)
        {
            mediaView.FormatName = e.Message;
            mediaGroup.FormatName.AddFilterCount(mediaView.FormatName);
        }
    }

    public static unsafe void ParseDetailMedia(MediaViewModel mediaView)
    {
        try
        {
            using FormatContext inFc = FormatContext.OpenInputUrl(mediaView.FilePath);
            inFc.LoadStreamInfo();

            var detail = new MediaDetailModel
            {
                Desc = mediaView.FilePath,
                Children = new List<MediaDetailModel>()
            };

            var group = new MediaDetailModel
            {
                Desc = "FORMAT",
                Children = new List<MediaDetailModel>()
            };
            detail.Children.Add(group);

            group.AddChild("nb_streams", inFc.Streams.Count);
            group.AddChild("nb_programs", inFc.Programs.Count);

            if (inFc.InputFormat.HasValue)
            {
                group.AddChild("format_name", inFc.InputFormat.Value.Name);
                group.AddChild("format_long_name", inFc.InputFormat.Value.LongName);
            }

            group.AddChildTime("start_time", inFc.StartTime, new AVRational(1, ffmpeg.AV_TIME_BASE));
            group.AddChildTime("duration", inFc.Duration, new AVRational(1, ffmpeg.AV_TIME_BASE));

            var size = inFc.Pb?.Size;
            if (size != null)
            {
                group.AddChild("size", size.Value);
            }
            else
            {
                group.AddChild("size", "N/A");
            }

            if (inFc.BitRate > 0)
            {
                group.AddChild("bit_rate", inFc.BitRate);
            }
            else
            {
                group.AddChild("bit_rate", "N/A");
            }
            group.AddChild("probe_score", inFc.ProbeScore);

            for (int i = 0; i < inFc.Streams.Count; i++)
            {
                group = new MediaDetailModel
                {
                    Desc = "STREAM " + (i),
                    Children = new List<MediaDetailModel>()
                };
                detail.Children.Add(group);

                var stream = inFc.Streams[i];
                group.AddChild("index", stream.Index);

                var par = stream.Codecpar;
                if (par == null)
                {
                    continue;
                }

                int codedWidth = 0, codedHeight = 0, bitsPerRawSample = 0;
                long rcMaxRate = 0;
                if (par.CodecId != AVCodecID.None)
                {
                    var codec = ffmpeg.avcodec_find_decoder(par.CodecId);
                    if (codec != null)
                    {
                        using CodecContext decCtx = new CodecContext(Codec.FindDecoderById(par.CodecId))
                        {
                            PktTimebase = stream.TimeBase
                        };
                        decCtx.FillParameters(par);
                        decCtx.Open();

                        codedWidth = decCtx.CodedWidth;
                        codedHeight = decCtx.CodedHeight;
                        rcMaxRate = decCtx.RcMaxRate;
                        bitsPerRawSample = decCtx.BitsPerRawSample;
                    }
                }

                AVCodecDescriptor* cd = ffmpeg.avcodec_descriptor_get(par.CodecId);
                if (cd is not null)
                {
                    group.AddChild("codec_name", Marshal.PtrToStringAnsi((IntPtr)cd->name)!);
                    group.AddChild("codec_long_name", Marshal.PtrToStringAnsi((IntPtr)cd->long_name)!);
                }
                else
                {
                    group.AddChild("codec_name", "unknown");
                    group.AddChild("codec_long_name", "unknown");
                }

                var profile = ffmpeg.avcodec_profile_name(par.CodecId, par.Profile);
                if (!string.IsNullOrEmpty(profile))
                {
                    group.AddChild("profile", profile);
                }
                else
                {
                    if (par.Profile != ffmpeg.AV_PROFILE_UNKNOWN)
                    {
                        group.AddChild("profile", par.Profile);
                    }
                    else
                    {
                        group.AddChild("profile", "unknown");
                    }
                }
                group.AddChild("codec_type", par.CodecType.ToString().ToLower());
                group.AddChild("codec_tag_string", GetCodecTagString(par.CodecTag));
                group.AddChild("codec_tag", "0x" + Convert.ToString(par.CodecTag, 16));

                switch (par.CodecType)
                {
                    case AVMediaType.Video:
                        {
                            group.AddChild("width", par.Width);
                            group.AddChild("height", par.Height);
                            {
                                group.AddChild("coded_width", codedWidth);
                                group.AddChild("coded_height", codedHeight);
                            }
                            group.AddChild("has_b_frames", par.VideoDelay);

                            var sar = ffmpeg.av_guess_sample_aspect_ratio(inFc, stream, null);
                            if (sar.Num != 0)
                            {
                                group.AddChild("sample_aspect_ratio", sar.ToString());

                                AVRational dar;
                                ffmpeg.av_reduce(&dar.Num, &dar.Den,
                                    par.Width * sar.Num,
                                    par.Height * sar.Den,
                                    1024 * 1024);
                                group.AddChild("display_aspect_ratio", dar.ToString());
                            }
                            else
                            {
                                group.AddChild("sample_aspect_ratio", "N/A");
                                group.AddChild("display_aspect_ratio", "N/A");
                            }
                            group.AddChild("pix_fmt", NameUtils.GetPixelFormatName((AVPixelFormat)par.Format));
                            group.AddChild("level", par.Level);
                        }
                        break;
                    case AVMediaType.Audio:
                        {
                            group.AddChild("sample_fmt", NameUtils.GetSampleFormatName((AVSampleFormat)par.Format));
                            group.AddChild("sample_rate", par.SampleRate);
                            group.AddChild("channels", par.ChLayout.nb_channels);
                            if (par.ChLayout.order != AVChannelOrder.Unspec)
                            {
                                group.AddChild("channel_layout", par.ChLayout.Describe());
                            }
                            else
                            {
                                group.AddChild("channel_layout", "unknown");
                            }
                            group.AddChild("bits_per_sample", ffmpeg.av_get_bits_per_sample(par.CodecId));
                            group.AddChild("initial_padding", par.InitialPadding);
                        }
                        break;
                    case AVMediaType.Subtitle:
                        {
                            if (par.Width > 0)
                            {
                                group.AddChild("width", par.Width);
                            }
                            else
                            {
                                group.AddChild("width", "N/A");
                            }
                            if (par.Height > 0)
                            {
                                group.AddChild("height", par.Height);
                            }
                            else
                            {
                                group.AddChild("height", "N/A");
                            }

                        }
                        break;
                }

                group.AddChild("r_frame_rate", stream.RFrameRate);
                group.AddChild("avg_frame_rate", stream.AvgFrameRate);
                group.AddChild("time_base", stream.TimeBase);
                group.AddChildTs("start_pts", stream.StartTime);
                group.AddChildTime("start_time", stream.StartTime, stream.TimeBase);
                group.AddChildTs("duration_ts", stream.Duration);
                group.AddChildTime("duration", stream.Duration, stream.TimeBase);

                group.AddChild("bit_rate", par.BitRate > 0 ? par.BitRate.ToString() : "N/A");
                if (rcMaxRate > 0)
                {
                    group.AddChild("max_bit_rate", rcMaxRate);
                }
                else
                {
                    group.AddChild("max_bit_rate", "N/A");
                }
                if (bitsPerRawSample > 0)
                {
                    group.AddChild("bits_per_raw_sample", bitsPerRawSample);
                }
                else
                {
                    group.AddChild("bits_per_raw_sample", "N/A");
                }

                if (stream.NbFrames > 0)
                {
                    group.AddChild("nb_frames", stream.NbFrames);
                }
                else
                {
                    group.AddChild("nb_frames", "N/A");
                }
                if (par.ExtradataSize > 0)
                {
                    group.AddChild("extradata_size", par.ExtradataSize);
                }
            }

            mediaView.DetailModel = detail;
        }
        catch (Exception e)
        {
            mediaView.FormatName = e.Message;
        }
    }
}