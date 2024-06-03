using Silk.NET.Maths;

namespace Autopalya;

public class CameraDescriptor
    {
        public bool MoveLeft { get; set; } = false;
        public bool MoveRight { get; set; } = false;
        public bool MoveForward { get; set; } = false;
        public bool MoveBackward { get; set; } = false;

        public bool RotateLeft { get; set; } = false;
        public bool RotateRight { get; set; } = false;
        public bool RotateUp { get; set; } = false;
        public bool RotateDown { get; set; } = false;

        public bool FirstPerson = false;

        public Vector3D<float> Position { get; set; } = new(0, 0, 0);

        // Yaw, Pitch, Roll
        public Vector3D<float> Rotation { get; set; } = Vector3D<float>.Zero;

        public float MoveSpeed { get; set;  } = 2;
        public float RotateSpeed { get; set; } = 1;

        public float Fov { get; set; } = (float)Math.PI / 3;

        public float MoveAxisX { get; private set; } = 0;
        public float MoveAxisY { get; private set; } = 0;
        public float RotateAxisX { get; private set; } = 0;
        public float RotateAxisY { get; private set; } = 0;

        private const float TimeToLockMove = 1; // seconds
        private const float TimeToLockRotate = 0.1f; // seconds

        public void Update(double deltaTime)
        {
            float moveXTarget = 0;
            if (MoveLeft)
            {
                moveXTarget = -1;
            }

            if (MoveRight)
            {
                moveXTarget += 1;
            }

            MoveAxisX += (moveXTarget - MoveAxisX) / TimeToLockMove * (float)deltaTime;

            float moveYTarget = 0; 
            if (MoveBackward)
            {
                moveYTarget = -1;
            }
            
            if (MoveForward)
            {
                moveYTarget += 1;
            }
            MoveAxisY += (moveYTarget - MoveAxisY) / TimeToLockMove * (float)deltaTime;

            float rotateXTarget = 0;
            if (RotateLeft)
            {
                rotateXTarget = -1;
            }

            if (RotateRight)
            {
                rotateXTarget += 1;
            }
            RotateAxisX += (rotateXTarget - RotateAxisX) / TimeToLockRotate * (float)deltaTime;

            float rotateYTarget = 0;
            if (RotateDown)
            {
                rotateYTarget = -1;
            }
            
            if (RotateUp)
            {
                rotateYTarget += 1;
            }
            RotateAxisY += (rotateYTarget - RotateAxisY) / TimeToLockRotate * (float)deltaTime;
        }

        public void Move(Vector3D<float> amount)
        {
            Position += amount;
        }

        public void Rotate(float vertical, float horizontal)
        {
            var rotate = new Vector3D<float>
            {
                Y = vertical,
                X = horizontal
            };
            Rotation += rotate;
        }

        public Vector3D<float> Forward => new(float.Cos(Rotation.Y) * float.Sin(Rotation.X), -float.Sin(Rotation.Y),
            -float.Cos(Rotation.Y) * float.Cos(Rotation.X));

        public Vector3D<float> Right => new(-float.Cos(Rotation.X), 0, -float.Sin(Rotation.X));
        
        public Vector3D<float> Up => new(float.Sin(Rotation.Y) * float.Sin(Rotation.X), float.Cos(Rotation.Y), float.Sin(Rotation.Y) * float.Cos(Rotation.X));

    }
