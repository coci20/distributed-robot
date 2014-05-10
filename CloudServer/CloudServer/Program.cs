using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace CloudServer
{
    class Program
    {
        static void Main(string[] args)
        {
            uint no_of_groups, no_of_replicas;
            uint worker_count = 0;

            /* Collect data from console given by user */
            Console.WriteLine("Starting Server Application ...");

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments provided to start server");
            }

            no_of_groups = Convert.ToUInt32(args[0]);
            no_of_replicas = Convert.ToUInt32(args[1]);
            
            /* Start the Workers based on the input given by the user */
            for (uint i=1; i <= no_of_groups; i++){
                for (uint j=1; j <= no_of_replicas; j++){
                    worker_count++;
                    Process workerProcess = new Process();
                    workerProcess.StartInfo.Arguments = Convert.ToString(i) + " " + Convert.ToString(worker_count);
                    workerProcess.StartInfo.CreateNoWindow = false;
                    workerProcess.StartInfo.FileName = "C:\\Users\\harsh\\Documents\\Visual Studio 2010\\Projects\\CloudServer\\WorkerServer\\bin\\Debug\\WorkerServer.exe";
                    workerProcess.Start();
                }
            }

            /* Start the Master server */
            master master_instance = new master(no_of_groups, no_of_replicas);

        }
    }
}
