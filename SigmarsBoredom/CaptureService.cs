using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace SigmarsBoredom
{
    public class CaptureService
    {
        #region Imports

        [DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, int Rop);

        [DllImport("gdi32", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDC);

        [DllImport("gdi32", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        private const int SRCCOPY = 13369376; // not sure what this is tbh

        private readonly IntPtr _mainWindowHandle;

        /// <summary>
        /// Builds a capture service capturing from the process with the given name.
        /// </summary>
        /// <param name="processName">Name of the process to capture.</param>
        public CaptureService(string processName)
        {
            var handle = Process.GetProcessesByName(processName).FirstOrDefault()?.MainWindowHandle;
            if (handle == null)
                throw new Exception($"Cannot find process \"{processName}\". Make sure the process is running before starting.");

            _mainWindowHandle = handle.Value;
        }

        /// <summary>
        /// Captures and returns an image of the target process' main window, with an option to get only the specified rectangle portion.
        /// </summary>
        /// <param name="rectangleInWindow">If specified, defines the area of the window that will be captured.</param>
        public Bitmap GetWindowImage(Rectangle? rectangleInWindow = null)
        {
            int srcX, srcY, width, height;

            if (rectangleInWindow == null)
            {
                // Use GetClientRect so that SrcX/SrcY are in client (window-relative) coordinates.
                // GetWindowRect returns screen coordinates, which would cause BitBlt to sample the
                // wrong area when the window is not positioned at (0,0) on the screen.
                var clientRect = GetMainClientRectangle();
                srcX = 0;
                srcY = 0;
                width = clientRect.Right - clientRect.Left;
                height = clientRect.Bottom - clientRect.Top;
            }
            else
            {
                // rectangleInWindow is already in client (window-relative) coordinates.
                srcX = rectangleInWindow.Value.X;
                srcY = rectangleInWindow.Value.Y;
                width = rectangleInWindow.Value.Width;
                height = rectangleInWindow.Value.Height;
            }

            IntPtr handle = GetDC(_mainWindowHandle);

            IntPtr mem = CreateCompatibleDC(handle);

            IntPtr result = CreateCompatibleBitmap(handle, width, height);

            if (result == IntPtr.Zero)
                throw new Exception("Could not create compatible bitmap from the target process' main window.");

            IntPtr oldBmp = SelectObject(mem, result);
            BitBlt(mem, 0, 0, width, height, handle, srcX, srcY, SRCCOPY);
            SelectObject(mem, oldBmp);
            DeleteDC(mem);
            ReleaseDC(_mainWindowHandle, handle);
            Image imgReturn = Image.FromHbitmap(result);

            DeleteObject(oldBmp);
            DeleteObject(handle);
            DeleteObject(mem);

            return (Bitmap)imgReturn;
        }

        /// <summary>
        /// Gets the size of the target process' main window client area in pixels.
        /// </summary>
        public Size GetWindowSize()
        {
            var clientRect = GetMainClientRectangle();
            return new Size(clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
        }

        /// <summary>
        /// Gets the client rectangle of the target process' main window.
        /// Coordinates are always relative to the window's top-left corner (Left=0, Top=0).
        /// </summary>
        private RECT GetMainClientRectangle()
        {
            if (!GetClientRect(_mainWindowHandle, out var clientRect))
                throw new Exception("Unable to get client rectangle");

            return clientRect;
        }

        /// <summary>
        /// Gets the rectangle of the target process' main window in screen coordinates.
        /// </summary>
        private RECT GetMainWindowRectangle()
        {
            if (!GetWindowRect(_mainWindowHandle, out var sourceRectangle))
                throw new Exception("Unable to get window dimensions");

            return sourceRectangle;
        }
    }
}
