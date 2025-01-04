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

        private RenderTarget2D _renderTarget;
        private Rectangle _renderTargetDestination;
        private Texture2D _bitmapTexture; // Your UltralightNet raw bitmap data (loaded as a Texture2D)

        private KeyboardState _previousKeyboardState;
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
            AppCoreMethods.ulEnableDefaultLogger("./ulLog.txt");
            AppCoreMethods.ulEnablePlatformFileSystem("./Content");
            AppCoreMethods.SetPlatformFontLoader();

            // Create Renderer
            var cfg = new ULConfig();
            _renderer = ULPlatform.CreateRenderer(cfg);

            // Create View
            ULViewConfig viewConfig = new ULViewConfig();
            viewConfig.IsTransparent = true;
            _view = _renderer.CreateView((uint)GraphicsDevice.Viewport.Bounds.Width, (uint)GraphicsDevice.Viewport.Bounds.Height, viewConfig);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height);
            _renderTargetDestination = GetRenderTargetDestination(
                new Point(GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height),
                _renderTarget.Width,
                _renderTarget.Height
            );

            // Load an HTML page
            //_view.HTML = "<html><body><h1>Hello, Ultralight in MonoGame!</h1></body></html>";
            //_view.URL = "https://www.google.fr/";
            _view.URL = "file:///helloworld.html";
        }

        private void MouseEventsToUltralight()
        {
            var mouseState = Mouse.GetState();

            // Move
            ULMouseEvent uLMouseMovedEvent = new ULMouseEvent();
            uLMouseMovedEvent.X = (int)(mouseState.X / _view.DeviceScale);
            uLMouseMovedEvent.Y = (int)(mouseState.Y / _view.DeviceScale);
            uLMouseMovedEvent.Type = ULMouseEventType.MouseMoved;
            _view.FireMouseEvent(uLMouseMovedEvent);

            // Left button
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                ULMouseEvent uLMouseEvent = new ULMouseEvent();
                uLMouseEvent.X = (int)(mouseState.X / _view.DeviceScale);
                uLMouseEvent.Y = (int)(mouseState.Y / _view.DeviceScale);

                uLMouseEvent.Type = ULMouseEventType.MouseDown;
                uLMouseEvent.Button = ULMouseEventButton.Left;
                _view.FireMouseEvent(uLMouseEvent);
            }
            else if (mouseState.LeftButton == ButtonState.Released)
            {
                ULMouseEvent uLMouseEvent = new ULMouseEvent();
                uLMouseEvent.X = (int)(mouseState.X / _view.DeviceScale);
                uLMouseEvent.Y = (int)(mouseState.Y / _view.DeviceScale);

                uLMouseEvent.Type = ULMouseEventType.MouseUp;
                uLMouseEvent.Button = ULMouseEventButton.Left;
                _view.FireMouseEvent(uLMouseEvent);
            }

            // Scroll
            if (mouseState.ScrollWheelValue != 0)
            {
                ULScrollEvent uLScrollEvent = new ULScrollEvent();
                uLScrollEvent.DeltaY = (int)(mouseState.ScrollWheelValue);
                _view.FireScrollEvent(uLScrollEvent);
            }
        }

        private void KeyboardEventsToUltralight()
        {
            // Capture current and previous keyboard states
            var keyboardState = Keyboard.GetState();
            var previousKeyboardState = _previousKeyboardState; // Store and update this every frame

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                bool isKeyDown = keyboardState.IsKeyDown(key);
                bool wasKeyDown = previousKeyboardState.IsKeyDown(key);

                // Key Pressed Event
                if (isKeyDown && !wasKeyDown)
                {
                    FireUltralightKeyEvent(key, ULKeyEventType.RawKeyDown);
                }

                // Key Released Event
                if (!isKeyDown && wasKeyDown)
                {
                    FireUltralightKeyEvent(key, ULKeyEventType.KeyUp);
                }

                // Character Input (optional, based on your use case)
                if (isKeyDown)
                {
                    FireUltralightKeyEvent(key, ULKeyEventType.Char);
                }
            }

            // Update previous keyboard state
            _previousKeyboardState = keyboardState;
        }

        private void FireUltralightKeyEvent(Keys key, ULKeyEventType eventType)
        {
            string text = GetKeyText(key); // Convert Keys to a printable character or string (e.g., "A", "1", etc.)

            ULKeyEvent ulKeyEvent = ULKeyEvent.Create(
                eventType, 0, (int)key, (int)key, text, text, false, false, false);

            _view.FireKeyEvent(ulKeyEvent);
        }

        private string GetKeyText(Keys key)
        {
            // Map Keys to their printable text representation.
            // Example for alphanumeric keys and basic symbols. Expand as needed.
            if (key >= Keys.A && key <= Keys.Z)
            {
                return key.ToString(); // Example: Keys.A -> "A"
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                return ((char)('0' + (key - Keys.D0))).ToString(); // Example: Keys.D0 -> "0"
            }
            else if (key == Keys.Space)
            {
                return " ";
            }

            // Add more mappings as needed for other keys (e.g., punctuation, special symbols)
            return "";
        }

        protected override void Update(GameTime gameTime)
        {
            // Update Ultralight
            MouseEventsToUltralight();
            KeyboardEventsToUltralight();
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

            // Allocate managed memory
            byte[] managedPixels = new byte[dataLength];

            // Copy and adjust pixel data (swapping R and B channels)
            for (int i = 0; i < dataLength; i += 4)
            {
                managedPixels[i] = bytePointer[i + 2]; // Blue to Red
                managedPixels[i + 1] = bytePointer[i + 1]; // Green unchanged
                managedPixels[i + 2] = bytePointer[i]; // Red to Blue
                managedPixels[i + 3] = bytePointer[i + 3]; // Alpha unchanged
            }

            // Set pixel data to texture
            texture.SetData(managedPixels);

            return texture;
        }

        protected override unsafe void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Render Ultralight content
            _renderer.Render();

            // Get Surface
            ULSurface surface = _view.Surface ?? throw new Exception("Surface not found, did you perhaps set ViewConfig.IsAccelerated to true?");

            // Get Bitmap
            ULBitmap bitmap = surface.Bitmap;
            try
            {
                _bitmapTexture?.Dispose(); // Dispose of the old texture to avoid leaks
                _bitmapTexture = CreateTextureFromBytePointer(GraphicsDevice, bitmap.RawPixels, (int)bitmap.Width, (int)bitmap.Height);
            }
            finally
            {
                bitmap.Dispose(); // Free bitmap memory
            }

            // Draw your bitmap onto the render target
            _spriteBatch.Begin();
            _spriteBatch.Draw(_bitmapTexture, Vector2.Zero, Color.White);
            _spriteBatch.End();

            // Restore the default render target
            GraphicsDevice.SetRenderTarget(null);

            // Draw the render target to the screen
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget, _renderTargetDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
