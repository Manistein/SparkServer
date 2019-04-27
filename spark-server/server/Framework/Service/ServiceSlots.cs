using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SparkServer.Framework.Service
{
    class ServiceSlots
    {
        private static ServiceSlots m_instance;
        private static ReaderWriterLock rwlock = new ReaderWriterLock();

        private ServiceBase[] m_slots;
        private int m_handleIndex = 1;
        private const int DefaultServiceSize = 8;

        ConcurrentDictionary<string, int> m_service2name = new ConcurrentDictionary<string, int>();

        // We should call this function first in main thread
        public static ServiceSlots GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new ServiceSlots();
            }

            return m_instance;
        }

        public int Add(ServiceBase service)
        {
            if (service.GetId() > 0)
            {
                return service.GetId();
            }

            int result = 0;
            try
            {
                rwlock.AcquireWriterLock(Timeout.InfiniteTimeSpan);
                try
                {
                    if (m_slots == null)
                    {
                        m_slots = new ServiceBase[DefaultServiceSize];
                    }

                    bool isFind = false;
                    int handle = m_handleIndex;
                    while (!isFind)
                    {
                        for (int i = 0; i < m_slots.Length; i++)
                        {
                            if (handle >= int.MaxValue)
                            {
                                handle = 1;
                            }

                            int hash = handle & (m_slots.Length - 1);
                            if (m_slots[hash] == null)
                            {
                                service.SetId(handle);
                                m_slots[hash] = service;
                                result = handle;

                                m_handleIndex = handle + 1;
                                isFind = true;

                                break;
                            }

                            handle++;
                        }

                        if (!isFind)
                        {
                            int oldSize = m_slots.Length;
                            int newSize = m_slots.Length * 2;
                            ServiceBase[] newSlots = new ServiceBase[newSize];
                            for (int i = 0; i < m_slots.Length; i ++)
                            {
                                ServiceBase slotService = m_slots[i];
                                int hash = slotService.GetId() & (newSize - 1);
                                newSlots[hash] = slotService;
                            }
                            m_slots = newSlots;
                            m_handleIndex = oldSize;
                            handle = m_handleIndex;
                        }
                    }
                }
                finally
                {
                    rwlock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException e)
            {

            }

            return result;
        }

        public ServiceBase Get(int serviceId)
        {
            ServiceBase s = null;

            try
            {
                rwlock.AcquireReaderLock(Timeout.InfiniteTimeSpan);
                try
                {
                    int hash = serviceId & (m_slots.Length - 1);
                    ServiceBase slot = m_slots[hash];
                    if (slot.GetId() == serviceId)
                    {
                        s = slot;
                    }
                }
                finally
                {
                    rwlock.ReleaseReaderLock();
                }
            }
            catch(ApplicationException e)
            {

            }

            return s;
        }

        public ServiceBase Get(string name)
        {
            int serviceId = 0;
            m_service2name.TryGetValue(name, out serviceId);
            return Get(serviceId);
        }

        public void Name(int serviceId, string name)
        {
            ServiceBase s = Get(serviceId);
            if (s != null)
            {
                m_service2name.TryAdd(name, serviceId);
            }
        }

        public int Name2Id(string name)
        {
            int serviceId = 0;
            m_service2name.TryGetValue(name, out serviceId);
            return serviceId;
        }
    }
}