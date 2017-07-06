using System.Runtime.InteropServices;
using System;

namespace DxLib {
    public static partial class DX {
        public enum ChangeScreenResult {
            Ok = 0, Return = 1, Default = 2, RefreshNormal = 3
        }

        public enum Result {
            Error = -1, Successed = 0
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_ChangeWindowMode", CharSet = CharSet.Unicode)]
        extern static int dx_ChangeWindowMode_x86([MarshalAs(UnmanagedType.Bool)] bool Flag);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_ChangeWindowMode", CharSet = CharSet.Unicode)]
        extern static int dx_ChangeWindowMode_x64([MarshalAs(UnmanagedType.Bool)] bool Flag);
        public static ChangeScreenResult ChangeWindowMode(bool Flag) =>
            (ChangeScreenResult)(Environment.Is64BitProcess ? dx_ChangeWindowMode_x64(Flag) : dx_ChangeWindowMode_x86(Flag));

        [DllImport("DxLibW.dll", EntryPoint = "dx_SetMainWindowText", CharSet = CharSet.Unicode)]
        extern static int dx_SetMainWindowText_x86(string WindowText);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetMainWindowText", CharSet = CharSet.Unicode)]
        extern static int dx_SetMainWindowText_x64(string WindowText);
        public static Result SetMainWindowText(string WindowText) =>
            (Result)(Environment.Is64BitProcess ? dx_SetMainWindowText_x64(WindowText) : dx_SetMainWindowText_x86(WindowText));

        [DllImport("DxLibW.dll", EntryPoint = "dx_SetWindowIconID", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowIconID_x86(int ID);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetWindowIconID", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowIconID_x64(int ID);
        public static Result SetWindowIconID(int ID) =>
            (Result)(Environment.Is64BitProcess ? dx_SetWindowIconID_x64(ID) : dx_SetWindowIconID_x86(ID));

        [DllImport("DxLibW.dll", EntryPoint = "dx_SetWindowSizeChangeEnableFlag", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowSizeChangeEnableFlag_x86(int Flag, [MarshalAs(UnmanagedType.Bool)] bool FitScreen);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetWindowSizeChangeEnableFlag", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowSizeChangeEnableFlag_x64(int Flag, [MarshalAs(UnmanagedType.Bool)] bool FitScreen);
        public static Result SetWindowSizeChangeEnableFlag(int Flag, bool FitScreen = true) =>
            (Result)(Environment.Is64BitProcess ? dx_SetWindowSizeChangeEnableFlag_x64(Flag, FitScreen) : dx_SetWindowSizeChangeEnableFlag_x86(Flag, FitScreen));

        [DllImport("DxLibW.dll", EntryPoint = "dx_SetWindowSizeExtendRate", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowSizeExtendRate_x86(double ExRateX, double ExRateY);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_SetWindowSizeExtendRate", CharSet = CharSet.Unicode)]
        extern static int dx_SetWindowSizeExtendRate_x64(double ExRateX, double ExRateY);
        public static Result SetWindowSizeExtendRate(double ExRateX, double ExRateY = -1.0) =>
            (Result)(Environment.Is64BitProcess ? dx_SetWindowSizeExtendRate_x64(ExRateX, ExRateY) : dx_SetWindowSizeExtendRate_x86(ExRateX, ExRateY));
    }
}
