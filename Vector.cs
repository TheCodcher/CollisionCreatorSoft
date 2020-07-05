using System;
using System.Collections.Generic;
using System.Text;
using ControlEngine.Extended;
using System.Drawing;

namespace ControlEngine
{
    namespace Extended
    {
        public struct Vector
        {
            public readonly double X;
            public readonly double Y;
            public static readonly Vector Up = new Vector(0, -1);
            public static readonly Vector Left = new Vector(-1, 0);
            public static readonly Vector Right = new Vector(1, 0);
            public static readonly Vector Down = new Vector(0, 1);
            public static readonly Vector Empty = new Vector(0, 0);

            public Vector(double X, double Y)
            {
                this.X = X;
                this.Y = Y;
            }

            public static Vector operator +(Vector A, Vector B) => new Vector(A.X + B.X, A.Y + B.Y);
            public static Vector operator +(Vector A, double b) => new Vector(A.X + b, A.Y + b);
            public static Vector operator *(Vector A, Vector B) => new Vector(A.X * B.X, A.Y * B.Y);
            public static Vector operator *(Vector A, double b) => new Vector(A.X * b, A.Y * b);
            public static Vector operator /(Vector A, Vector B) => new Vector(A.X / B.X, A.Y / B.Y);
            public static Vector operator /(Vector A, double b) => new Vector(A.X / b, A.Y / b);
            public static Vector operator -(Vector A) => new Vector(-A.X, -A.Y);
            public static Vector operator -(Vector A, Vector B) => new Vector(A.X - B.X, A.Y - B.Y);
            public static Vector operator -(Vector A, double b) => new Vector(A.X - b, A.Y - b);
            public static Vector operator ++(Vector A)
            {
                var normal = A.Normalize();
                return new Vector(A.X + normal.X, A.Y + normal.X);
            }
            public static Vector operator --(Vector A)
            {
                var normal = A.Normalize();
                return new Vector(A.X - normal.X, A.Y - normal.X);
            }
            public static bool operator ==(Vector A, Vector B) => (A.X == B.X) && (A.Y == B.Y) ? true : false;
            public static bool operator ==(Vector A, double b) => (A.X == b) && (A.Y == b) ? true : false;
            public static bool operator !=(Vector A, Vector B) => (A.X != B.X) || (A.Y != B.Y) ? true : false;
            public static bool operator !=(Vector A, double b) => (A.X != b) || (A.Y != b) ? true : false;
            //.///.////

            public static bool operator <=(Vector A, double b)
            {
                return A.SquareLength() <= b * b ? true : false;
            }
            public static bool operator >=(Vector A, double b)
            {
                return A.SquareLength() >= b * b ? true : false;
            }
            public static bool operator <=(Vector A, Vector B)
            {
                return A.SquareLength() <= B.SquareLength() ? true : false;
            }
            public static bool operator >=(Vector A, Vector B)
            {
                return A.SquareLength() >= B.SquareLength() ? true : false;
            }

            //.///.////

            public static bool operator <(Vector A, double b)
            {
                return A.SquareLength() < b * b ? true : false;
            }
            public static bool operator >(Vector A, double b)
            {
                return A.SquareLength() > b * b ? true : false;
            }
            public static bool operator <(Vector A, Vector B)
            {
                return A.SquareLength() < B.SquareLength() ? true : false;
            }
            public static bool operator >(Vector A, Vector B)
            {
                return A.SquareLength() > B.SquareLength() ? true : false;
            }

            //.///.////
            public override bool Equals(object obj)
            {
                if (!(obj is Vector))
                {
                    return false;
                }
                var A = (Vector)obj;
                return (X == A.X) && (Y == A.Y);
            }

