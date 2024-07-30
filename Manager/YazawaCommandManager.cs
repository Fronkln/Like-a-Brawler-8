using LikeABrawler2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElvisCommand;

namespace LikeABrawler2
{
    internal static class YazawaCommandManager
    {
        private static Dictionary<string, YFC> m_loadedYFC = new Dictionary<string, YFC>();
        private static Dictionary<string, EHC> m_loadedYHC = new Dictionary<string, EHC>();

        public static YFC GetYFCByName(string name)
        {
            if (!m_loadedYFC.ContainsKey(name))
                return null;

            return m_loadedYFC[name];
        }

        public static EHC GetYHCByName(string name)
        {
            if (!m_loadedYHC.ContainsKey(name))
                return null;

            return m_loadedYHC[name];
        }

        public static EHC LoadYHC(string name)
        {
            string path = Path.Combine(Mod.ModPath, "battle", "ehc", name);
            EHC ehc = EHC.Read(path);

#if DEBUG
            if (ehc == null)
                Mod.MessageBox((IntPtr)0, $"Error reading EHC at {path}\n\nMissing/invalid file", "EHC Error", 0x00000010);
#endif

            m_loadedYHC[Path.GetFileNameWithoutExtension(path)] = ehc;

            return ehc;
        }

        public static YFC LoadYFC(string name)
        {
            string path = Path.Combine(Mod.ModPath, "battle", "yfc", name);
            YFC yfc = YFC.Read(path);

#if DEBUG
            if (yfc == null)
                Mod.MessageBox((IntPtr)0, $"Error reading YFC at {path}\n\nMissing/invalid file", "YFC Error", 0x00000010);
#endif

            m_loadedYFC[Path.GetFileNameWithoutExtension(path)] = yfc;

            return yfc;
        }
    }
}
