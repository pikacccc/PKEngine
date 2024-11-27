using PKEngineEditor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKEngineEditor.GameProject
{
    class NewProjectViewModel : ViewModelBase
    {
        private string _name = "NewProject";
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\PKProject\";
        public string Path
        {
            get => _path;
            set
            {
                if (value != _path)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }
    }
}
