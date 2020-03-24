using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleApplication2
{   //控制可用buf，使用事件方法在不同线程间实现同步
class Program
{
    private static Mutex mut;
    private static int SharedBuffer;
    private static int BufferState;
    private static Thread[] threadVec;
    private static AutoResetEvent hNotFullEvent, hNotEmptyEvent; 
    public const int FULL=1;
    public const int EMPTY = 0; 

    private static void Producer()
    {
         int i;

         for (i = 20; i >= 0; i--)
         {
             while (true)
             {
                 mut.WaitOne(); 

                 if (BufferState == FULL)
                 {
                     mut.ReleaseMutex();
                     hNotFullEvent.WaitOne();
                     continue; // back to loop to test BufferState again
                 } 
                 // got mutex and buffer is not FULL, break out of while loop
                 break;
             }//end of while
             // got Mutex, buffer is not full, producing data 
             Console.WriteLine("Produce: {0}", i);
             SharedBuffer = i;
             BufferState = FULL;
             mut.ReleaseMutex();
             hNotEmptyEvent.Set(); 
             Thread.Sleep(3000);
         }//end of for
    }//end of Producer thread


    private static void Consumer()
    {
        int result;
        while (true) 
        {
            mut.WaitOne();
            if (BufferState == EMPTY)
            { // nothing to consume
                mut.ReleaseMutex();
                // wait until buffer is not empty
                hNotEmptyEvent.WaitOne();
                continue; // return to while loop to contend for Mutex again
            }
            if (SharedBuffer == 0)
            { // test for end of data token
                Console.WriteLine("Consumed  {0}: end of data\r\n", SharedBuffer);
                mut.ReleaseMutex();
                break;
            }
            else
            {
                result = SharedBuffer;
                Console.WriteLine("Consumed: {0}", result);
                BufferState = EMPTY;
                mut.ReleaseMutex();
                hNotFullEvent.Set();
            }
        }//end of while
    }//end of Consumer thread
     

    static void Main(string[] args)
    {
        SharedBuffer = 20;
        mut = new Mutex(false, "Tr");
        hNotFullEvent = new AutoResetEvent(false);
        hNotEmptyEvent = new AutoResetEvent(false); 
        threadVec = new Thread[2];
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
