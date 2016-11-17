//-----------------------------------------------------------------------------------------------------------------------------//
// Author      : solz                                                                                                          //
// Created     : 04.11.2016 09:35:39                                                                                           //
// Last Change : 17.11.2016 09:30:42                                                                                           //
// Description : <!!! Generated standard description for class MyRenderer !!!>                                          //
//-----------------------------------------------------------------------------------------------------------------------------//

using System;
using System.Windows;
using SD = System.Drawing;
using WpfUserControl;
using SlimDX;
using SlimDX.DirectInput;
using SlimDX.Multimedia;
using SlimDX.RawInput;
using Dx9 = SlimDX.Direct3D9;

namespace DirectXTest
{
    public class MyRenderer : IRenderEngine
    {
        #region Private Fields **************************************************************************************************

        private DirectxControl _control;
        private Dx9.Texture _texture;
        private Dx9.Texture _shipTexture;
        private Dx9.Texture _circleTexture;
        private Dx9.Sprite _sprite;
        private Dx9.Sprite _shipSprite;
        private Dx9.ImageInformation _imageInformation;
        private Dx9.Font _font;

        private Vector3 _shipVector3;
        private Vector3 _mapVector3;
        private float _rotation;
        private Vector3 _mousePosition;

        #endregion

        #region Public Properties ***********************************************************************************************

        #endregion

        #region Constructor(s) **************************************************************************************************

        public MyRenderer()
        {
            _texture = null;
            _font = null;
            _shipVector3 = new Vector3(0, 0, 0);
            _mapVector3 = new Vector3(0, 0, 100);
            _mousePosition = new Vector3(0, 0, 0);
            _rotation = 0f;
        }
        #endregion

        #region Methods *********************************************************************************************************

        #region Public Methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void OnDeviceCreated(object sender, EventArgs e)
        {

        }

        public void OnDeviceDestroyed(object sender, EventArgs e)
        {
            if (_sprite != null && !_sprite.Disposed)
            {
                _sprite.Dispose();
            }
            if (_shipSprite != null && !_shipSprite.Disposed)
            {
                _shipSprite.Dispose();
            }
            if (_texture != null && !_texture.Disposed)
            {
                _texture.Dispose();
                _texture = null;
            }
            if (_font != null && !_font.Disposed)
            {
                _font.Dispose();
                _font = null;
            }
        }

        public void OnDeviceLost(object sender, EventArgs e)
        {
            if (_sprite != null && !_sprite.Disposed)
            {
                _sprite.Dispose();
            }
            if (_shipSprite != null && !_shipSprite.Disposed)
            {
                _shipSprite.Dispose();
            }
            if (_texture != null && !_texture.Disposed)
            {
                _texture.Dispose();
                _texture = null;
            }
            if (_font != null && !_font.Disposed)
            {
                _font.Dispose();
                _font = null;
            }
        }

        public void OnDeviceReset(object sender, EventArgs e)
        {
            _control = sender as DirectxControl;
            if (_control == null)
                throw new ArgumentNullException(nameof(sender));

            _sprite?.Dispose();
            _sprite = new Dx9.Sprite(_control.Device);

            _shipSprite?.Dispose();
            _shipSprite = new Dx9.Sprite(_control.Device);

            if (_texture == null)
                _texture = Dx9.Texture.FromFile(_control.Device, "Ressources\\maxresdefault.jpg", _control.Device.Viewport.Width, _control.Device.Viewport.Height, 0, Dx9.Usage.None, Dx9.Format.Unknown, Dx9.Pool.Default, Dx9.Filter.Default, Dx9.Filter.Default, 0, out _imageInformation);
            if (_shipTexture == null)
                _shipTexture = Dx9.Texture.FromFile(_control.Device, "Ressources\\Voyager.png", 200, 200, 0, Dx9.Usage.None, Dx9.Format.Unknown, Dx9.Pool.Default, Dx9.Filter.Default, Dx9.Filter.Default, 0);
            if (_circleTexture == null)
                _circleTexture = Dx9.Texture.FromFile(_control.Device, "Ressources\\200px-65537-gon.svg.png", 20, 20, 0, Dx9.Usage.None, Dx9.Format.Unknown, Dx9.Pool.Default, Dx9.Filter.Default, Dx9.Filter.Default, 0);
            if (_font == null)
            {
                SD.Font f = new SD.Font("Arial", 20f, SD.FontStyle.Regular);

                _font = new Dx9.Font(_control.Device, f);
            }
        }

