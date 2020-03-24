using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
class Program
{   //没有实现对buf可用空间的控制，只使用了互斥量访问公共变量
    private static Mutex mut;
    private static Thread[] threadVec;
    private static int SharedBuffer;
    private static void Consumer()
    {
        while (true)
        {
            int result;
            mut.WaitOne();
            if (SharedBuffer == 0)
            {
                Console.WriteLine("Consumed {0}: end of data\r\n", SharedBuffer);
                mut.ReleaseMutex();
                break;
            }
            if (SharedBuffer > 0)
            { // ignore negative values
                result = SharedBuffer;
                Console.WriteLine("Consumed: {0}", result);
                mut.ReleaseMutex();
            } 
        }
    }

    private static void Producer()
    {
        int i;
        for (i = 20; i >= 0; i--)
        {
            mut.WaitOne();
            Console.WriteLine("Produce: {0}", i);
            SharedBuffer = i;
            mut.ReleaseMutex();
            Thread.Sleep(1000);
        }
    }
    static void Main(string[] args)
    {
        SharedBuffer = 20;
        mut  = new Mutex(false,"Tr");
        threadVec=new Thread[2];
        threadVec[0] = new Thread(new ThreadStart(Consumer));
        threadVec[1] = new Thread(new ThreadStart(Producer));
        threadVec[0].Start();
        threadVec[1].Start();
        threadVec[0].Join();
        threadVec[1].Join();
        Console.ReadLine();
    }
}
}
