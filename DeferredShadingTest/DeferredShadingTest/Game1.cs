using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DeferredShadingTest
{
    //XNA uses right handed coordinates, +Z is out of the screen.
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Global Variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTargetBinding[] renderTargetBindings;
        const int screenWidth = 600, screenHeight = 450;

        Vector3 cameraPosition = new Vector3(0,4,4);
        float camRotXZ = MathHelper.Pi - MathHelper.PiOver4, camRotXY = MathHelper.Pi - 1.5f * MathHelper.PiOver4;
        Matrix viewMatrix, projectionMatrix, cameraRotation;

        Mesh cubeMesh;
        Texture2D bricks, bricksNormal;

        Effect deferredRenderEffect, clearBufferEffect, directionalLightEffect;
        QuadRenderComponent quadRenderer;
        float Elapsed;
        #endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            graphics.ApplyChanges();
            base.Initialize();
        }

        #region Load
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, screenWidth / screenHeight, 1.0f, 100.0f);

            renderTargetBindings = new RenderTargetBinding[3];
            renderTargetBindings[0] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth16));//Color
            renderTargetBindings[1] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth16));//Normal
            renderTargetBindings[2] = new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, SurfaceFormat.Single, DepthFormat.Depth16));//Depth

            cubeMesh = Mesh.CreateCubeMesh(0.4f);
            bricks = Content.Load<Texture2D>("brickwork-texture");
            bricksNormal = Content.Load<Texture2D>("brickwork_normal-map");

            clearBufferEffect = Content.Load<Effect>("ClearGBuffer");

            deferredRenderEffect = Content.Load<Effect>("DeferredRenderEffect");
            deferredRenderEffect.Parameters["DiffuseTexture"].SetValue(bricks);
            deferredRenderEffect.Parameters["World"].SetValue(Matrix.Identity);
            deferredRenderEffect.Parameters["Projection"].SetValue(projectionMatrix);

            directionalLightEffect = Content.Load<Effect>("DirectionalLightEffect");
            directionalLightEffect.Parameters["DiffuseMap"].SetValue(renderTargetBindings[0].RenderTarget);
            directionalLightEffect.Parameters["NormalMap"].SetValue(renderTargetBindings[1].RenderTarget);
            directionalLightEffect.Parameters["DepthMap"].SetValue(renderTargetBindings[2].RenderTarget);
            directionalLightEffect.Parameters["lightDirection"].SetValue(-Vector3.UnitY);
            directionalLightEffect.Parameters["Color"].SetValue(Color.BlueViolet.ToVector3());
            directionalLightEffect.Parameters["halfPixel"].SetValue(new Vector2(0.5f / screenWidth, 0.5f / screenHeight));

            quadRenderer = new QuadRenderComponent(this);

            //ship = Content.Load<Model>("Models/ship1");
            //volleyBall = Content.Load<Model>("volleyBall");
            //e = new BasicEffect(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion
        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Input();
            base.Update(gameTime);
        }
        private void Input()
        {
            #region Mouse
            MouseState mouseState = Mouse.GetState();
            int dx = mouseState.X - screenWidth / 2;
            int dy = mouseState.Y - screenHeight / 2;
            if (dx != 0 || dy != 0)
            {
                camRotXZ += (float)dx / 128.0f;
                camRotXY += (float)dy / 128.0f;
                camRotXZ %= MathHelper.TwoPi;
                camRotXY %= MathHelper.TwoPi;
                Mouse.SetPosition(screenWidth / 2, screenHeight / 2);
                UpdateView();
            }
            #endregion           
            #region Keyboard
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();
            Vector3 moveVector = new Vector3(0);
            if (keyState.IsKeyDown(Keys.W))
                moveVector.Z -= 1;
            if (keyState.IsKeyDown(Keys.S))
                moveVector.Z += 1;
            if (keyState.IsKeyDown(Keys.A))
                moveVector.X -= 1;
            if (keyState.IsKeyDown(Keys.D))
                moveVector.X += 1;
            if (keyState.IsKeyDown(Keys.E))
                moveVector.Y -= 1;
            if (keyState.IsKeyDown(Keys.Q))
                moveVector.Y += 1;
            if (moveVector.Length() > 0)
            {
                moveVector.Normalize();
                moveVector = Vector3.Transform(moveVector, cameraRotation) * 10 * Elapsed;
                cameraPosition += moveVector;
            }
            #endregion

        }
        private void UpdateView()
        {
            cameraRotation = Matrix.CreateRotationX(camRotXY) * Matrix.CreateRotationY(-camRotXZ);
            Vector3 lookAt = Vector3.Transform(-Vector3.UnitZ, cameraRotation);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + lookAt, Vector3.Transform(Vector3.UnitY, cameraRotation));
        }
        #endregion
        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            initGBuffer();
            foreach (EffectPass pass in deferredRenderEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                cubeMesh.Draw(GraphicsDevice);
                //volleyBall.Draw(Matrix.Identity, viewMatrix, projectionMatrix);
                //ship.Draw(Matrix.CreateScale(.1f), viewMatrix, projectionMatrix);
            }
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            drawDirectionalLight();
            /*e.DiffuseColor = Vector3.UnitX;
            e.Projection = projectionMatrix;
            e.View = viewMatrix;
            e.World = Matrix.Identity;
            foreach (EffectPass pass in e.CurrentTechnique.Passes)
            {
                pass.Apply();
                cubeMesh.Draw(GraphicsDevice);
            }*/

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw((Texture2D)renderTargetBindings[2].RenderTarget, new Vector2(2 * screenWidth / 3, 0), null, Color.White, 0, Vector2.Zero, .33f, SpriteEffects.None, 0);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw((Texture2D)renderTargetBindings[0].RenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, .33f, SpriteEffects.None, 0);
            spriteBatch.Draw((Texture2D)renderTargetBindings[1].RenderTarget, new Vector2(screenWidth / 3, 0), null, Color.White, 0, Vector2.Zero, .33f, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        private void initGBuffer()
        {
            deferredRenderEffect.Parameters["View"].SetValue(viewMatrix);
            GraphicsDevice.SetRenderTargets(renderTargetBindings);
            foreach (EffectPass pass in clearBufferEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                quadRenderer.Render(Vector2.One * -1, Vector2.One);
            }
        }
        private void drawDirectionalLight()
        {
            directionalLightEffect.Parameters["cameraPosition"].SetValue(cameraPosition);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(viewMatrix * projectionMatrix));
            foreach (EffectPass pass in directionalLightEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                quadRenderer.Render(Vector2.One * -1, Vector2.One);
            }
        }
        #endregion
    }
    public struct CVF //Custom Vertex Format
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        //public Vector2 NormalMap;
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            //new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );
        public CVF(Vector3 p, Vector3 n, Vector2 t)
        {
            Position = p;
            Normal = n;
            Texture = t;
            //NormalMap = Vector2.Zero;
        }
    }
}