        public void OnMainLoop(object sender, EventArgs e)
        {

            _sprite.Begin(Dx9.SpriteFlags.AlphaBlend);

            _font.DrawString(_sprite, "Hello World", 0, 0, SD.Color.White);
            _sprite.Draw(_circleTexture, Vector3.Zero, _mousePosition, new Color4(SD.Color.White));

            _sprite.End();

            using (Dx9.Sprite s = new Dx9.Sprite(_control.Device))
            {
                s.Begin(Dx9.SpriteFlags.AlphaBlend);

                s.Draw(_texture, Vector3.Zero, Vector3.Zero, new Color4(SD.Color.White));

                UpdateCamera();

                s.End();
            }

            _shipSprite.Begin(Dx9.SpriteFlags.AlphaBlend);

            Matrix matrix = Matrix.Transformation2D(new Vector2(0, 0), 0.0f, new Vector2(1f, 1f),
                new Vector2(_shipVector3.X + 100f, _shipVector3.Y + 100f), _rotation, new Vector2(0f, 0f));

            _shipSprite.Transform = matrix;

            _shipSprite.Draw(_shipTexture, Vector3.Zero, _shipVector3, SD.Color.White);

            UpdateCamera();
            _shipSprite.End();
        }


        private void UpdateCamera()
        {
            _control.Device.SetTransform(Dx9.TransformState.Projection,
                Matrix.PerspectiveFovLH((float)Math.PI / 3, (float)_control.Device.Viewport.Width / _control.Device.Viewport.Height, 0.1f, 2000f));
            _control.Device.SetTransform(Dx9.TransformState.View,
                Matrix.LookAtLH(_mapVector3, new Vector3(_mapVector3.X, _mapVector3.Y, 0), new Vector3(0, -1, 0)));
        }

        public void ZoomPlus()
        {
            if (Math.Abs(_mapVector3.Z - 2000) < 0.1)
                return;

            _mapVector3.Z += 2;
            _control.ForceRendering();
        }

        public void ZoomMinus()
        {
            if (Math.Abs(_mapVector3.Z) < 0.1)
                return;

            _mapVector3.Z -= 2;
            _control.ForceRendering();
        }

        public void MapUp()
        {
            _mapVector3.Y -= 2;
            _control.ForceRendering();
        }

        public void MapLeft()
        {
            _mapVector3.X -= 2;
            _control.ForceRendering();
        }

        public void MapRight()
        {
            _mapVector3.X += 2;
            _control.ForceRendering();
        }

        public void MapDown()
        {
            _mapVector3.Y += 2;
            _control.ForceRendering();
        }

        public void ShipUp()
        {
            _shipVector3.Y -= 2;
            _control.ForceRendering();
        }

        public void ShipLeft()
        {
            _shipVector3.X -= 2;
            _control.ForceRendering();
        }

        public void ShipRight()
        {
            _shipVector3.X += 2;
            _control.ForceRendering();
        }

        public void ShipDown()
        {
            _shipVector3.Y += 2;
            _control.ForceRendering();
        }

        public void ShipRotationLeft()
        {
            _rotation -= 0.1f;

            Console.WriteLine(_rotation);
            _control.ForceRendering();
        }

        public void ShipRotationRight()
        {
            _rotation += 0.1f;
            _control.ForceRendering();
        }
        #endregion

        #region Private Methods +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        #endregion

        #endregion

