using System.Runtime.InteropServices;
using System;

namespace DxLib {
    public static partial class DX {
        [DllImport("DxLibW.dll", EntryPoint = "dx_SetMouseDispFlag", CharSet = CharSet.Unicode)]
        extern static int dx_SetMouseDispFlag_x86([MarshalAs(UnmanagedType.Bool)] bool DispFlag);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetMouseDispFlag", CharSet = CharSet.Unicode)]
        extern static int dx_SetMouseDispFlag_x64([MarshalAs(UnmanagedType.Bool)] bool DispFlag);
        public static Result SetMouseDispFlag(bool DispFlag) =>
            (Result)(Environment.Is64BitProcess ? dx_SetMouseDispFlag_x64(DispFlag) : dx_SetMouseDispFlag_x86(DispFlag));

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMousePoint", CharSet = CharSet.Unicode)]
        extern static int dx_GetMousePoint_x86(out int XBuf, out int YBuf);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMousePoint", CharSet = CharSet.Unicode)]
        extern static int dx_GetMousePoint_x64(out int XBuf, out int YBuf);
        public static Result GetMousePoint(out int XBuf, out int YBuf) =>
            (Result)(Environment.Is64BitProcess ? dx_GetMousePoint_x64(out XBuf, out YBuf) : dx_GetMousePoint_x86(out XBuf, out YBuf));

        [DllImport("DxLibW.dll", EntryPoint = "dx_SetMousePoint", CharSet = CharSet.Unicode)]
        extern static int dx_SetMousePoint_x86(int PointX, int PointY);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetMousePoint", CharSet = CharSet.Unicode)]
        extern static int dx_SetMousePoint_x64(int PointX, int PointY);
        public static Result SetMousePoint(int PointX, int PointY) =>
            (Result)(Environment.Is64BitProcess ? dx_SetMousePoint_x64(PointX, PointY) : dx_SetMousePoint_x86(PointX, PointY));

        public enum MouseInput {
            Left = 1, Right = 2, Middle = 4, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = 32, Seven = 64, Eight = 128
        }

        public const int MOUSE_INPUT_LOG_DOWN = 0;
        public const int MOUSE_INPUT_LOG_UP = 1;

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseInput", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInput_x86();
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseInput", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInput_x64();
        public static MouseInput GetMouseInput() =>
            (MouseInput)(Environment.Is64BitProcess ? dx_GetMouseInput_x64() : dx_GetMouseInput_x86());

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseWheelRotVol", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseWheelRotVol_x86(int CounterReset);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseWheelRotVol", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseWheelRotVol_x64(int CounterReset);
        public static int GetMouseWheelRotVol() {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseWheelRotVol_x86(TRUE);
            } else {
                return dx_GetMouseWheelRotVol_x64(TRUE);
            }
        }
        public static int GetMouseWheelRotVol(int CounterReset) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseWheelRotVol_x86(CounterReset);
            } else {
                return dx_GetMouseWheelRotVol_x64(CounterReset);
            }
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseHWheelRotVol", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseHWheelRotVol_x86(int CounterReset);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseHWheelRotVol", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseHWheelRotVol_x64(int CounterReset);
        public static int GetMouseHWheelRotVol() {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseHWheelRotVol_x86(TRUE);
            } else {
                return dx_GetMouseHWheelRotVol_x64(TRUE);
            }
        }
        public static int GetMouseHWheelRotVol(int CounterReset) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseHWheelRotVol_x86(CounterReset);
            } else {
                return dx_GetMouseHWheelRotVol_x64(CounterReset);
            }
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseWheelRotVolF", CharSet = CharSet.Unicode)]
        extern static float dx_GetMouseWheelRotVolF_x86(int CounterReset);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseWheelRotVolF", CharSet = CharSet.Unicode)]
        extern static float dx_GetMouseWheelRotVolF_x64(int CounterReset);
        public static float GetMouseWheelRotVolF() {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseWheelRotVolF_x86(TRUE);
            } else {
                return dx_GetMouseWheelRotVolF_x64(TRUE);
            }
        }
        public static float GetMouseWheelRotVolF(int CounterReset) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseWheelRotVolF_x86(CounterReset);
            } else {
                return dx_GetMouseWheelRotVolF_x64(CounterReset);
            }
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseHWheelRotVolF", CharSet = CharSet.Unicode)]
        extern static float dx_GetMouseHWheelRotVolF_x86(int CounterReset);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseHWheelRotVolF", CharSet = CharSet.Unicode)]
        extern static float dx_GetMouseHWheelRotVolF_x64(int CounterReset);
        public static float GetMouseHWheelRotVolF() {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseHWheelRotVolF_x86(TRUE);
            } else {
                return dx_GetMouseHWheelRotVolF_x64(TRUE);
            }
        }
        public static float GetMouseHWheelRotVolF(int CounterReset) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseHWheelRotVolF_x86(CounterReset);
            } else {
                return dx_GetMouseHWheelRotVolF_x64(CounterReset);
            }
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseInputLog", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInputLog_x86(out int Button, out int ClickX, out int ClickY, int LogDelete);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseInputLog", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInputLog_x64(out int Button, out int ClickX, out int ClickY, int LogDelete);
        public static int GetMouseInputLog(out int Button, out int ClickX, out int ClickY) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseInputLog_x86(out Button, out ClickX, out ClickY, TRUE);
            } else {
                return dx_GetMouseInputLog_x64(out Button, out ClickX, out ClickY, TRUE);
            }
        }
        public static int GetMouseInputLog(out int Button, out int ClickX, out int ClickY, int LogDelete) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseInputLog_x86(out Button, out ClickX, out ClickY, LogDelete);
            } else {
                return dx_GetMouseInputLog_x64(out Button, out ClickX, out ClickY, LogDelete);
            }
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_GetMouseInputLog2", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInputLog2_x86(out int Button, out int ClickX, out int ClickY, out int LogType, int LogDelete);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetMouseInputLog2", CharSet = CharSet.Unicode)]
        extern static int dx_GetMouseInputLog2_x64(out int Button, out int ClickX, out int ClickY, out int LogType, int LogDelete);
        public static int GetMouseInputLog2(out int Button, out int ClickX, out int ClickY, out int LogType) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseInputLog2_x86(out Button, out ClickX, out ClickY, out LogType, TRUE);
            } else {
                return dx_GetMouseInputLog2_x64(out Button, out ClickX, out ClickY, out LogType, TRUE);
            }
        }
        public static int GetMouseInputLog2(out int Button, out int ClickX, out int ClickY, out int LogType, int LogDelete) {
            if (System.IntPtr.Size == 4) {
                return dx_GetMouseInputLog2_x86(out Button, out ClickX, out ClickY, out LogType, LogDelete);
            } else {
                return dx_GetMouseInputLog2_x64(out Button, out ClickX, out ClickY, out LogType, LogDelete);
            }
        }
    }
}
