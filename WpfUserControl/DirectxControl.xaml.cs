using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Interop;
using SlimDX;
using SlimDX.Direct3D9;

namespace WpfUserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DirectxControl
    {
        #region DLL imports
        // can't figure out how to access remote session status through .NET
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int GetSystemMetrics(int smIndex);
        private const int SmRemotesession = 0x1000;
        #endregion

        #region locals
        // D3D settings
        private Direct3D _direct3D;
        private Direct3DEx _direct3DEx;
        private Device _device;
        private DeviceEx _deviceEx;
        private Surface _backBufferSurface;

        // device settings
        Format _adapterFormat = Format.X8R8G8B8;
        Format _backbufferFormat = Format.A8R8G8B8;
        Format _depthStencilFormat = Format.D16;
        CreateFlags _createFlags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve;
        private PresentParameters _presentParameters;

        private bool _sizeChanged;
        #endregion

        #region Events
        public event EventHandler MainLoop;
        public event EventHandler DeviceCreated;
        public event EventHandler DeviceDestroyed;
        public event EventHandler DeviceLost;
        public event EventHandler DeviceReset;
        #endregion

        #region Properties
        public IRenderEngine RenderEngine { private set; get; }

        public System.Windows.Controls.Image Image { get { return Image3D; } }

        public bool UseDeviceEx { get; private set; }

        public Direct3D Direct3D
        {
            get
            {
                return UseDeviceEx ? _direct3DEx : _direct3D;
            }
        }

        public Device Device
        {
            get
            {
                return UseDeviceEx ? _deviceEx : _device;
            }
        }

        public DirectXStatus DirectXStatus { get; private set; }
        #endregion

        public DirectxControl()
        {
            InitializeComponent();
        }

        public void SetRenderEngine(IRenderEngine renderEngine)
        {
            RenderEngine = renderEngine;

            DeviceCreated += RenderEngine.OnDeviceCreated;
            DeviceDestroyed += RenderEngine.OnDeviceDestroyed;
            DeviceLost += RenderEngine.OnDeviceLost;
            DeviceReset += RenderEngine.OnDeviceReset;
            MainLoop += RenderEngine.OnMainLoop;
        }

        #region Event raisers
        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnMainLoop(EventArgs e)
        {
            MainLoop?.Invoke(this, e);
        }

        protected virtual void OnDeviceCreated(EventArgs e)
        {
            DeviceCreated?.Invoke(this, e);
            ForceRendering();
        }

        protected virtual void OnDeviceDestroyed(EventArgs e)
        {
            DeviceDestroyed?.Invoke(this, e);
        }

        protected virtual void OnDeviceLost(EventArgs e)
        {
            DeviceLost?.Invoke(this, e);
        }

        protected virtual void OnDeviceReset(EventArgs e)
        {
            DeviceReset?.Invoke(this, e);
        }
        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeDirect3D();
            InitializeDevice();

            if (DirectXStatus != DirectXStatus.Available)
            {
                Shutdown();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.HeightChanged || sizeInfo.WidthChanged)
                _sizeChanged = true;

        }

        /// <summary>
        /// Initializes the Direct3D objects and sets the Available flag
        /// </summary>
        private void InitializeDirect3D()
        {
            DirectXStatus = DirectXStatus.Unavailable_Unknown;

            ReleaseDevice();
            ReleaseDirect3D();

            // assume that we can't run at all under terminal services
            if (GetSystemMetrics(SmRemotesession) != 0)
            {
                DirectXStatus = DirectXStatus.Unavailable_RemoteSession;
                return;
            }

            int renderingTier = (RenderCapability.Tier >> 16);
            if (renderingTier < 2)
            {
                DirectXStatus = DirectXStatus.Unavailable_LowTier;
                return;
            }

#if USE_XP_MODE
         _direct3D = new Direct3D();
         UseDeviceEx = false;
#else
            try
            {
                _direct3DEx = new Direct3DEx();
                UseDeviceEx = true;
            }
            catch
            {
                try
                {
                    _direct3D = new Direct3D();
                    UseDeviceEx = false;
                }
                catch (Direct3DX9NotFoundException)
                {
                    DirectXStatus = DirectXStatus.Unavailable_MissingDirectX;
                    return;
                }
                catch
                {
                    DirectXStatus = DirectXStatus.Unavailable_Unknown;
                    return;
                }
            }
#endif

            Result result;

            bool ok = Direct3D.CheckDeviceType(0, DeviceType.Hardware, _adapterFormat, _backbufferFormat, true, out result);
            if (!ok)
            {
                //const int D3DERR_NOTAVAILABLE = -2005530518;
                //if (result.Code == D3DERR_NOTAVAILABLE)
                //{
                //   ReleaseDirect3D();
                //   Available = Status.Unavailable_NotReady;
                //   return;
                //}
                ReleaseDirect3D();
                return;
            }

            ok = Direct3D.CheckDepthStencilMatch(0, DeviceType.Hardware, _adapterFormat, _backbufferFormat, _depthStencilFormat, out result);
            if (!ok)
            {
                ReleaseDirect3D();
                return;
            }

            Capabilities deviceCaps = Direct3D.GetDeviceCaps(0, DeviceType.Hardware);
            if ((deviceCaps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
                _createFlags |= CreateFlags.HardwareVertexProcessing;
            else
                _createFlags |= CreateFlags.SoftwareVertexProcessing;

            DirectXStatus = DirectXStatus.Available;
        }

        /// <summary>
        /// Initializes the Device and starts up the d3dimage object
        /// </summary>
        private void InitializeDevice()
        {
            if (DirectXStatus != DirectXStatus.Available)
                return;

            Debug.Assert(Direct3D != null);

            ReleaseDevice();

            HwndSource hwnd = new HwndSource(0, 0, 0, 0, 0, D3Dimage.PixelWidth, D3Dimage.PixelHeight, "SlimDXControl", IntPtr.Zero);

            _presentParameters = new PresentParameters
            {
                SwapEffect = SwapEffect.Copy,
                DeviceWindowHandle = hwnd.Handle,
                Windowed = true,
                BackBufferWidth = ((int) ActualWidth <= 0) ? 1 : (int) ActualWidth,
                BackBufferHeight = ((int) ActualHeight <= 0) ? 1 : (int) ActualHeight,
                BackBufferFormat = _backbufferFormat,
                AutoDepthStencilFormat = _depthStencilFormat
            };

            try
            {
                if (UseDeviceEx)
                {
                    _deviceEx = new DeviceEx((Direct3DEx)Direct3D, 0,
                       DeviceType.Hardware,
                       hwnd.Handle,
                       _createFlags,
                       _presentParameters);
                }
                else
                {
                    _device = new Device(Direct3D, 0,
                       DeviceType.Hardware,
                       hwnd.Handle,
                       _createFlags,
                       _presentParameters);
                }
            }
            catch (Direct3D9Exception)
            {
                DirectXStatus = DirectXStatus.Unavailable_Unknown;
                return;
            }
            // call the user's ones
            OnDeviceCreated(EventArgs.Empty);
            OnDeviceReset(EventArgs.Empty);

            {
                // is it the case that someone else is nulling these out on us?  seems so
                // this means we need to be careful not to let multiple copies of the delegate get onto the list
                // not sure what else we can do here...
                CompositionTarget.Rendering -= OnRendering;
                D3Dimage.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
                CompositionTarget.Rendering += OnRendering;
                D3Dimage.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
            }

            D3Dimage.Lock();
            _backBufferSurface = Device.GetBackBuffer(0, 0);
            D3Dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _backBufferSurface.ComPointer);
            D3Dimage.Unlock();
        }

        #region Shutdown and Release
        public void Shutdown()
        {
            ReleaseDevice();
            ReleaseDirect3D();
        }

        private void ReleaseDevice()
        {
            if (_device != null)
            {
                if (!_device.Disposed)
                {
                    _device.Dispose();
                    _device = null;
                    OnDeviceDestroyed(EventArgs.Empty);
                }
            }

            if (_deviceEx != null)
            {
                if (!_deviceEx.Disposed)
                {
                    _deviceEx.Dispose();
                    _deviceEx = null;
                    OnDeviceDestroyed(EventArgs.Empty);
                }
            }

            D3Dimage.Lock();
            D3Dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
            D3Dimage.Unlock();

            ReleaseBackBuffer();
        }

        private void ReleaseDirect3D()
        {
            if (_direct3D != null && !_direct3D.Disposed)
            {
                _direct3D.Dispose();
                _direct3D = null;
            }

            if (_direct3DEx != null && !_direct3DEx.Disposed)
            {
                _direct3DEx.Dispose();
                _direct3DEx = null;
            }
        }
        #endregion

        #region Support for controlling on-demand vs. per-frame rendering
        // set this to true when something has changed, to signal that the rendering loop should not be skipped
        private bool _needsRendering;
        public void ForceRendering()
        {
            lock (this)
            {
                _needsRendering = true;
            }
        }

        private bool ResetForceRendering()
        {
            bool ret;
            lock (this)
            {
                ret = _needsRendering;
                _needsRendering = false;
            }
#if USE_ALWAYSRENDER_MODE
         return true;
#else
            return ret;
#endif
        }
        #endregion

        private void OnRendering(object sender, EventArgs e)
        {
            Debug.Assert(D3Dimage.IsFrontBufferAvailable);

            if (DirectXStatus != DirectXStatus.Available)
                return;

            bool needToReset = false;

            try
            {
                if (Direct3D == null)
                {
                    InitializeDirect3D();
                }
                if (Device == null)
                {
                    InitializeDevice();
                    ForceRendering();
                }
                if (Device == null)
                {
                    // device might still be not available, so we'll just have to try again next time around
                    return;
                }

                if (_sizeChanged)
                {
                    _presentParameters.BackBufferWidth = (int)ActualWidth;
                    _presentParameters.BackBufferHeight = (int)ActualHeight;
                    ReleaseBackBuffer();
                    OnDeviceLost(EventArgs.Empty);
                    Device.Reset(_presentParameters);
                    OnDeviceReset(EventArgs.Empty);
                    _sizeChanged = false;
                    ForceRendering();
                }

                bool needsRendering = ResetForceRendering();
                if (needsRendering)
                {
                    D3Dimage.Lock();
                    if (D3Dimage.IsFrontBufferAvailable)
                    {
                        Result result = Device.TestCooperativeLevel();
                        if (result.IsFailure)
                        {
                            // we'll change the status in the needToReset block below
                            DirectXStatus = DirectXStatus.Unavailable_Unknown;
                            throw new Direct3D9Exception("Device.TestCooperativeLevel() failed");
                        }

                        Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(System.Drawing.Color.CornflowerBlue), 1, 0);
                        Device.BeginScene();

                        try
                        {
                            // call the user's method
                            OnMainLoop(EventArgs.Empty);
                        }
                        catch (Direct3D9Exception d3Dex)
                        {
                            if (d3Dex.Message.StartsWith("D3DERR_OUTOFVIDEOMEMORY"))
                            {
                                needToReset = true;
                            }
                            else
                            {
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Caught Exception in SlimDXControl.OnRendering() [d] " + ex);
                        }

                        Device.EndScene();
                        Device.Present();

                    }
                    _backBufferSurface = Device.GetBackBuffer(0, 0);
                    D3Dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _backBufferSurface.ComPointer);
                    D3Dimage.AddDirtyRect(new Int32Rect(0, 0, D3Dimage.PixelWidth, D3Dimage.PixelHeight));
                    D3Dimage.Unlock();
                }
            }
            catch (Direct3D9Exception)
            {
                needToReset = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Caught Exception in SlimDXControl.OnRendering() [a] " + ex);
                
            }

            if (needToReset)
            {
                try
                {
                    InitializeDirect3D();
                    InitializeDevice();
                    if (DirectXStatus != DirectXStatus.Available)
                    {
                        // we were once available (because we were able to enter the OnRender function), but now we're not available
                        // This could be due to a return from Ctrl-Alt-Del under XP and things just aren't ready yet.
                        // Keep everything nulled out and we'll just try the next time around.
                        ReleaseDevice();
                        ReleaseDirect3D();
                        DirectXStatus = DirectXStatus.Available;
                    }
                    else
                    {
                        // we're available now, that's good
                        // force a rendering next time around
                        ForceRendering();
                    }
                }
                catch (Direct3D9Exception ex)
                {
                    MessageBox.Show(ex.Message, "Caught Exception in SlimDXControl.OnRendering() [b] " + ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Caught Exception in SlimDXControl.OnRendering() [c] " + ex);
                }
            }
        }

        #region front & back buffer management
        private void ReleaseBackBuffer()
        {
            if (_backBufferSurface != null && !_backBufferSurface.Disposed)
            {
                _backBufferSurface.Dispose();
                _backBufferSurface = null;

                D3Dimage.Lock();
                D3Dimage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                D3Dimage.Unlock();
            }
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (D3Dimage.IsFrontBufferAvailable)
                {
                    InitializeDevice();
                }
                else
                {
                    CompositionTarget.Rendering -= OnRendering;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Caught Exception in SlimDXControl.OnIsFrontBufferAvailableChanged()");
            }
        }
        #endregion
    }
}