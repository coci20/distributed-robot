using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Collect data from console given by main program */
            Console.WriteLine("Starting Worker Server ...");

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments provided to start server");
            }

            uint group_id = Convert.ToUInt32(args[0]);
            uint worker_id = Convert.ToUInt32(args[1]);

            worker worker_instance = new worker(group_id, worker_id);
        }
    }
}
