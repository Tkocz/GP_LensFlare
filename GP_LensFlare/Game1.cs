using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GP_LensFlare
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class LensFlareGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState currentKeyboardState = new KeyboardState();

        Matrix View;
        Matrix Projection;
        Vector3 Up;
        Vector3 Offset;
        Quaternion CameraRotation;
        Vector3 Position;
        Quaternion Orientation;
        Matrix ObjectWorld;
        Vector3 YPR;

        Model terrain;

        LensFlareComponent lensFlare;



        public LensFlareGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            Content.RootDirectory = "Content";

            Up = Vector3.Up;
            CameraRotation = Quaternion.Identity;
            Offset = new Vector3(0, 0, 0.1F);
            View = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, 0), Up);
            Position = new Vector3(0, -6, 1);
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 24, 0), Vector3.Up));
            ObjectWorld = Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);



            // Create and add the lensflare component.
            lensFlare = new LensFlareComponent(this);

            Components.Add(lensFlare);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            terrain = Content.Load<Model>("terrain");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 500f);


            // Draw the terrain.
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.Identity;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.LightingEnabled = true;
                    effect.DiffuseColor = new Vector3(1f);
                    effect.AmbientLightColor = new Vector3(0.5f);

                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = Vector3.One;
                    effect.DirectionalLight0.Direction = lensFlare.LightDirection;

                    effect.FogEnabled = true;
                    effect.FogStart = 200;
                    effect.FogEnd = 500;
                    effect.FogColor = Color.CornflowerBlue.ToVector3();
                }

                mesh.Draw();
            }

            // Tell the lensflare component where our camera is positioned.
            lensFlare.View = View;
            lensFlare.Projection = Projection;

            base.Draw(gameTime);
        }
        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            currentKeyboardState = Keyboard.GetState();

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                Position += 0.01f * time * ObjectWorld.Forward;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                Position += 0.01f * time * ObjectWorld.Backward;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                Position += 0.01f * time * ObjectWorld.Left;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                Position += 0.01f * time * ObjectWorld.Right;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                Position += 0.01f * 1f * new Vector3(0, 1, 0);
            }
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                Position += 0.01f * 1f * new Vector3(0, -1, 0);
            }


            YPR = Vector3.Zero;
            float angle = time * 0.001f;

            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                YPR.Y += angle;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                YPR.Y += -angle;
            }
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                YPR.X += angle;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                YPR.X += -angle;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Q))
            {
                YPR.Z += angle;
            }
            if (currentKeyboardState.IsKeyDown(Keys.E))
            {
                YPR.Z += -angle;
            }
        }


        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            var cameraRotation = Quaternion.Lerp(CameraRotation, Orientation, 0.1f);

            Vector3 cameraPosition = Vector3.Transform(Offset, Orientation);
            cameraPosition += Position;

            Vector3 cameraUp = new Vector3(0, 1, 0);
            cameraUp = Vector3.Transform(cameraUp, Orientation);

            View = Matrix.CreateLookAt(cameraPosition, Position, cameraUp);

            CameraRotation = cameraRotation;
            Up = cameraUp;

            Quaternion addRot = Quaternion.CreateFromYawPitchRoll(YPR.X, YPR.Y, YPR.Z);
            addRot.Normalize();
            Orientation *= addRot;
        }
    }
}
