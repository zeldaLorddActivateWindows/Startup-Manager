using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StartupManager
{
    internal class StartupProgram
    {
        public BitmapSource Icon { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Command { get; set; }
        public List<string> AvailableCommands { get; set; }
    }
}