            public override int GetHashCode()
            {
                return Tuple.Create(X, Y).GetHashCode();
            }
            public double Length()
            {
                return Math.Sqrt(X * X + Y * Y);
            }
            public double SquareLength()
            {
                return X * X + Y * Y;
            }
            public static Vector Rotate(Vector Vect, double Angle)
            {
                double rad = MathExtended.ToRadians(Angle);
                if (rad == 0) return Vect;
                var sin = Math.Sin(rad);
                var cos = Math.Cos(rad);
                return new Vector(Vect.X * cos - Vect.Y * sin, Vect.X * sin + Vect.Y * cos);
            }
            public static double CalcAngleVect(Vector Vect1, Vector Vect2)
            {
                if (Vect1 == 0 || Vect2 == 0) return 0;
                double angle = Math.Acos((Vect1.X * Vect2.X + Vect1.Y * Vect2.Y) / (Vect1.Length() * Vect2.Length())) / Math.PI * 180;
                var d = Vect2.X * Vect1.Y - Vect2.Y * Vect1.X;
                return d > 0 ? 360 - angle : angle;
            }
            public static double CalcAngleNormalVect(Vector NormVect1, Vector NormVect2)
            {
                if (NormVect1 == 0 || NormVect2 == 0) return 0;
                double angle = Math.Acos(NormVect1.X * NormVect2.X + NormVect1.Y * NormVect2.Y) / Math.PI * 180;
                var d = NormVect2.X * NormVect1.Y - NormVect2.Y * NormVect1.X;
                return d > 0 ? 360 - angle : angle;
            }
            /// <summary>
            /// Возвращает угол между -OY и вектором, с началом в наччале координат и концом в указанной точке
            /// </summary>
            /// <returns></returns>
            public double UpVectorAngle()
            {
                return CalcAngleVect(Up, this);
            }
            /// <summary>
            /// Возвращает угол между -OY и вектором, с началом в наччале координат и концом в указанной точке
            /// </summary>
            /// <returns></returns>
            public double RightVectorAngle()
            {
                return CalcAngleVect(Right, this);
            }
            /// <summary>
            /// Возвращает угол между двумя векторами
            /// </summary>
            /// <param name="Vect"></param>
            /// <returns></returns>
            public double CalcAngleVect(Vector Vect)
            {
                return CalcAngleVect(this, Vect);
            }
            public Vector Rotate(double Angle) => Rotate(this, Angle);
            public static Vector Normalize(Vector Vect)
            {
                if (Vect == 0) return Vect;
                var l = Vect.Length();
                return new Vector(Vect.X / l, Vect.Y / l);
            }
            public static Vector Projection(Vector SourceVect, Vector DirectVect)
            {
                if (SourceVect == 0 || DirectVect == 0) return 0;
                return (SourceVect.X * DirectVect.X + SourceVect.Y * DirectVect.Y) / DirectVect.SquareLength() * DirectVect;
            }
            public static Vector Rejection(Vector SourceVect, Vector DirectVect)
            {
                if (SourceVect == 0 || DirectVect == 0) return 0;
                return -SourceVect + SourceVect.Projection(DirectVect);
            }
            public Vector Projection(Vector DirectVect) => Projection(this, DirectVect);
            public Vector Rejection(Vector DirectVect) => Rejection(this, DirectVect);
            public Vector Normalize() => Normalize(this);
            //добавить нахождение проекции одного вектора на другой и перпендикулярный вектор

            public static implicit operator Point(Vector A) => new Point((int)Math.Round(A.X), (int)Math.Round(A.Y));
            public static implicit operator Vector(Point A) => new Vector(A.X, A.Y);

            public static implicit operator Vector(Size A) => new Vector(A.Width, A.Height);
            public static implicit operator Size(Vector A) => new Size((int)Math.Round(A.X), (int)Math.Round(A.Y));

            public static implicit operator Vector(double a) => new Vector(a, a);

            public static implicit operator Vector(PointF A) => new Vector(A.X, A.Y);
            public static implicit operator PointF(Vector A) => new PointF((float)A.X, (float)A.Y);
            public Vector Ceiling()
            {
                return new Vector(Math.Ceiling(Math.Abs(X)) * Math.Sign(X), Math.Ceiling(Math.Abs(Y)) * Math.Sign(Y));
            }
        }
    }
    static class MathExtended
    {
        public static double ToRadians(double Degrees)
        {
            if (Math.Abs(Degrees) >= 360) Degrees = Degrees - (int)Degrees / 360 * 360;
            if (Degrees == 0) return 0;
            if (Degrees < 0) Degrees = 360 - Degrees;
            return Degrees / 180 * Math.PI;
        }
        public static double ToDegrees(double Radians)
        {
            throw new NotImplementedException();
        }
    }
}
