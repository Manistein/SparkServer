/* author: manistein
 * since : 2019.03.23
 * desc  : unpack skynet clusters's request and pack response for skynet clusters
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Framework.Utility;
using System.Diagnostics;
using SparkServer.Framework.Service;

namespace SparkServer.Framework.MessageQueue
{
    /* ----------------------------------------------------------------------------------------------------------------------
     * Skynet cluster request packet formats
     * Skynet cluster's normal packet(<= 32k) format
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * +     2 byte     +  1byte +  1byte + name len bytes +            4 bytes             +            msg size            +
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * +    packet len  +Tag:0x80+name len+    name bytes  +            session             +            msg bytes           +
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * 
     * Skynet cluster's large packet(> 32k) format
     * First FRAGMENT format
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * +     2 byte     +  1byte +  1byte + name len bytes +            4 bytes             +            4 byte              +
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * +    packet len  +Tag:0x81+name len+    name bytes  +            session             +            msg size            +
     * +----------------+--------+--------+----------------+--------------------------------+--------------------------------+
     * 
     * FRAGMENT format
     * +----------------+--------+--------------------------------+--------------------------------+
     * +     2 byte     +  1byte +            4 bytes             +            msg size            +
     * +----------------+--------+--------------------------------+--------------------------------+
     * +    packet len  +  Tag:2 +            session             +            msg buf bytes       +
     * +----------------+--------+--------------------------------+--------------------------------+
     * 
     * Last FRAGMENT format
     * +----------------+--------+--------------------------------+--------------------------------+
     * +     2 byte     +  1byte +            4 bytes             +            msg size            +
     * +----------------+--------+--------------------------------+--------------------------------+
     * +    packet len  +  Tag:3 +            session             +            msg buf bytes       +
     * +----------------+--------+--------------------------------+--------------------------------+
     * 
     * -----------------------------------------------------------------------------------------------------------------------
     * 
     * Skynet cluster response packet formats
     * Large response packet (> 32k) format, Tag: MULTI_BEGIN
     * +----------------+--------------------------------+--------+--------------------------------+
     * +     2 byte     +             4 bytes            +  1byte +            4 bytes             +
     * +----------------+--------------------------------+--------+--------------------------------+
     * +    packet len  +            session             +  Tag   +            msg size            +
     * +----------------+--------------------------------+--------+--------------------------------+
     * 
     * Normal response packet (<= 32k) format, Tag: ERROR, OK, MULTI_PART, MULTI_END
     * +----------------+--------------------------------+--------+--------------------------------+
     * +     2 byte     +             4 bytes            +  1byte +            msg size            +
     * +----------------+--------------------------------+--------+--------------------------------+
     * +    packet len  +            session             +  Tag   +            msg buf bytes       +
     * +----------------+--------------------------------+--------+--------------------------------+
     * 
     * -----------------------------------------------------------------------------------------------------------------------
     * Skynet inner message packet format
     * pack string to buffer, lua long string, (str bytes < 32 bytes)
     * +--------+--------------------------------+
     * + 1 byte +         str len bytes          +
     * +--------+--------------------------------+
     * +Len|Type+         str buf bytes          +
     * +--------+--------------------------------+
     * 
     * pack string to buffer, lua long string, (32 bytes <= str bytes < 16k)
     * +--------+----------------+--------------------------------+
     * + 1 byte +     2 bytes    +         str len bytes          +
     * +--------+----------------+--------------------------------+
     * + 2|Type +     str len    +         str buf bytes          +
     * +--------+----------------+--------------------------------+
     * 
     * pack string to buffer, lua long string, (str bytes >= 16k)
     * +--------+--------------------------------+--------------------------------+
     * + 1 byte +             4 bytes            +         str len bytes          +
     * +--------+--------------------------------+--------------------------------+
     * + 4|Type +             str len            +         str buf bytes          +
     * +--------+--------------------------------+--------------------------------+
     * 
     * ------------------------------------------------------------------------------------------------------------------------
     * pack integer to buffer. value == 0
     * +--------+
     * + 1 byte +
     * +--------+
     * + 0|Type +
     * +--------+
     * 
     * pack integer to buffer. value != (int32)value
     * +--------+----------------------------------------------------------------+
     * + 1 byte +                            8 bytes                             +
     * +--------+----------------------------------------------------------------+
     * + 6|Type +                          integer value                         +
     * +--------+----------------------------------------------------------------+ 
     * 
     * pack integer to buffer. value < 0
     * +--------+--------------------------------+
     * + 1 byte +            4 bytes             +
     * +--------+--------------------------------+
     * + 4|Type +          integer value         +
     * +--------+--------------------------------+ 
     * 
     * pack integer to buffer. value < 0x100(256)
     * +--------+--------+
     * + 1 byte + 1 bytes+
     * +--------+--------+
     * + 1|Type +  value +
     * +--------+--------+ 
     * 
     * pack integer to buffer. value < 0x10000(16k)
     * +--------+----------------+
     * + 1 byte +    2 bytes     +
     * +--------+----------------+
     * + 2|Type +  integer value +
     * +--------+----------------+ 
     *
     * else
     * +--------+--------------------------------+
     * + 1 byte +            4 bytes             +
     * +--------+--------------------------------+
     * + 4|Type +          integer value         +
     * +--------+--------------------------------+ 
     */

    // Notice: Dot not change the define of SkynetPacketTag, they are all define in skynet source code
    enum RequestPacketTag
    {
        NORMAL_PACKET = 0x80, // a pakcet <= 32k will use this tag (battle server must send a response packet back to skynet clusters)
        LARGE_PACKET  = 0x81, // a packet > 32k  will use this tag (battle server must send a response packet back to skynet clusters)
        LARGE_PUSH    = 0xc1, // a packet > 32k will use this tag  (battle server dot not have to response packet back to skynet clusters)

        FRAGMENT     = 2,
        LAST_FRAGMENT = 3,
    }

    enum ResponsePacketTag
    {
        ERROR       = 0,
        OK          = 1,
        MULTI_BEGIN  = 2,
        MULTI_PART   = 3,
        MULTI_END    = 4,
    }

    enum LargePacketType
    {
        REQUEST     = 1,
        RESPONSE    = 2,
    }

    enum LuaNumberSubType
    {
        TYPE_NUMBER_ZERO    = 0,
        TYPE_NUMBER_BYTE    = 1,
        TYPE_NUMBER_WORD    = 2,
        TYPE_NUMBER_DWORD   = 4,
        TYPE_NUMBER_QWORD   = 6,
    }

    enum LuaStringType
    {
        TYPE_SHORT_STRING = 4,
        TYPE_LONG_STRING  = 5,
    }

    class SkynetMessage
    {
        public int Size { get; set; }
        public byte[] Data { get; set; }
    }

    class SkynetLargePacket
    {
        public int Tag { get; set; }
        public string ServiceName { get; set; }
        public int Session { get; set; }
        public int TotalDataSize { get; set; }
        public List<SkynetMessage> Messages { get; set; }
        public LargePacketType Type { get; set; }
    }

    class SkynetClusterRequest
    {
        public string ServiceName { get; set; }
        public int Session { get; set; }
        public int ProtoId { get; set; }
        public byte[] Data { get; set; }
    }

    class SkynetClusterResponse
    {
        public RPCError ErrorCode { get; set; }
        public int Session { get; set; }
        public int ProtoId { get; set; }
        public byte[] Data { get; set; }
    }

    class SkynetPacketManager
    {
        private Dictionary<int, SkynetLargePacket> m_largeRequestPackets;
        private Dictionary<int, SkynetLargePacket> m_largeResponsePackets;
        private int m_sourceId;
        private const int MultiPart = 0x8000;

        public void Init(int serviceId)
        {
            m_largeRequestPackets = new Dictionary<int, SkynetLargePacket>();
            m_largeResponsePackets = new Dictionary<int, SkynetLargePacket>();
            m_sourceId = serviceId;
        }

        public List<byte[]> PackSkynetRequest(string serviceName, int session, int protoId, byte[] data)
        {
            List<byte[]> result = new List<byte[]>();
            byte[] protoIdBytes = PackInteger(protoId);
            byte[] dataBytes = PackString(data);

            byte[] msg = new byte[protoIdBytes.Length + dataBytes.Length];
            protoIdBytes.CopyTo(msg, 0);
            dataBytes.CopyTo(msg, protoIdBytes.Length);

            if (msg.Length < MultiPart)
            {
                byte[] tempData = new byte[msg.Length + serviceName.Length + 6];
                int startIndex = PackHeader(tempData, (int)RequestPacketTag.NORMAL_PACKET, serviceName, session);
                msg.CopyTo(tempData, startIndex);
                result.Add(tempData);
            }
            else
            {
                byte[] headerBytes = new byte[serviceName.Length + 10];
                int startIndex = PackHeader(headerBytes, (int)RequestPacketTag.LARGE_PACKET, serviceName, session);
                byte[] msgLengthBytes = BitConverter.GetBytes(msg.Length);
                msgLengthBytes.CopyTo(headerBytes, startIndex);
                result.Add(headerBytes);

                int part = (msg.Length - 1) / MultiPart + 1;
                int sz = msg.Length;
                int copyedBytes = 0;
                for (int i = 0; i < part; i ++)
                {
                    if (sz > MultiPart)
                    {
                        byte[] tempData = new byte[MultiPart + 5];
                        int tempIndex = PackHeader(tempData, (int)RequestPacketTag.FRAGMENT, "", session);
                        Array.Copy(msg, copyedBytes, tempData, 5, MultiPart);

                        sz -= MultiPart;
                        copyedBytes += MultiPart;

                        result.Add(tempData);
                    }
                    else
                    {
                        byte[] tempData = new byte[sz + 5];
                        int tempIndex = PackHeader(tempData, (int)RequestPacketTag.LAST_FRAGMENT, "", session);
                        Array.Copy(msg, copyedBytes, tempData, 5, sz);

                        sz -= sz;
                        copyedBytes += sz;

                        result.Add(tempData);
                    }
                }
            }

            return result;
        }

        public SkynetClusterRequest UnpackSkynetRequest(byte[] msg)
        {
            SkynetClusterRequest request = null;
            byte tag = msg[0];

            switch(tag)
            {
                case (byte)RequestPacketTag.NORMAL_PACKET:
                    {
                        request = UnpackNormalRequestPacket(msg);
                    }break;
                case (byte)RequestPacketTag.LARGE_PACKET:
                case (byte)RequestPacketTag.FRAGMENT:
                case (byte)RequestPacketTag.LAST_FRAGMENT:
                case (byte)RequestPacketTag.LARGE_PUSH:
                    {
                        request = UnpackLargeRequestPacket((RequestPacketTag)tag, msg);
                    }break;
                default:
                    {
                        LoggerHelper.Info(m_sourceId, String.Format("Unknow request tag {0}", tag));
                    }break;
            }

            return request;
        }

        public List<byte[]> PackSkynetResponse(int remoteSession, int protoId, byte[] data)
        {
            List<byte[]> networkPacketList = new List<byte[]>();
            byte[] protoIdBuf = PackInteger(protoId);
            byte[] dataBuf = PackString(data);
            byte[] remoteSessionBytes = BitConverter.GetBytes(remoteSession);

            byte[] buffer = new byte[protoIdBuf.Length + dataBuf.Length];
            int startBufferIndex = 0;

            protoIdBuf.CopyTo(buffer, 0);
            dataBuf.CopyTo(buffer, protoIdBuf.Length);

            int totalBytes = protoIdBuf.Length + dataBuf.Length;
            if (totalBytes > MultiPart) // > 32k
            {
                short headerPacketLength = 9;
                byte[] headerPacketBuffer = new byte[headerPacketLength];
                Array.Copy(remoteSessionBytes, 0, headerPacketBuffer, 0, 4);

                headerPacketBuffer[4] = (byte)ResponsePacketTag.MULTI_BEGIN;
                byte[] totalBytesBuf = BitConverter.GetBytes(totalBytes);
                Array.Copy(totalBytesBuf, 0, headerPacketBuffer, 5, 4);
                networkPacketList.Add(headerPacketBuffer);

                int part = (totalBytes - 1) / MultiPart + 1;
                for (int i = 0; i < part; i ++)
                {
                    int s = 0;
                    byte[] tempBlock = null;
                    if (totalBytes > MultiPart)
                    {
                        s = MultiPart;
                        totalBytes -= MultiPart - 5;

                        tempBlock = new byte[s];
                        tempBlock[4] = (byte)ResponsePacketTag.MULTI_PART;
                    }
                    else
                    {
                        s = totalBytes + 5;
                        tempBlock = new byte[s];
                        tempBlock[4] = (byte)ResponsePacketTag.MULTI_END;
                    }

                    Array.Copy(remoteSessionBytes, 0, tempBlock, 0, 4);
                    Array.Copy(buffer, startBufferIndex, tempBlock, 5, s - 5);
                    startBufferIndex += s - 5;

                    byte[] p = tempBlock;
                    networkPacketList.Add(p);
                }
            }
            else
            {
                byte[] netpackBuffer = new byte[totalBytes + 5];

                Array.Copy(remoteSessionBytes, 0, netpackBuffer, 0, 4);
                netpackBuffer[4] = (byte)ResponsePacketTag.OK;
                Array.Copy(buffer, startBufferIndex, netpackBuffer, 5, totalBytes);

                networkPacketList.Add(netpackBuffer);
            }

            return networkPacketList;
        }

        public List<byte[]> PackErrorResponse(int session, string errorText)
        {
            errorText = errorText.Substring(0, Math.Min(errorText.Length, MultiPart));

            byte[] sessionBytes = BitConverter.GetBytes(session);
            byte[] errorTextBytes = Encoding.ASCII.GetBytes(errorText);
            byte[] response = new byte[errorTextBytes.Length + 5];
            sessionBytes.CopyTo(response, 0);
            response[4] = (byte)ResponsePacketTag.ERROR;
            errorTextBytes.CopyTo(response, 5);

            List<byte[]> byteList = new List<byte[]>();
            byteList.Add(response);
            return byteList;
        }

        public SkynetClusterResponse UnpackSkynetResponse(byte[] msg)
        {
            SkynetClusterResponse response = null;

            int startIndex = 0;
            int session = msg[0] | msg[1] << 8 | msg[2] << 16 | msg[3] << 24;
            startIndex += 4;

            int tag = msg[startIndex];
            startIndex++;

            switch((ResponsePacketTag)tag)
            {
                case ResponsePacketTag.OK:
                    {
                        response = new SkynetClusterResponse();

                        int byteCount = 0;
                        int protoId = (int)UnpackInteger(msg, startIndex, out byteCount);
                        startIndex += 1 + byteCount;
                        byte[] tempData = UnpackString(msg, startIndex);

                        response.ErrorCode = RPCError.OK;
                        response.ProtoId = protoId;
                        response.Data = tempData;
                    }
                    break;
                case ResponsePacketTag.ERROR:
                    {
                        response = new SkynetClusterResponse();

                        response.ErrorCode = RPCError.RemoteError;
                        response.ProtoId = 0;
                        response.Data = UnpackString(msg, startIndex);
                    } break;
                case ResponsePacketTag.MULTI_BEGIN:
                    {
                        SkynetLargePacket largePacket = null;
                        bool isExist = m_largeResponsePackets.TryGetValue(session, out largePacket);
                        if (!isExist)
                        {
                            largePacket = new SkynetLargePacket();
                            m_largeResponsePackets.Add(session, largePacket);
                        }

                        largePacket.Tag = tag;
                        largePacket.Type = LargePacketType.RESPONSE;
                        largePacket.Session = session;

                        int byteCount = 0;
                        largePacket.TotalDataSize = (int)UnpackInteger(msg, startIndex, out byteCount);
                        largePacket.Messages = new List<SkynetMessage>();
                    } break;
                case ResponsePacketTag.MULTI_PART:
                    {
                        SkynetLargePacket largePacket = null;
                        bool isExist = m_largeResponsePackets.TryGetValue(session, out largePacket);
                        if (isExist)
                        {
                            SkynetMessage skynetMessage = new SkynetMessage();
                            skynetMessage.Size = msg.Length - 5;
                            skynetMessage.Data = UnpackString(msg, startIndex);

                            largePacket.Messages.Add(skynetMessage);
                        }
                    } break;
                case ResponsePacketTag.MULTI_END:
                    {
                        SkynetLargePacket largePacket = null;
                        bool isExist = m_largeResponsePackets.TryGetValue(session, out largePacket);
                        if (isExist)
                        {
                            response = new SkynetClusterResponse();

                            byte[] tempData = new byte[largePacket.TotalDataSize];
                            int tempStartIndex = 0;
                            int count = largePacket.Messages.Count;
                            for (int i = 0; i < count; i ++)
                            {
                                SkynetMessage skynetMessage = largePacket.Messages[i];
                                Array.Copy(skynetMessage.Data, 0, tempData, tempStartIndex, skynetMessage.Size);
                                tempStartIndex += skynetMessage.Size;
                            }

                            Array.Copy(msg, 5, tempData, tempStartIndex, msg.Length - 5);

                            int byteCount = 0;
                            response.ErrorCode = RPCError.OK;
                            response.ProtoId = (int)UnpackInteger(tempData, 0, out byteCount);
                            response.Data = UnpackString(tempData, byteCount);
                        }
                    } break;
                default: break;
            }

            if (response != null)
            {
                response.Session = session;   
            }

            return response;
        }

        private byte[] PackInteger(Int64 value)
        {
            byte[] result = null;

            // TYPE_NUMBER == 2
            if (value == 0)
            {
                result = new byte[1];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_ZERO << 3) | 2;
            }
            else if (value > Int32.MaxValue)
            {
                byte[] temp = BitConverter.GetBytes(value);
                result = new byte[9];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_QWORD << 3) | 2;
                Array.Copy(temp, 0, result, 1, 8);
            }
            else if (value < 0)
            {
                byte[] temp = BitConverter.GetBytes((int)value);
                result = new byte[5];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_DWORD << 3) | 2;
                Array.Copy(temp, 0, result, 1, 4);
            }
            else if (value < 0x100) // less than 256 bytes
            {
                result = new byte[2];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_BYTE << 3) | 2;
                result[1] = (byte)value;
            }
            else if (value < 0x10000) // less than 64k
            {
                byte[] temp = BitConverter.GetBytes((short)value);
                result = new byte[3];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_WORD << 3) | 2;
                Array.Copy(temp, 0, result, 1, 2);
            }
            else
            {
                byte[] temp = BitConverter.GetBytes((int)value);
                result = new byte[5];
                result[0] = ((byte)LuaNumberSubType.TYPE_NUMBER_DWORD << 3) | 2;
                Array.Copy(temp, 0, temp, 1, 4);
            }

            return result;
        }

        private byte[] PackString(byte[] data)
        {
            byte[] result = null;
            if (data.Length < 32)
            {
                result = new byte[data.Length + 1];
                byte len = (byte)data.Length;
                result[0] = (byte)((len << 3) | (byte)LuaStringType.TYPE_SHORT_STRING);
                Array.Copy(data, 0, result, 1, len);
            }
            else
            {
                if (data.Length < 0x10000)
                {
                    byte[] tempLengthBytes = BitConverter.GetBytes((short)data.Length);

                    result = new byte[data.Length + 3];
                    result[0] = (2 << 3) | (byte)LuaStringType.TYPE_LONG_STRING;
                    Array.Copy(tempLengthBytes, 0, result, 1, 2);
                    Array.Copy(data, 0, result, 3, data.Length);
                }
                else
                {
                    byte[] tempLengthBytes = BitConverter.GetBytes((int)data.Length);
                    result = new byte[data.Length + 5];
                    result[0] = (4 << 3) | (byte)LuaStringType.TYPE_LONG_STRING;

                    Array.Copy(tempLengthBytes, 0, result, 1, 4);
                    Array.Copy(data, 0, result, 5, data.Length);
                }
            }
            return result;
        }

        private int PackHeader(byte[] data, int tag, string name, int session)
        {
            int startIndex = 0;
            data[0] = (byte)tag;
            startIndex += 1;

            if (name.Length > 0)
            {
                data[1] = (byte)name.Length;
                startIndex += 1;

                byte[] nameBytes = Encoding.ASCII.GetBytes(name);
                nameBytes.CopyTo(data, 2);
                startIndex += nameBytes.Length;
            }

            byte[] sessionBytes = BitConverter.GetBytes(session);
            sessionBytes.CopyTo(data, startIndex);
            startIndex += 4;

            return startIndex;
        }

        private void UnpackHeader(byte[] data, out byte tag, out string name, out int nameLength, out int session)
        {
            tag = data[0];
            nameLength = data[1];
            name = Encoding.ASCII.GetString(data, 2, nameLength);
            session = data[nameLength + 5] << 24 | data[nameLength + 4] << 16 | data[nameLength + 3] << 8 | data[nameLength + 2];
        }

        private Int64 UnpackInteger(byte[] data, int startIndex, out int byteCount)
        {
            Int64 result = Int64.MaxValue;
            byteCount = 0;

            byte type = (byte)(data[startIndex] & 0x07);
            byte len = (byte)((data[startIndex] & 0x1f) >> 3);
            Debug.Assert(type == 2); // TYPE_NUMBER == 2

            switch(len)
            {
                case (byte)LuaNumberSubType.TYPE_NUMBER_ZERO:
                    {
                        byteCount = 0;
                        result = 0;
                    }break;
                case (byte)LuaNumberSubType.TYPE_NUMBER_BYTE: // 1 byte
                    {
                        result = 0;
                        byteCount = 1;

                        result = (Int64)data[startIndex + 1];
                    }break;
                case (byte)LuaNumberSubType.TYPE_NUMBER_WORD: // 2 byte
                    {
                        result = 0;
                        byteCount = 2;

                        result = (Int64)(data[startIndex + 2] << 8 | data[startIndex + 1]);
                    }break;
                case (byte)LuaNumberSubType.TYPE_NUMBER_DWORD: // 4 byte
                    {
                        result = 0;
                        byteCount = 4;

                        result = (Int64)(data[startIndex + 4] << 24 | data[startIndex + 3] << 16 | data[startIndex + 2] << 8 | data[startIndex + 1]);
                    }break;
                case (byte)LuaNumberSubType.TYPE_NUMBER_QWORD: // 8 byte, Notice:TYPE_NUMBER_QWORD is 6, it means integer is 8 bytes
                    {
                        result = 0;
                        byteCount = 8;

                        result = (Int64)(data[startIndex + 8] << 56 | 
                            data[startIndex + 7] << 48 |
                            data[startIndex + 6] << 40 |
                            data[startIndex + 5] << 32 |
                            data[startIndex + 4] << 24 |
                            data[startIndex + 3] << 16 |
                            data[startIndex + 2] << 8 |
                            data[startIndex + 1]);
                    }break;
                default: break;
            }

            return result;
        }

        private byte[] UnpackString(byte[] data, int startIndex)
        {
            byte[] buffer = null;
            byte type = (byte)(data[startIndex] & 0x07);
            byte head = (byte)((data[startIndex] >> 3) & 0x1f);

            switch(type)
            {
                case 4: // TYPE_SHORT_STRING
                    {
                        int len = (int)head;
                        buffer = new byte[len];
                        Array.Copy(data, startIndex + 1, buffer, 0, len);
                    }break;
                case 5: // TYPE_LONG_STRING
                    {
                        if (head == 2)
                        {
                            int len = (int)(data[startIndex + 2] << 8 | data[startIndex + 1]);
                            buffer = new byte[len];
                            Array.Copy(data, startIndex + 3, buffer, 0, len);
                        }
                        else if(head == 4)
                        {
                            int len = (int)(data[startIndex + 4] << 24 | data[startIndex + 3] << 16 | data[startIndex + 2] << 8 | data[startIndex + 1]);
                            buffer = new byte[len];
                            Array.Copy(data, startIndex + 5, buffer, 0, len);
                        }
                        else
                        {
                            LoggerHelper.Info(m_sourceId, String.Format("SkynetPacketManager.UnpackString Unknow string head {0}", head));
                        }
                    }break;
                default: break;
            }

            return buffer;
        }

        private SkynetClusterRequest UnpackNormalRequestPacket(byte[] data)
        {
            SkynetClusterRequest request = new SkynetClusterRequest();

            // unpack header
            byte tag = 0;
            string name = "";
            int nameLength = 0;
            int session = 0;
            UnpackHeader(data, out tag, out name, out nameLength, out session);

            int startIndex = nameLength + 6;

            // unpack message: protoId and message buffer
            int byteCount = 0;
            int protoId = (int)UnpackInteger(data, startIndex, out byteCount);
            startIndex += 1 + byteCount;

            byte[] msg = UnpackString(data, startIndex);

            request.ProtoId = protoId;
            request.ServiceName = name;
            request.Session = session;
            request.Data = msg;

            return request;
        }

        private SkynetClusterRequest UnpackLargeRequestPacket(RequestPacketTag tag, byte[] data)
        {
            SkynetClusterRequest request = null;

            switch(tag)
            {
                case RequestPacketTag.LARGE_PACKET:
                case RequestPacketTag.LARGE_PUSH:
                    {
                        // unpack header
                        byte tempTag = 0;
                        string name = "";
                        int nameLength = 0;
                        int session = 0;
                        UnpackHeader(data, out tempTag, out name, out nameLength, out session);

                        if (m_largeRequestPackets.ContainsKey(session))
                        {
                            LoggerHelper.Info(m_sourceId, String.Format("SkynetPacketManager.UnpackLargePacket multi header for same session {0}", session));
                            m_largeRequestPackets.Remove(session);
                        }

                        int startIndex = nameLength + 6;
                        int totalDataSize = data[startIndex + 3] << 24 | data[startIndex + 2] << 16 | data[startIndex + 1] << 8 | data[startIndex];

                        SkynetLargePacket largeRequest = new SkynetLargePacket();
                        largeRequest.ServiceName = name;
                        largeRequest.Session = session;
                        largeRequest.Tag = (int)tempTag;
                        largeRequest.TotalDataSize = totalDataSize;
                        largeRequest.Messages = new List<SkynetMessage>();
                        largeRequest.Type = LargePacketType.REQUEST;

                        m_largeRequestPackets.Add(session, largeRequest);
                    }break;
                case RequestPacketTag.FRAGMENT:
                case RequestPacketTag.LAST_FRAGMENT:
                    {
                        int session = data[4] << 24 | data[3] << 16 | data[2] << 8 | data[1];

                        int size = data.Length - 5;
                        byte[] msg = new byte[size];
                        Array.Copy(data, 5, msg, 0, size);

                        SkynetMessage skynetMessage = new SkynetMessage();
                        skynetMessage.Data = msg;
                        skynetMessage.Size = size;

                        SkynetLargePacket largeRequest = null;
                        bool isSuccess = m_largeRequestPackets.TryGetValue(session, out largeRequest);
                        if (!isSuccess)
                        {
                            LoggerHelper.Info(m_sourceId, String.Format("SkynetPacketManager.UnpackLargePacket illegal FRAGMENT {0}", session));
                            return null;
                        }
                        largeRequest.Messages.Add(skynetMessage);

                        if (tag == RequestPacketTag.LAST_FRAGMENT)
                        {
                            byte[] messageBuf = new byte[largeRequest.TotalDataSize];
                            int startIndex = 0;
                            int sumBytes = 0;
                            for (int i = 0; i < largeRequest.Messages.Count; i ++)
                            {
                                SkynetMessage tempMsg = largeRequest.Messages[i];
                                Array.Copy(tempMsg.Data, 0, messageBuf, startIndex, tempMsg.Size);
                                startIndex += tempMsg.Size;
                                sumBytes += tempMsg.Size;
                            }

                            if (largeRequest.TotalDataSize != sumBytes)
                            {
                                LoggerHelper.Info(m_sourceId, String.Format("SkynetPacketManager.UnpackLargePacket large packet totalsize is not equal to real size {0}", session));
                            }

                            // unpack message: protoId and message buffer
                            startIndex = 0;
                            int byteCount = 0;
                            int protoId = (int)UnpackInteger(messageBuf, startIndex, out byteCount);
                            startIndex += 1 + byteCount;

                            byte[] unpackMsg = UnpackString(messageBuf, startIndex);

                            request = new SkynetClusterRequest();
                            request.ProtoId = protoId;
                            request.ServiceName = largeRequest.ServiceName;
                            request.Session = session;
                            request.Data = unpackMsg;

                            m_largeRequestPackets.Remove(session);
                        }
                    }break;
                default: break;
            }

            return request;
        }
    }
}