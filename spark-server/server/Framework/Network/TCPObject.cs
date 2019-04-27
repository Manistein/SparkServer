// author:manistein
// since: 2019.04.25
// desc:  Add base class for TCPServer and TCPClient class

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Network
{
    class TCPObject
    {
        protected int m_objectId = 0;
        protected int m_opaque = 0;     // service address

        public void SetObjectId(int objectId)
        {
            m_objectId = objectId;
        }

        public int GetObjectId()
        {
            return m_objectId;
        }

        public int GetOpaque()
        {
            return m_opaque;
        }

        public virtual Session GetSessionBy(long sessionId)
        {
            return null;
        }

        public virtual void Disconnect(long sessionId)
        {

        }
    }
}
