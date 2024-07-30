using DragonEngineLibrary.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class NopPatch
    {
        private IntPtr m_address;
        private byte[] m_origMem;


        private bool m_enabled = false;

        public NopPatch(IntPtr origCallAddress)
        {
            m_address = origCallAddress;
        }

        public void Enable(int length)
        {
            if (m_enabled)
                return;

            if(m_origMem == null)
            {
                m_origMem = new byte[length];
                Marshal.Copy(m_address, m_origMem, 0, length);
            }

            CPP.NopMemory(m_address, (uint)length);

            m_enabled = true;
        }

        public void Disable()
        {
            if (!m_enabled)
                return;

            CPP.PatchMemory(m_address, m_origMem);

            m_enabled = false;
        }
    }
}
