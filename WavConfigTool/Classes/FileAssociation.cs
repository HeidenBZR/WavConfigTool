using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes
{
    public class FileAssociation
    {
        public static void AssociateIfNeeded()
        {
            AssociateIfNeeded(
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\Resources\project.ico",
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\bin\Debug\WavConfigTool.exe",
                "WavConfigTool.Project",
                ".wcp",
                "WavConfigTool Project File"
            );
            AssociateIfNeeded(
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\Resources\reclist.ico",
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\bin\Debug\WavConfigTool.exe",
                "WavConfigTool.Reclist",
                ".reclist",
                "WavConfigTool Reclist File"
            );
            AssociateIfNeeded(
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\Resources\mask.ico",
                @"D:\External\Github\HeidenBZR\WavConfigTool\WavConfigTool\bin\Debug\WavConfigTool.exe",
                "WavConfigTool.Mask",
                ".mask",
                "WavConfigTool Mask File"
            );

        }

        private static void AssociateIfNeeded(string icon, string exe, string appName, string ext, string desc)
        {
            if (!System.IO.File.Exists(icon))
                throw new Exception("icon not found");
            if (!System.IO.File.Exists(exe))
                throw new Exception("exe not found");
            if (IsAssociated(exe))
                Remove(ext, appName);
            Associate(ext, appName, desc, icon, exe);
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        ////////////// STACK OVERFLOW //////////////

        // Associate file extension with progID, description, icon and application
        private static void Associate(string extension, string progID, string description, string icon, string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            if (progID != null && progID.Length > 0)
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
                {
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("",
                                    ToShortPathName(application) + " \"%1\"");
                }
        }

        public static void Remove(string ext, string appName)
        {
            Registry.ClassesRoot.DeleteSubKeyTree(ext);
            Registry.ClassesRoot.DeleteSubKeyTree(appName);
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(string lpszLongPath,
            [Out] StringBuilder lpszShortPath, uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        ////////////// STACK OVERFLOW END //////////////
    }
}
