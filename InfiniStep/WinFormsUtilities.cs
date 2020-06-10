using System;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace GetWinFormsId

{
    public class WinFormsUtilities
    {

        public class NativeMethods
        {

                [DllImport("kernel32.dll")]
                public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

                [DllImport("kernel32.dll")]
                public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,UIntPtr dwSize, uint flAllocationType, PageProtection flProtect);

                [DllImport("user32.dll", SetLastError=true)]
                public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

                [DllImport("kernel32.dll")]
                public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

                [DllImport("kernel32.dll")]
                public static extern bool CloseHandle(IntPtr hObject);

                [DllImport("kernel32.dll")]
                public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow,

                    UIntPtr dwNumberOfBytesToMap);

                [DllImport("kernel32.dll")]

                public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

                [DllImport("kernel32.dll", SetLastError=true)]

                public static extern IntPtr CreateFileMapping(IntPtr hFile,

                    IntPtr lpFileMappingAttributes, PageProtection flProtect, int dwMaximumSizeHigh,

                    int dwMaximumSizeLow, string lpName);

                [DllImport("user32.dll")]

                public static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

                [DllImport("kernel32.dll")]

                public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,

                    [Out] byte [] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

                [DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]

                public static extern void MoveMemoryFromByte(IntPtr dest, ref byte src, int size);

                [DllImport("Kernel32.dll", EntryPoint="RtlMoveMemory", SetLastError=false)]

                public static extern void MoveMemoryToByte(ref byte dest, IntPtr src, int size);

                [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Ansi)]

                public static extern int RegisterWindowMessage(string lpString);

                [DllImport("user32.dll")]
                public static extern IntPtr WindowFromPoint(int x,int y);

                [DllImport("User32.Dll")]
                public static extern IntPtr GetParent(IntPtr hWnd);

                [DllImport("user32.dll")]
                public static extern IntPtr GetForegroundWindow();

                [DllImport("user32.dll")]
                public static extern int GetWindowTextLength(IntPtr hWnd);

                //  int GetWindowText(
                //      __in   HWND hWnd,
                //      __out  LPTSTR lpString,
                //      __in   int nMaxCount
                //  );
                [DllImport("user32.dll")]
                public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

                //HANDLE WINAPI OpenProcess(
                //  __in  DWORD dwDesiredAccess,
                //  __in  BOOL bInheritHandle,
                //  __in  DWORD dwProcessId
                //);
                [DllImport("psapi.dll")]
                public static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

                //  DWORD WINAPI GetModuleFileNameEx(
                //      __in      HANDLE hProcess,
                //      __in_opt  HMODULE hModule,
                //      __out     LPTSTR lpFilename,
                //      __in      DWORD nSize
                //  );
                [DllImport("psapi.dll")]
                public  static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

                [DllImport("kernel32.dll")]
                public static extern int WideCharToMultiByte(uint CodePage, uint dwFlags,
                                    [MarshalAs(UnmanagedType.LPWStr)] string lpWideCharStr, int cchWideChar,
                                    [MarshalAs(UnmanagedType.LPArray)] byte[] lpMultiByteStr, int cbMultiByte,
                                    uint lpDefaultChar, uint lpUsedDefaultChar);
                public const uint CP_ACP = 0;
                const uint CP_SHIFTJIS = 932;

                [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
                public static extern int GetClassName(
                    IntPtr hWnd,
                    StringBuilder s,
                    int nMaxCount
                    );
                [DllImport("user32.dll")]
                public static extern IntPtr GetTopWindow(IntPtr hWnd);

                [DllImport("user32.dll", ExactSpelling = true)]
                public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

                [DllImport("user32.dll")]
                public static extern IntPtr ChildWindowFromPointEx(IntPtr hWndParent, Point pt, uint uFlags);
                /// <summary>
                /// For use with ChildWindowFromPointEx 
                /// </summary>
                [Flags]
                public enum WindowFromPointFlags
                {
                    /// <summary>
                    /// Does not skip any child windows
                    /// </summary>
                    CWP_ALL = 0x0000,
                    /// <summary>
                    /// Skips invisible child windows
                    /// </summary>
                    CWP_SKIPINVISIBLE = 0x0001,
                    /// <summary>
                    /// Skips disabled child windows
                    /// </summary>
                    CWP_SKIPDISABLED = 0x0002,
                    /// <summary>
                    /// Skips transparent child windows
                    /// </summary>
                    CWP_SKIPTRANSPARENT = 0x0004
                }

                [DllImport("user32.dll")]
                public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

                [DllImport("user32.dll")]
                [return: MarshalAs(UnmanagedType.Bool)]
                public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

                public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

                public static bool EnumWindow(IntPtr handle, IntPtr pointer)
                {
                    GCHandle gch = GCHandle.FromIntPtr(pointer);
                    List<IntPtr> list = gch.Target as List<IntPtr>;
                    if (list == null)
                    {
                        throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
                    }
                    //list
                    //uint process_id= GetProcessIdFromHWnd(handle);
                    //if (data.process_id != process_id || !is_main_window(handle)) {
                    //    return true;
                    //}
                    //data.best_handle = handle;
                    return false;  
                }
                [DllImport("user32.dll")]
                [return: MarshalAs(UnmanagedType.Bool)]
                static extern bool IsWindowVisible(IntPtr hWnd);
                [DllImport("user32.dll", SetLastError = true)]
                public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);
                public enum GetWindowType : uint
                {
                    GW_HWNDFIRST = 0,
                    GW_HWNDLAST = 1,
                    GW_HWNDNEXT = 2,
                    GW_HWNDPREV = 3,
                    GW_OWNER = 4,
                    GW_CHILD = 5,
                    GW_ENABLEDPOPUP = 6
                }

                static bool is_main_window(IntPtr handle)
                {
                    return GetWindow(handle, GetWindowType.GW_OWNER) == (IntPtr)0 && IsWindowVisible(handle);
                }
                struct handle_data {
                    uint process_id;
                    IntPtr best_handle;
                };

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                public static extern bool PhysicalToLogicalPoint(IntPtr hwnd, ref Point pt);

                [DllImport("user32.dll")]
                public static extern bool GetPhysicalCursorPos(ref Point lpPoint);

                [DllImport("user32.dll")]
                public static extern bool GetWindowRect(IntPtr HWND, out System.Windows.Rect rect);
                [DllImport("Imm32.dll")]
                public static extern IntPtr ImmGetContext(IntPtr hWnd);
                [DllImport("Imm32.dll")]
                public static extern bool ImmGetOpenStatus(IntPtr hIMC);
                [DllImport("Imm32.dll")]
                public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
            //============NT Shared memory constant======================

            public const short PROCESS_VM_OPERATION = 0x8;

                public const short PROCESS_VM_READ = 0x10;

                public const short PROCESS_VM_WRITE = 0x20;

                public const long PROCESS_ALL_ACCESS = 0x1F0FFF;

                public const short MEM_COMMIT = 0x1000;

                public const short MEM_RESERVE = 0x2000;

                public const short MEM_DECOMMIT = 0x4000;

                public const int MEM_RELEASE = 0x8000;

                public const int MEM_FREE = 0x10000;

                public const int MEM_PRIVATE = 0x20000;

                public const int MEM_MAPPED = 0x40000;

                public const int MEM_TOP_DOWN = 0x100000;


                public const int INVALID_HANDLE_VALUE = -1;

         }


        [Flags]
        public enum PageProtection : uint
        {

            NoAccess =     0x01,

            Readonly =     0x02,

            ReadWrite =    0x04,

            WriteCopy =    0x08,

            Execute =      0x10,

            ExecuteRead =      0x20,

            ExecuteReadWrite = 0x40,

            ExecuteWriteCopy = 0x80,

            Guard =        0x100,

            NoCache =      0x200,

            WriteCombine =     0x400,

        }
        public enum GetAncestorFlags
        {
            GetParent = 1,
            GetRoot = 2,
            GetRootOwner = 3
        }

        private static int GetControlNameMessage = 0;


        static WinFormsUtilities()
        {

            GetControlNameMessage = NativeMethods.RegisterWindowMessage("WM_GETCONTROLNAME");

        }


        public static string GetWinFormsId(IntPtr hWnd)
        {
            //string name =XProcGetControlName(hWnd, GetControlNameMessage);
            //if (string.IsNullOrEmpty(name))
            //{
            return GetClassName(hWnd);
            //}else
            //{
            //    string pname = XProcGetControlName(GetTopWindow(hWnd), GetControlNameMessage);
            //    return pname + "." + name;
            //}
        }


        protected static string XProcGetControlName(IntPtr hwnd, int msg)
        {

            //define the buffer that will eventually contain the desired window’s WinFormsId
            byte[] bytearray = new byte[65536];

            //allocate space in the target process for the buffer as shared memory

            IntPtr bufferMem = IntPtr.Zero; //base address of the allocated region for the buffer

            IntPtr written= IntPtr.Zero;  //number of bytes written to memory

            IntPtr retHandle= IntPtr.Zero;

            bool retVal;

            //creating and reading from a shared memory region is done differently in Win9x then in newer OSs

            IntPtr processHandle= IntPtr.Zero;

            IntPtr fileHandle= IntPtr.Zero;



            try
            {
                uint size; //the amount of memory to be allocated

                size = 65536;


                processHandle = NativeMethods.OpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE, false, GetProcessIdFromHWnd(hwnd));


                if(processHandle.ToInt64() == 0)
                {
                    throw new Win32Exception();
                }


                bufferMem = NativeMethods.VirtualAllocEx(processHandle, IntPtr.Zero, new UIntPtr(size), NativeMethods.MEM_RESERVE | NativeMethods.MEM_COMMIT, PageProtection.ReadWrite);


                if(bufferMem.ToInt64() == 0)
                {
                    throw new Win32Exception();
                }


                //send message to the control’s hWnd for getting the specified control name

                retHandle = NativeMethods.SendMessage(hwnd, msg, new IntPtr(size), bufferMem);


                //now read the TVITEM’s info from the shared memory location

                retVal = NativeMethods.ReadProcessMemory(processHandle, bufferMem, bytearray, new UIntPtr(size), written);

                if(!retVal)
                {
                    throw new Win32Exception();
                }

            }

            finally
            {
                //free the memory that was allocated

                retVal = NativeMethods.VirtualFreeEx(processHandle, bufferMem, new UIntPtr(0), NativeMethods.MEM_RELEASE);

                if(!retVal)
                {
                    throw new Win32Exception();
                }
                NativeMethods.CloseHandle(processHandle);
            }

            return ByteArrayToString(bytearray);
        }


        private static uint GetProcessIdFromHWnd(IntPtr hwnd)
        {
            uint pid;

            NativeMethods.GetWindowThreadProcessId(hwnd, out pid);


            return pid;

        }

        private static string ByteArrayToString(byte[] bytes)
        {
            if(Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                // Use the Ansii encoder

                return Encoding.Default.GetString(bytes).TrimEnd('\0');

            }
            else
            {
                // use Unicode
                return Encoding.Unicode.GetString(bytes).TrimEnd('\0');
            }
        }

        public static string GetTopWindowText(IntPtr hWnd)
        {
            hWnd = GetTopWindow(hWnd);
            int length = NativeMethods.GetWindowTextLength(hWnd);
            StringBuilder text = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }
        public static string GetWindowText(IntPtr hWnd)
        {
            int length = NativeMethods.GetWindowTextLength(hWnd);
            StringBuilder text = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }

        public static string GetTopWindowName(IntPtr hWnd)
        {
            //IntPtr hWnd = NativeMethods.GetForegroundWindow();
            uint lpdwProcessId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            IntPtr hProcess = NativeMethods.OpenProcess(0x0410, false, lpdwProcessId);

            StringBuilder text = new StringBuilder(1000);
            NativeMethods.GetModuleBaseName(hProcess, IntPtr.Zero, text, text.Capacity);
            //NativeMethods.GetModuleFileNameEx(hProcess, IntPtr.Zero, text, text.Capacity);

            NativeMethods.CloseHandle(hProcess);

            return text.ToString();
        }
        public static string GetClassName(IntPtr hWnd,StringBuilder sb=null)
        {
            if(sb==null)
            {
                sb = new StringBuilder();
            }
            StringBuilder sbClassName = new StringBuilder(256);

            IntPtr PhWnd = NativeMethods.GetParent(hWnd);
            //if ( PhWnd != IntPtr.Zero)
            //{
            //    sb.Append(GetClassName(PhWnd,sb) + ".");
            //}

            NativeMethods.GetClassName(hWnd, sbClassName, sbClassName.Capacity);

            return sbClassName.ToString();
        }
        public static IntPtr GetTopWindow(IntPtr hWnd)
        {
            IntPtr phWnd = NativeMethods.GetAncestor(hWnd, GetAncestorFlags.GetRoot);

            return phWnd;
        }

        public static IntPtr GetTopmostHwnd(IntPtr inhwnd)
        {
            if (inhwnd == IntPtr.Zero)
                return inhwnd;

            var topWnd = GetTopWindow(inhwnd);
            var topmostHwnd = IntPtr.Zero;

            var hwnd = inhwnd;

            while (hwnd != IntPtr.Zero)
            {
                if(topWnd == GetTopWindow(hwnd))
                {
                    topmostHwnd = hwnd;
                }

                hwnd = NativeMethods.GetWindow(hwnd, NativeMethods.GetWindowType.GW_HWNDNEXT);
            }

            return topmostHwnd;
        }

        //public static IntPtr WindowFromPoint(int x,int y)
        //{
        //    IntPtr wnd = WinFormsUtilities.NativeMethods.WindowFromPoint(x,y);
        //    if(wnd != IntPtr.Zero)
        //    {
        //        //IntPtr pWnd = GetTopWindow(wnd);
        //        //if(pWnd != IntPtr.Zero)
        //        {
        //            Point p = new Point(x,y);
        //            NativeMethods.ScreenToClient(wnd, ref p);
        //            IntPtr cwnd = NativeMethods.ChildWindowFromPointEx(wnd, p, (uint)(WinFormsUtilities.NativeMethods.WindowFromPointFlags.CWP_SKIPTRANSPARENT | WinFormsUtilities.NativeMethods.WindowFromPointFlags.CWP_SKIPINVISIBLE));
        //            if (wnd == cwnd)
        //            {
        //                return wnd;
        //            }
        //            else
        //            {
        //                List<IntPtr> result = new List<IntPtr>();
        //                GCHandle listHandle = GCHandle.Alloc(result);
        //                try
        //                {
        //                    result.Add(cwnd);
        //                    NativeMethods.EnumWindowsProc childProc = new NativeMethods.EnumWindowsProc(NativeMethods.EnumWindow);
        //                    NativeMethods.EnumChildWindows(wnd, childProc, GCHandle.ToIntPtr(listHandle));
        //                }
        //                finally
        //                {
        //                if (listHandle.IsAllocated)
        //                    listHandle.Free();
        //                }
        //                return result[0];
        //            }
        //        }
        //        //else
        //        //{
        //        //    return wnd;
        //        //}

        //    }
        //    return IntPtr.Zero;
        //}
    }

}