        public void SetMousePosition(Point posPoint)
        {
            Console.WriteLine(posPoint);
            #region first try

            /*_mousePosition.X = _mapVector3.X + (float)posPoint.X - _control.Device.Viewport.Width / 2;
            _mousePosition.Y = _mapVector3.Y + (float)posPoint.Y - _control.Device.Viewport.Height / 2;*/

            /*Matrix view = _control.Device.GetTransform(Dx9.TransformState.View);
            Matrix projection = _control.Device.GetTransform(Dx9.TransformState.Projection);
            Console.WriteLine(Matrix.LookAtLH(_mapVector3, new Vector3(_mapVector3.X, _mapVector3.Y, 0), new Vector3(0, -1, 0)));
            Console.WriteLine(Matrix.PerspectiveFovLH((float)Math.PI / 3, (float)_control.Device.Viewport.Width / _control.Device.Viewport.Height, 0f, 2000f));
            Console.WriteLine("World     : " + _control.Device.GetTransform(Dx9.TransformState.World));
            Console.WriteLine("View      : " + view);
            Console.WriteLine("Projection: " + projection);
            

            Matrix worldview = Matrix.Multiply(Matrix.Identity,
                Matrix.LookAtLH(_mapVector3, new Vector3(_mapVector3.X, _mapVector3.Y, 0), new Vector3(0, -1, 0)));//_control.Device.GetTransform(Dx9.TransformState.View));
            Matrix worldviewprojection = Matrix.Multiply(worldview,
                Matrix.PerspectiveFovLH((float) Math.PI/3,(float) _control.Device.Viewport.Width/_control.Device.Viewport.Height, 0f, 2000f));//_control.Device.GetTransform(Dx9.TransformState.Projection));
            worldviewprojection.Invert();

            float pointX = (float)((2.0 * ((float)posPoint.X) / _control.Device.Viewport.Width) - 1.0f);
            float pointY = (float)((2.0 * (((float)posPoint.Y) / _control.Device.Viewport.Height)) - 1.0f) * -1.0f;
            Vector3 orig = new Vector3(pointX, pointY, 0.0f);
            Vector3 far  = new Vector3(pointX, pointY, 1.0f);

            //This gets mouse position on near plane
            Vector3 origin = Vector3.TransformCoordinate(orig, worldviewprojection);
            
            //This gets mouse position on far plane
            Vector3 posfar = Vector3.TransformCoordinate(far, worldviewprojection);

            _mousePosition = origin;

            Console.WriteLine("Origin: " + _mousePosition);
            Console.WriteLine("-------------------------");*/
            #endregion

            #region second try

            /*int screenWidth = _control.Device.Viewport.Width;
            int screenHeight = _control.Device.Viewport.Height;
            float aspectRatio = screenWidth / (float)screenHeight;
            float fov = (float)Math.PI / 3f;
            float near = 0.1f;
            float far = 2000f;

            float nx = (float)posPoint.X / (screenWidth / 2 - 1) / aspectRatio;
            float ny = (1 - (float)posPoint.Y) / (screenHeight / 2f);

            float ratioX = (float)Math.Tan(fov / 2) * nx;
            float ratioY = (float)Math.Tan(fov / 2) * ny;

            Vector3 P1 = new Vector3();
            Vector3 P2 = new Vector3();

            Vector3 P3 = new Vector3();

            P1.X = ratioX * near;
            P1.Y = ratioY * near;
            P1.Z = near;


            P2.X = ratioX * far;
            P2.Y = ratioY * far;
            P2.Z = far;*/

            /*P3.X = ratioX*_mapVector3.Z + _mapVector3.X;//-screenWidth/2f;
            P3.Y = ratioY*_mapVector3.Z*-1 + _mapVector3.Y;//-screenHeight/2f;

            _mousePosition = P3;

            Console.WriteLine(posPoint);
            Console.WriteLine(_mousePosition);
            Console.WriteLine("-------------------------");*/
            #endregion

            #region third try

            /*Matrix worldview = Matrix.Multiply(Matrix.Identity,_control.Device.GetTransform(Dx9.TransformState.View));
            Matrix worldviewprojection = Matrix.Multiply(worldview,_control.Device.GetTransform(Dx9.TransformState.Projection));

            Ray ray = GetPickRay((float)posPoint.X, (float)posPoint.Y, _control.Device.Viewport.Width, _control.Device.Viewport.Height, worldviewprojection);

            Vector3 result = IntersectLine(ray.Position, ray.Position + ray.Direction);
            _mousePosition = result;

            Console.WriteLine(_mousePosition);*/
            #endregion

            #region fours try

            float m_iClientSizeW = _control.Device.Viewport.Width;
            float m_iClientSizeH = _control.Device.Viewport.Height;

            Matrix worldview = _control.Device.GetTransform(Dx9.TransformState.View);
            Matrix worldviewprojection = Matrix.Multiply(worldview, _control.Device.GetTransform(Dx9.TransformState.Projection));

            Dx9.Viewport vp = _control.Device.Viewport;

            // Unproject near plane
            Vector3 vNear = new Vector3((float)posPoint.X, (float)posPoint.Y, 0);
            Vector3 vFar = new Vector3((float)posPoint.X, (float)posPoint.Y, 1);
            Vector3 vNearW = Vector3.Unproject(vNear,
            0, 0,
            m_iClientSizeW, m_iClientSizeH,
            0, 1, Matrix.Identity * worldviewprojection);

            Vector3 vFarW = Vector3.Unproject(vFar,
            0, 0,
            m_iClientSizeW, m_iClientSizeH,
            0, 1, Matrix.Identity * worldviewprojection);

            Ray ray = new Ray(vNearW, vFarW - vNearW);

            Console.WriteLine(ray.Position);
            Console.WriteLine(ray.Direction);
            
            #endregion

            #region five try
            /*Matrix worldview = _control.Device.GetTransform(Dx9.TransformState.View);
            Matrix worldviewprojection = Matrix.Multiply(worldview, _control.Device.GetTransform(Dx9.TransformState.Projection));

            Vector3 worldSpaceCoordinates = new Vector3((float)posPoint.X, (float)posPoint.Y, 0);

            Dx9.Viewport vp = _control.Device.Viewport;
            Vector3 screenCoords = Vector3.Project(worldSpaceCoordinates, vp.X, vp.Y, vp.Width, vp.Height, vp.MinZ, vp.MaxZ, worldviewprojection);

            _mousePosition = screenCoords;

            Console.WriteLine(_mousePosition);*/
            #endregion

            Console.WriteLine("--------------------------------------");

            _control.ForceRendering();
        }

