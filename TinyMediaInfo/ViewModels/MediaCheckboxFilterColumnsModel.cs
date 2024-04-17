namespace TinyMediaInfo.ViewModels
{
    public class MediaCheckboxFilterColumnsModel
    {
        public MediaCheckboxFilterColumnModel FormatName {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel NbStreams {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel VideoCode {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel VideoCodeTag {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel VideoWidth {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel VideoHeight {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel DisplayAspectRatio {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel VideoFrameRate {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel AudioCode {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel AudioCodeTag {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel AudioSampleRate {get;} = new MediaCheckboxFilterColumnModel();

        public MediaCheckboxFilterColumnModel AudioChannels {get;} = new MediaCheckboxFilterColumnModel();
    }
}