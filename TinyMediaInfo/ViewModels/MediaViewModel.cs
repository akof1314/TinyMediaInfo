using System;
using MiniExcelLibs.Attributes;

namespace TinyMediaInfo.ViewModels
{
    public class MediaViewModel
    {
        public string FilePath {get;set;} = String.Empty;

        public string FileName {get;set;} = String.Empty;

        public string FormatName {get;set;} = String.Empty;

        public string NbStreams {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long NbStreamsLong { get; set; } = 0;

        public string Size {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long SizeLong { get; set; } = 0;

        public string Duration {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long DurationLong {get;set;} = 0;

        public string BitRate {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long BitRateLong { get; set; } = 0;

        public string VideoCode {get;set;} = String.Empty;

        public string VideoCodeTag {get;set;} = String.Empty;

        public string VideoWidth {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long VideoWidthLong { get; set; } = 0;

        public string VideoHeight {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long VideoHeightLong { get; set; } = 0;

        public string DisplayAspectRatio {get;set;} = String.Empty;

        public string VideoBitRate {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long VideoBitRateLong { get; set; } = 0;

        public string VideoFrameRate {get;set;} = String.Empty;

        public string VideoNbFrames {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long VideoNbFramesLong { get; set; } = 0;

        public string AudioCode {get;set;} = String.Empty;

        public string AudioCodeTag {get;set;} = String.Empty;

        public string AudioSampleRate {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long AudioSampleRateLong { get; set; } = 0;

        public string AudioBitRate {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long AudioBitRateLong { get; set; } = 0;

        public string AudioChannels {get;set;} = String.Empty;

        [ExcelColumn(Ignore = true)]
        public long AudioChannelsLong { get; set; } = 0;

        [ExcelColumn(Ignore = true)]
        public MediaDetailModel? DetailModel {get;set;}
    }
}