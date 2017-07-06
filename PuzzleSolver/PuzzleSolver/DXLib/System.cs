using System.Runtime.InteropServices;
using System;

namespace DxLib {
    public static partial class DX {
        public enum Result {
            Error = -1, Success = 0
        }

        [DllImport("DxLibW.dll", EntryPoint = "dx_DxLib_Init", CharSet = CharSet.Unicode)]
        extern static int dx_DxLib_Init_x86();
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_DxLib_Init", CharSet = CharSet.Unicode)]
        extern static int dx_DxLib_Init_x64();
        public static Result Init() =>
            (Result)(Environment.Is64BitProcess ? dx_DxLib_Init_x64() : dx_DxLib_Init_x86());

        [DllImport("DxLibW.dll", EntryPoint = "dx_DxLib_End", CharSet = CharSet.Unicode)]
        extern static int dx_DxLib_End_x86();
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_DxLib_End", CharSet = CharSet.Unicode)]
        extern static int dx_DxLib_End_x64();
        public static Result Finalize() =>
            (Result)(Environment.Is64BitProcess ? dx_DxLib_End_x64() : dx_DxLib_End_x86());

        [DllImport("DxLibW.dll", EntryPoint = "dx_ProcessMessage", CharSet = CharSet.Unicode)]
        extern static int dx_ProcessMessage_x86();
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_ProcessMessage", CharSet = CharSet.Unicode)]
        extern static int dx_ProcessMessage_x64();
        public static Result ProcessMessage() =>
            (Result)(Environment.Is64BitProcess ? dx_ProcessMessage_x64() : dx_ProcessMessage_x86());
    }
}
