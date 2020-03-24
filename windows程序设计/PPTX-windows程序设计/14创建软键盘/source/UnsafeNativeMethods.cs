using System;
using System.Runtime.InteropServices;

namespace softKB
{
    internal static class UnsafeNativeMethods
    { 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern short GetKeyState(int nVirtKey);
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SendInput(uint nInputs, NativeMethods.INPUT[] pInputs,
            int cbSize);
    }
}
