using DragonEngineLibrary.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal class HijackedFunction
    {
        private IntPtr m_address;
        private IntPtr m_oldCall;
        private IntPtr m_newCall;

        private bool m_enabled = false;

        public HijackedFunction(IntPtr origCallAddress, IntPtr newFunc)
        {
            m_address = origCallAddress;
            m_oldCall = CPP.ReadCall(origCallAddress);
            m_newCall = newFunc;
        }

        public void Enable()
        {
            if (m_enabled)
                return;

            CPP.WriteCall(m_address, m_newCall);

            m_enabled = true;
        }

        public void Disable()
        {
            if (!m_enabled)
                return;

            CPP.WriteCall(m_address, m_oldCall);

            m_enabled = false;
        }
    }
}
