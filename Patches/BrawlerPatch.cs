using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    internal abstract class BrawlerPatch
    {
        protected bool m_active = false;
        protected bool m_activeFirstTime = true;

        public void Activate()
        {
            if (m_active)
                return;

            SetActive();

            if (m_activeFirstTime)
                m_activeFirstTime = false;

            m_active = true;
        }

        public void Deactivate()
        {
            if (!m_active)
                return;

            SetInactive();
            m_active = false;
        }

        public virtual void Init()
        {

        }

        protected virtual void SetActive()
        {

        }

        protected virtual void SetInactive()
        {

        }
    }
}
