using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System.IO;


namespace AowVN.MVVM
{
    internal class ShortcutFile
    {
        public static void ShortcutFileA(string targetFileLocation)
        {

            string shortcutLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SANABI" + ".lnk");
            WshShell shell = new WshShell();    
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            Icon icon = Icon.ExtractAssociatedIcon(targetFileLocation);
            using (MemoryStream stream = new MemoryStream())
            {
                icon.Save(stream);
                byte[] iconData = stream.ToArray();
                System.IO.File.WriteAllBytes(shortcutLocation, iconData);
            }

            shortcut.Description = "Team AowVN!";
            shortcut.TargetPath = targetFileLocation;
            shortcut.Save();
        }
}
}
