using System.Runtime.InteropServices;
using System;

namespace DxLib {
    public static partial class DX {
        public struct Color {
            internal uint Co;

            [DllImport("DxLibW.dll", EntryPoint = "dx_GetColor", CharSet = CharSet.Unicode)]
            extern static uint dx_GetColor_x86(int Red, int Green, int Blue);
            [DllImport("DxLibW_x64.dll", EntryPoint = "dx_GetColor", CharSet = CharSet.Unicode)]
            extern static uint dx_GetColor_x64(int Red, int Green, int Blue);
            public static uint GetColor(int Red, int Green, int Blue) =>
                Environment.Is64BitProcess ? dx_GetColor_x64(Red, Green, Blue) : dx_GetColor_x86(Red, Green, Blue);

            public Color(int Red, int Green, int Blue) {
                Co = GetColor(Red, Green, Blue);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RECT {
            [FieldOffset(0)] public int Left;
            [FieldOffset(4)] public int Top;
            [FieldOffset(8)] public int Right;
            [FieldOffset(12)] public int Bottom;

            public RECT(int l, int t, int r, int b) {
                Left = l;
                Top = t;
                Right = r;
                Bottom = b;
            }

            public RECT(int all) {
                Left = all;
                Top = all;
                Right = all;
                Bottom = all;
            }
        };

        [DllImport("DxLibW.dll", EntryPoint = "dx_ScreenFlip", CharSet = CharSet.Unicode)]
        extern static int dx_ScreenFlip_x86();
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_ScreenFlip", CharSet = CharSet.Unicode)]
        extern static int dx_ScreenFlip_x64();
        public static Result ScreenFlip() =>
            (Result)(Environment.Is64BitProcess ? dx_ScreenFlip_x64() : dx_ScreenFlip_x86());

        [DllImport("DxLibW.dll", EntryPoint = "dx_ClearDrawScreen", CharSet = CharSet.Unicode)]
        extern static int dx_ClearDrawScreen_x86(out RECT ClearRect);
        [DllImport("DxLibW_x64.dll", EntryPoint = "dx_ClearDrawScreen", CharSet = CharSet.Unicode)]
        extern static int dx_ClearDrawScreen_x64(out RECT ClearRect);
        public static Result ClearDrawScreen(out RECT ClearRect) =>
            (Result)(Environment.Is64BitProcess ? dx_ClearDrawScreen_x64(out ClearRect) : dx_ClearDrawScreen_x86(out ClearRect));

        public static Result ClearDrawScreen() {
            RECT temp = new RECT(-1);
            return ClearDrawScreen(out temp);
        }
    }
}