        public Vector3 IntersectLine(Vector3 a, Vector3 b)
        {
            Vector3 ba = b - a;
            float nDotA = Vector3.Dot(new Vector3(0, 0, 1), a);
            float nDotBA = Vector3.Dot(new Vector3(0, 0, 1), ba);

            float blub = (0 - nDotA) / nDotBA;


            return a + (blub * ba);
        }


        public static Ray GetPickRay(float screenX, float screenY, float viewportWidth, float viewportHeight, Matrix viewProjectionMatrix)
        {
            return GetPickRay(screenX, screenY, 0, 0, viewportWidth, viewportHeight, viewProjectionMatrix);
        }

        public static Ray GetPickRay(float screenX, float screenY, float viewportX, float viewportY, float viewportWidth, float viewportHeight, Matrix viewProjectionMatrix)
        {
            Ray ray = new Ray(new Vector3(viewportWidth / 2.0f, viewportHeight / 2.0f, 0.5f), new Vector3(screenX, screenY, 1));
            ray.Position = Vector3.Unproject(ray.Position, viewportX, viewportY, viewportWidth, viewportHeight, 0.1f, 2000f, viewProjectionMatrix);
            ray.Direction = Vector3.Unproject(ray.Direction, viewportX, viewportY, viewportWidth, viewportHeight, 0.1f, 2000f, viewProjectionMatrix);
            ray.Direction = Vector3.Subtract(ray.Direction, ray.Position);
            ray.Direction.Normalize();

            return ray;
        }
    }
}