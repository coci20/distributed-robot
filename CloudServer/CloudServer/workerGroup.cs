using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudServer
{
    class workerGroup : IComparable
    {
        public uint group_id;
        public uint number_of_workers;
        public uint number_of_robots;
        public int group_port_base;

        public int CompareTo(object obj)
        {
            workerGroup group = obj as workerGroup;
            if (group.number_of_robots < number_of_robots)
            {
                return 1;
            }
            if (group.number_of_robots > number_of_robots)
            {
                return -1;
            }

            // The orders are equivalent.
            return 0;
        }
    }
}
