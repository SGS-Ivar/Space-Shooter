﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using System.Collections.Generic;
using System;

namespace SpaceShooter.Gameplay.Player
{
    public class Player : Entity
    {
        private float m_MaxSpeed;
        private float m_CurrentSpeed;

        private bool m_ShootPressed = false;
        private bool m_FrontGunsActive = true;
        private bool m_GunChangePressed = false;

        private Camera2D m_Camera;
        private Texture2D m_BulletTexture;
        private List<Bullet> m_Bullets = new List<Bullet>();

        private float m_Health = 300;
        private float m_MaxHealth = 300f;
        private Texture2D m_EmptyTexture;

        private int m_KillCount = 0;
        private int m_Score = 0;
        private int m_CurrentAmmo = 50;
        private int m_MaxAmmo = 50;

        private SoundEffect m_ShootSound;

        //Getting
        public List<Bullet> GetBullets() { return m_Bullets; }
        public float GetMaxSpeed() { return m_MaxSpeed; }
        public float GetHealth() { return m_Health; }
        public float GetMaxHealth() { return m_MaxHealth; }
        public int GetKills() { return m_KillCount; }
        public int GetScore() { return m_Score; }
        public int GetCurrentAmmo() { return m_CurrentAmmo; }
        public int GetMaxAmmo() { return m_MaxAmmo; }

        //Setting
        public override void SetPosition(Vector2 pos)
        {
            //if (pos.X < m_Graphics.PreferredBackBufferWidth && pos.X > 0 &&
            //    pos.Y < m_Graphics.PreferredBackBufferHeight && pos.Y > 0)
            //{
                m_Position = pos;
                m_Rectangle.X = (int)pos.X;
                m_Rectangle.Y = (int)pos.Y;
            //}
        }
        public void SetBulletTexture(Texture2D texture) { m_BulletTexture = texture; }
        public void SetHealth(float health) { m_Health = health; }
        public void SetEmptyTexture(Texture2D texture) { m_EmptyTexture = texture; }
        public void SetKillCount(int i) { m_KillCount = i; }
        public void SetScore(int i) { m_Score = i; }
        public void SetCurrentAmmo(int ammo) { m_CurrentAmmo = ammo; }
        public void SetShootSound(SoundEffect shoot) { m_ShootSound = shoot; }

        //Constructor sets the start values
        public Player(Vector2 pos, float rotation, float scale, Texture2D texture, Rectangle rect, GraphicsDeviceManager graphics, float maxSpeed, Camera2D camera) :
            base(pos, rotation, scale, texture, rect, graphics)
        {
            m_Position.X = pos.X;
            m_Position.Y = pos.Y;

            m_Scale = scale;
            m_Rotation = rotation;
            m_Texture = texture;

            m_Rectangle = rect;
            m_Graphics = graphics;
            m_MaxSpeed = maxSpeed;

            m_Camera = camera;
        }


        //Overrides the drawing of the player
        public override void Draw(ref SpriteBatch spriteBatch)
        {
            Rectangle rect = new Rectangle(0, 0, m_Texture.Width, m_Texture.Height);
            Vector2 origin = new Vector2(m_Texture.Width / 2, m_Texture.Height / 2);

            //Draw the player
            spriteBatch.Draw(m_Texture, m_Position, rect, Color.White, m_Rotation + MathHelper.ToRadians(90), origin, m_Scale, SpriteEffects.None, 0);

            //Draws the health bar in different colors depending on health value
            if (m_Health > m_MaxHealth / 2)
            {
                spriteBatch.Draw(m_EmptyTexture, new Rectangle((int)m_Position.X - 50, (int)m_Position.Y + m_Texture.Height, (int)(m_Texture.Width * (m_Health / m_MaxHealth)), m_EmptyTexture.Height + 10), Color.Green);
            }
            if (m_Health <= m_MaxHealth / 2)
            {
                spriteBatch.Draw(m_EmptyTexture, new Rectangle((int)m_Position.X - 50, (int)m_Position.Y + m_Texture.Height, (int)(m_Texture.Width * (m_Health / m_MaxHealth)), m_EmptyTexture.Height + 10), Color.Yellow);
            }
            if (m_Health <= m_MaxHealth / 5)
            {
                spriteBatch.Draw(m_EmptyTexture, new Rectangle((int)m_Position.X - 50, (int)m_Position.Y + m_Texture.Height, (int)(m_Texture.Width * (m_Health / m_MaxHealth)), m_EmptyTexture.Height + 10), Color.Red);
            }

            //Draw the bullets
            if (m_Bullets.Count > 0)
            {
                for (int i = 0; i < m_Bullets.Count; i++)
                {
                    m_Bullets[i].Draw(ref spriteBatch);
                }
            }
        }

