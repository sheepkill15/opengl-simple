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

        public Vector3D<float> Position { get; set; } = new(0, 0, 0);

        // Yaw, Pitch, Roll
        public Vector3D<float> Rotation { get; set; } = Vector3D<float>.Zero;

        public float MoveSpeed { get; set;  } = 2;
        public float RotateSpeed { get; set; } = 1;

        public float Fov { get; set; } = (float)Math.PI / 3;

        public float MoveAxisX
        {
            get
            {
                return MoveLeft switch
                {
                    true when MoveRight => 0,
                    true => -1,
                    _ => MoveRight ? 1 : 0
                };
            }
        }
        public float MoveAxisY
        {
            get
            {
                return MoveForward switch
                {
                    true when MoveBackward => 0,
                    true => -1,
                    _ => MoveBackward ? 1 : 0
                };
            }
        }
        public float RotateAxisX
        {
            get
            {
                return RotateLeft switch
                {
                    true when RotateRight => 0,
                    true => -1,
                    _ => RotateRight ? 1 : 0
                };
            }
        }
        public float RotateAxisY
        {
            get
            {
                return RotateUp switch
                {
                    true when RotateDown => 0,
                    true => -1,
                    _ => RotateDown ? 1 : 0
                };
            }
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
