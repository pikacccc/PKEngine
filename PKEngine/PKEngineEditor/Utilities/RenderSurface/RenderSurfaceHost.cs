using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using PKEngineEditor.DllWrappers;

namespace PKEngineEditor.Utilities
{
    public class RenderSurfaceHost : HwndHost
    {
        private readonly int             VK_LBUTTON          = 0x01;
        private readonly int             _width              = 800;
        private readonly int             _height             = 600;
        private          IntPtr          _renderWindowHandle = IntPtr.Zero;
        private          DelayEventTimer _resizeTimer;


        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public int SurfaceId { get; private set; } = ID.INVALID_ID;

        public RenderSurfaceHost(int width, int height)
        {
            _width                =  width;
            _height               =  height;
            _resizeTimer          =  new DelayEventTimer(TimeSpan.FromMilliseconds(250));
            _resizeTimer.Triggers += Resize;
            SizeChanged           += (_, _) => _resizeTimer.Trigger();
        }

        private void Resize(object? sender, DelayEventTimerArgs e)
        {
            e.RepeatEvent = GetAsyncKeyState(VK_LBUTTON) < 0;
            if (!e.RepeatEvent)
            {
                EngineAPI.ResizeRenderSurface(SurfaceId);
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SurfaceId = EngineAPI.CreateRenderSurface(hwndParent.Handle, _width, _height);
            Debug.Assert(ID.IsValid(SurfaceId));
            _renderWindowHandle = EngineAPI.GetRenderHandle(SurfaceId);
            Debug.Assert(_renderWindowHandle != IntPtr.Zero);
            return new HandleRef(this, _renderWindowHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            EngineAPI.RemoveRenderSurface(SurfaceId);
            _renderWindowHandle = IntPtr.Zero;
            SurfaceId           = ID.INVALID_ID;
        }
    }
}