        //Moves the player
        public override void Move(float speed, float mul)
        {
            //Get the direction it should move in
            Vector2 dir = new Vector2((float)Math.Cos(GetRotation()),
                          (float)Math.Sin(GetRotation()));
            dir.Normalize();

            //Add to the speed or remove from it
            if (m_CurrentSpeed <= m_MaxSpeed)
            {
                m_CurrentSpeed += speed;
            }
            else if (m_CurrentSpeed > m_MaxSpeed)
            {
                m_CurrentSpeed -= speed;
            }

            //Set thep players position
            SetPosition(GetPosition() + dir * m_CurrentSpeed * mul);

            //Set the posistions at which the camera should start moving
            Vector2 movePosLeft = Vector2.Transform(new Vector2(m_Graphics.PreferredBackBufferWidth / 7 * 6, m_Graphics.PreferredBackBufferHeight / 7 * 6), Matrix.Invert(m_Camera.GetTransform()));
            Vector2 movePosRight = Vector2.Transform(new Vector2(m_Graphics.PreferredBackBufferWidth / 7, m_Graphics.PreferredBackBufferHeight / 7), Matrix.Invert(m_Camera.GetTransform()));

            //If the player reaches any of these positions, move the camera
            if (m_Position.X > movePosLeft.X || m_Position.X < movePosRight.X || m_Position.Y < movePosRight.Y || m_Position.Y > movePosLeft.Y)
            {
                m_Camera.Move(dir * m_CurrentSpeed);
            }
            base.Move(speed, mul);
        }

        //Updates the player
        public override void Update(GameTime gameTime)
        {
            //Get the player input and set the rectangle
            GetInput();
            m_Rectangle = new Rectangle((int)m_Position.X, (int)m_Position.Y, m_Texture.Width, m_Texture.Height);
            base.Update(gameTime);
        }

        //Gets the input for the player
        private void GetInput()
        {
            /*
             * Moves the player
             * Rotates the player
             * Shoots
             * Changes shooting direction
             */
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                m_MaxSpeed = 10;
                Move(0.1f, 1);
            }
            if (Keyboard.GetState().IsKeyUp(Keys.W))
            {
                m_MaxSpeed = 0;
                Move(0.1f, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                SetRotation(MathHelper.ToDegrees(GetRotation()) + 3f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                SetRotation(MathHelper.ToDegrees(GetRotation()) - 3f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !m_ShootPressed)
            {
                float rotation = 0f;

                if (m_FrontGunsActive)
                {
                    rotation = GetRotation();
                }
                else
                {
                    rotation = GetRotation() - MathHelper.ToRadians(180);
                }
                m_ShootPressed = true;

                if (GetCurrentAmmo() > 0)
                {
                    m_ShootSound.Play();
                    GetBullets().Add(new Bullet(new Vector2(GetPosition().X, GetPosition().Y),
                        rotation, 0.5f, m_BulletTexture, 10f,
                        new Rectangle((int)GetPosition().X, (int)GetPosition().Y, m_BulletTexture.Width, m_BulletTexture.Height),
                        m_Graphics));

                    GetBullets()[GetBullets().Count - 1].LoadTextureData();
                    GetBullets()[GetBullets().Count - 1].SetDamage(50f);
                    SetCurrentAmmo(GetCurrentAmmo() - 1);
                }
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Space) && m_ShootPressed)
            {
                m_ShootPressed = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F) && !m_GunChangePressed)
            {
                m_FrontGunsActive = !m_FrontGunsActive;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.F))
            {
                m_GunChangePressed = false;
            }
        }
    }
}
