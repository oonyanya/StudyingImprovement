using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyingImprovement.Model
{
    public class Setting : BindableBase
    {
        private Setting()
        {
        }

        private void Init()
        {
            _ForceDownloadMovie = Preferences.Get("ForceDownloadMovie", false);
            _IsShowTextSpeech = false;
        }

        static Setting? _current = null;
        public static Setting Current {
            get
            {
                if(_current == null)
                {
                    _current = new Setting();
                    _current.Init();
                }
                return _current;
            }
        }

        bool _ForceDownloadMovie = false;
        public bool ForceDownloadMovie
        {
            get => _ForceDownloadMovie;
            set
            {
                SetProperty(ref _ForceDownloadMovie, value);
                Preferences.Set("ForceDownloadMovie", value);
            }
        }

        bool _IsShowTextSpeech = false;
        public bool IsShowTextSpeech
        {
            get => _IsShowTextSpeech;
            set
            {
                SetProperty(ref _IsShowTextSpeech, value);
            }
        }
    }
}
