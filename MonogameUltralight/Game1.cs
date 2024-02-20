using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using UltralightNet;
using UltralightNet.AppCore;

namespace MonogameUltralight
{
    public class Game1 : Game
    {
        private UltralightNet.View _view;
        private UltralightNet.Renderer _renderer;

        RenderTarget2D renderTarget;
        Rectangle renderTargetDestination;
        Texture2D _bitmapTexture; // Your UltralightNet raw bitmap data (loaded as a Texture2D)

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set Font Loader
            AppCoreMethods.SetPlatformFontLoader();

            // Create Renderer
            var cfg = new ULConfig();
            _renderer = ULPlatform.CreateRenderer(cfg);

            // Create View
            _view = _renderer.CreateView((uint)GraphicsDevice.Viewport.Bounds.Width, (uint)GraphicsDevice.Viewport.Bounds.Height);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height);
            renderTargetDestination = GetRenderTargetDestination(
                new Point(GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height),
                renderTarget.Width,
                renderTarget.Height
            );

            // Load an HTML page
            //_view.HTML = "<html><body><h1>Hello, Ultralight in MonoGame!</h1></body></html>";
            _view.URL = "https://www.google.fr/";
        }

        private void MouseEventsToUltralight()
        {
            var mouseState = Mouse.GetState();

            ULMouseEvent uLMouseEvent = new ULMouseEvent();
            uLMouseEvent.X = mouseState.X;
            uLMouseEvent.Y = mouseState.Y;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                uLMouseEvent.Type = ULMouseEventType.MouseDown;
                uLMouseEvent.Button = ULMouseEventButton.Left;
            }

            _view.FireMouseEvent(uLMouseEvent);
        }

        protected override void Update(GameTime gameTime)
        {
            // Update Ultralight
            MouseEventsToUltralight();
            _renderer.Update();

            base.Update(gameTime);
        }

        private Rectangle GetRenderTargetDestination(Point screenResolution, int rtWidth, int rtHeight)
        {
            float screenAspectRatio = (float)screenResolution.X / screenResolution.Y;
            float rtAspectRatio = (float)rtWidth / rtHeight;

            int destWidth, destHeight;
            if (screenAspectRatio > rtAspectRatio)
            {
                destWidth = screenResolution.X;
                destHeight = (int)(rtHeight * (screenResolution.X / (float)rtWidth));
            }
            else
            {
                destHeight = screenResolution.Y;
                destWidth = (int)(rtWidth * (screenResolution.Y / (float)rtHeight));
            }

            int destX = (screenResolution.X - destWidth) / 2;
            int destY = (screenResolution.Y - destHeight) / 2;

            return new Rectangle(destX, destY, destWidth, destHeight);
        }

        private static unsafe Texture2D CreateTextureFromBytePointer(GraphicsDevice gd, byte* bytePointer, int width, int height)
        {
            // Create a new Texture2D object
            Texture2D texture = new Texture2D(gd, width, height);

            int dataLength = width * height * 4; // RGBA format

            // Your byte pointer (stream) containing pixel data
            byte[] stream = new byte[dataLength];

            for (int i = 0; i < dataLength; i += 4)
            {
                // Swap red and blue channels
                byte red = bytePointer[i];
                byte blue = bytePointer[i + 2];
                stream[i] = blue;
                stream[i + 2] = red;

                // Copy green and alpha channels
                stream[i + 1] = bytePointer[i + 1];
                stream[i + 3] = bytePointer[i + 3];
            }

            // Copy data from your byte pointer to the Texture2D
            texture.SetData(stream);

            return texture;
        }

        protected override unsafe void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Render Ultralight content
            _renderer.Render();

            // Get Surface
            ULSurface surface = _view.Surface ?? throw new Exception("Surface not found, did you perhaps set ViewConfig.IsAccelerated to true?");

            // Get Bitmap
            ULBitmap bitmap = surface.Bitmap;
            _bitmapTexture = CreateTextureFromBytePointer(GraphicsDevice, bitmap.RawPixels, (int)bitmap.Width, (int)bitmap.Height);

            // Draw your bitmap onto the render target
            _spriteBatch.Begin();
            _spriteBatch.Draw(_bitmapTexture, Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Restore the default render target
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Draw the render target to the screen
            _spriteBatch.Begin();
            _spriteBatch.Draw(renderTarget, renderTargetDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
