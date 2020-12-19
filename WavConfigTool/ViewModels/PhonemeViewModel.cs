using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    public class PhonemeViewModel : ViewModelBase
    {
        public Phoneme Phoneme { get; private set; }

        public bool HasZone => Phoneme.HasZone;

        public PhonemeViewModel(Phoneme phoneme)
        {
            Phoneme = phoneme;
        }

        public void FireChanged()
        {
            RaisePropertyChanged(nameof(HasZone));
        }
    }
}
