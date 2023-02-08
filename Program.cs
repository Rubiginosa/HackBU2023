using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TerminalRenderer;


namespace TerminalRenderer;
internal class Program
{
    public static Window STAT;
    const Double SPEED = .5;

    static void Main(String[] args)
    {
        Terminal.HideCursor();


        Window window = new(Console.WindowWidth, Console.WindowHeight-2);
        STAT = window;
        window.Clear();
        Camera c = new Camera(width: window.Width * 2 / 20.0, height: window.Height * 2 / 8.0, x: 1000, fov: 100, yaw: -90 * Math.PI/180);

        List<Triangle3> triangles = new();

        STLParser.Read(args[0], triangles, scaleFactor: Double.Parse(args[1]));
        List<Polygon> polygons = new(triangles.Select(s => new Polygon { Triangle = s, Color = Terminal.ForegroundColor.Default, Shade = ' ' }).ToArray());


        Boolean rotate = args.Contains("-r");

        (Double x, Double z) Center()
        {
            Double x = 0, z = 0;
            foreach (Polygon p in polygons)
            {
                x += p.Triangle.A.X / polygons.Count;
                z += p.Triangle.A.Z / polygons.Count;
            }
            return (x , z );


        }

        (Double centerX, Double centerZ) = Center();

        while (true)
        {
            STLParser.Shade(polygons, Terminal.ForegroundColor.Green);

            polygons.Sort((a, b) => (Int32) c.Distance(b.Triangle.Midpoint) - (Int32) c.Distance(a.Triangle.Midpoint));
            window.Clear();
            window.Update();

            foreach (Polygon polygon in polygons)
            {
                if (!c.InView(polygon.Triangle)) continue;
                Triangle2 transformedTriangle = new(c.Scale(c.ConvertVector(polygon.Triangle.A), window.Width, window.Height),
                c.Scale(c.ConvertVector(polygon.Triangle.B), window.Width, window.Height),
                c.Scale(c.ConvertVector(polygon.Triangle.C), window.Width, window.Height)
                );
                window.Triangle(transformedTriangle, polygon.Color, polygon.Shade);
            }
            //window.Text($"Transformed Coords: {transformedTriangle.A.X}, {transformedTriangle.A.Y}");
            HandleInput(c);
            window.Text($"X: {c.X}, Y: {c.Y}, Z: {c.Z}");
            window.Text($"Rotating around: {centerX}, {centerZ}");
            window.Draw();
            if (rotate) Rotate(polygons, Math.PI / 180,centerX, centerZ);
        }

    }
    public static void Rotate(List<Polygon> polygons, Double yaw, Double centerX = 0, Double centerZ = 0)
    {

        for (Int32 i = 0; i < polygons.Count; i++)
        {
            Polygon p = polygons[i];
            Double x = p.Triangle.A.X - centerX;
            Double z = p.Triangle.A.Z - centerZ;

            p.Triangle.A.X = x * Math.Cos(yaw) - z * Math.Sin(yaw) + centerX;
            p.Triangle.A.Z = x * Math.Sin(yaw) + z * Math.Cos(yaw) + centerZ;

            x = p.Triangle.B.X - centerX;
            z = p.Triangle.B.Z - centerZ;

            p.Triangle.B.X = x * Math.Cos(yaw) - z * Math.Sin(yaw) + centerX;
            p.Triangle.B.Z = x * Math.Sin(yaw) + z * Math.Cos(yaw) + centerZ;

            x = p.Triangle.C.X - centerX;
            z = p.Triangle.C.Z - centerZ;

            p.Triangle.C.X = x * Math.Cos(yaw) - z * Math.Sin(yaw) + centerX;
            p.Triangle.C.Z = x * Math.Sin(yaw) + z * Math.Cos(yaw)  + centerZ;

            polygons[i] = p;
        }
    }

    public static void HandleInput (Camera camera)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey();
            switch (key.Key) {

                case ConsoleKey.W:
                    camera.X = camera.X + (Math.Sin(camera.Yaw)) / SPEED;
                    camera.Z = camera.Z + (Math.Cos(camera.Yaw)) / SPEED;
                    break;
                case ConsoleKey.S:
                    camera.X = camera.X - (Math.Sin(camera.Yaw)) / SPEED;
                    camera.Z = camera.Z - (Math.Cos(camera.Yaw)) / SPEED;
                    break;
                case ConsoleKey.A:
                    camera.X = camera.X - (Math.Cos(camera.Yaw)) / SPEED;
                    camera.Z = camera.Z + (Math.Sin(camera.Yaw)) / SPEED;
                    break;
                case ConsoleKey.D:
                    camera.X = camera.X + (Math.Cos(camera.Yaw)) / SPEED;
                    camera.Z = camera.Z - (Math.Sin(camera.Yaw)) / SPEED;
                    break;
                case ConsoleKey.R:
                    camera.Y = camera.Y + 1 / SPEED;
                    break;
                case ConsoleKey.F:
                    camera.Y = camera.Y - 1/ SPEED;
                    break;
                case ConsoleKey.LeftArrow:
                    camera.Yaw -= Math.PI / 180 / SPEED;
                    break;
                case ConsoleKey.RightArrow:
                    camera.Yaw += Math.PI / 180 / SPEED;
                    break;
                case ConsoleKey.UpArrow:
                    camera.Pitch = Math.Max(Math.PI / 180 * -80, camera.Pitch - Math.PI / 180 / SPEED);
                    break;
                case ConsoleKey.DownArrow:
                    camera.Pitch = Math.Min(Math.PI / 180 * 80, camera.Pitch + Math.PI / 180 / SPEED);
                    break;
                case ConsoleKey.Q:
                    camera.Roll += Math.PI / 180 / SPEED;
                    break;
                case ConsoleKey.E:
                    camera.Roll -= Math.PI / 180 / SPEED;
                    break;
                case ConsoleKey.O:
                    camera.FOV+=10;
                    break;
                case ConsoleKey.P:
                    camera.FOV-=10;
                    break;
                case ConsoleKey.X:
                    Terminal.Reset();
                    Environment.Exit(0);
                    break;

            }
        }
    }

}
internal struct Pixel
{
    public Char Character = ' ';
    public Terminal.ForegroundColor Foreground = Terminal.ForegroundColor.Default;
    public Pixel() { }
}
internal struct Triangle2
{
    public Vector2 A, B, C;
    public Triangle2(Vector2 A, Vector2 B, Vector2 C)
    {
        this.A = A; this.B = B; this.C = C;
    }
}
internal struct Vector2
{
    public Double X, Y;
    public static implicit operator Vector2((Double,Double) tuple)
    {
        Vector2 v = new();
        (v.X, v.Y) = tuple;
        return v;
    }
    public override String ToString()
    {
        return $"<{X}, {Y}>";
    }
}
internal struct Triangle3
{
    public Vector3 A, B, C;
    public Triangle3(Vector3 A, Vector3 B, Vector3 C)
    {
        this.A = A; this.B = B; this.C = C;
    }
    public Vector3 Midpoint
    {
        get => (A.X + B.X + C.X, A.Y + B.Y + C.Y, A.Z + B.Z + C.Z);
    }
}
internal class Polygon
{
    public Triangle3 Triangle;
    public Char Shade;
    public Terminal.ForegroundColor Color;
}
internal struct Vector3
{
    public Double X, Y, Z;
    public static implicit operator Vector3((Double, Double, Double) tuple)
    {
        Vector3 v = new();
        (v.X, v.Y, v.Z) = tuple;
        return v;
    }
    public void Deconstruct(out Double x, out Double y, out Double z)
    {
        x = X; y = Y; z = Z;
    }
    public override string ToString()
    {
        return $"<{X}, {Y}, {Z}";
    }
    public Double Length
    {
        get => Math.Sqrt(X * X + Y * Y + Z * Z);
    }
}

class Terminal
{
    const String ESCAPE = "\u001b";
    internal enum ForegroundColor
    {
        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Magenta = 35,
        Cyan = 36,
        White = 37,
        Default = 39,
        Reset = 0
    }
    internal enum BackgroundColor
    {
        Black = 40,
        Red = 41,
        Green = 42,
        Yellow = 43,
        Blue = 44,
        Magenta = 45,
        Cyan = 46,
        White = 47,
        Default = 49,
        Reset = 0

    }
    internal static void MoveUp(Int32 spaces) => Console.Write($"{ESCAPE}[{spaces}A");
    internal static void MoveDown(Int32 spaces) => Console.Write($"{ESCAPE}[{spaces}B");
    internal static void MoveLeft(Int32 spaces) => Console.Write($"{ESCAPE}[{spaces}C");
    internal static void MoveRight(Int32 spaces) => Console.Write($"{ESCAPE}[{spaces}D");
    internal static void MoveToColumn(Int32 column) => Console.Write($"{ESCAPE}[{column}G");
    internal static void HideCursor() => Console.Write($"{ESCAPE}[?25l");
    internal static void SetColor(ForegroundColor color) => Console.Write($"{ESCAPE}[{(Int32)color}m");
    internal static String SetColorCommand(ForegroundColor color) => $"{ESCAPE}[{(Int32)color}m";
    internal static void Reset() => Console.Write($"{ESCAPE}[?25h{ESCAPE}[0m");

}
class Window
{
    Pixel[,] output;
    StringBuilder stringBuilder;
    Int32 textLine;

    public Window(int width, int height)
    {
        output = new Pixel[width, height];
        stringBuilder = new StringBuilder(4 * output.GetLength(1));
    }
    public void Triangle(Triangle2 triangle, Terminal.ForegroundColor color, Char character)
    {
        output.RasterizeTriangle(triangle, color, character);
    }
    public Int32 Width { get => output.GetLength(0); }
    public Int32 Height { get => output.GetLength(1); }


    public void Clear()
    {
        textLine = 0;
        for (int i = 0; i < output.Length;i++)
        {
            output[i % output.GetLength(0), i / output.GetLength(0)].Character = ' ';
        }
    }
    /*public void Draw()
    {
        Terminal.MoveUp(output.GetLength(1));
        for (Int32 i = 0; i < output.GetLength(1); i++) { 
            for (Int32 j = 0; j < output.GetLength(0); j++)
            {
                Terminal.SetColor(output[j,i].Foreground);
                Console.Write(output[j,i].Character);
            }
            Console.WriteLine();
        }
        Terminal.SetColor(Terminal.ForegroundColor.Default);
    }*/
    public void Text(String s, Int32 x, Int32 y)
    {
        for (Int32 i = 0; i < s.Length;i++)
        {
            output[x + i, y].Character = s[i];
        }
    }
    public void Text(String s)
    {
        Text(s, 0, textLine);
        textLine++;
    }
    
    public void Draw()
    {
        Terminal.MoveUp(output.GetLength(1)+1);
        stringBuilder.Clear();
        for (Int32 i = 0; i < output.GetLength(1);i++)
        {
            for (Int32 j = 0; j < output.GetLength(0);j++)
            {
                stringBuilder.Append(Terminal.SetColorCommand(output[j, i].Foreground));
                stringBuilder.Append(output[j, i].Character);
            }
            stringBuilder.AppendLine();
        }
        Console.Write(stringBuilder.ToString());
        Terminal.SetColor(Terminal.ForegroundColor.Default);
    }
    public void Update()
    {
        if (Console.WindowHeight -2 != Height || Console.WindowWidth != Width)
        {
            //Console.WriteLine("CHANGE");
            output = new Pixel[Console.WindowWidth, Console.WindowHeight - 2];
            stringBuilder = new StringBuilder(4 * output.Length);
            Clear();
            Terminal.HideCursor();
        }
    }
}
class STLParser
{
    public static void Shade(in List<Polygon> polygons, Terminal.ForegroundColor color)
    {
        List<Polygon> result = new();
        Double maxX = 0, minX = 0;

        foreach (Polygon polygon in polygons)
        {
            if (polygon.Triangle.Midpoint.X > maxX) maxX = polygon.Triangle.Midpoint.X;
            if (polygon.Triangle.Midpoint.X < minX) minX = polygon.Triangle.Midpoint.X;

        }
        Double range = maxX - minX;

        for (Int32 i = 0; i < polygons.Count; i++)
        {
            Char shade;

            if (polygons[i].Triangle.Midpoint.X < minX + range / 4) shade = '░';
            else if (polygons[i].Triangle.Midpoint.X < minX + range * 2 / 4) shade = '▒';
            else if (polygons[i].Triangle.Midpoint.X < minX + range * 3 / 4) shade = '▓';
            else shade = '█';

            polygons[i].Shade = shade;
            polygons[i].Color = color;

        }
    }
    public static void Read(String filename, List<Triangle3> triangles, Double scaleFactor = 1)
    {
        StreamReader streamReader = File.OpenText(filename);
        String currentLine;
        while ((currentLine = streamReader.ReadLine()) != null)
        {
            if (currentLine.Contains("outer"))
            {
                String subLine;
                Regex regex = new Regex("\\s+");

                subLine = streamReader.ReadLine();
                //Console.WriteLine($"Subline: {subLine}");
                var split = regex.Split(subLine)[2..5].Select(str => Double.Parse(str)).ToArray();
                Vector3 a = (scaleFactor * split[0], scaleFactor * split[1], scaleFactor * split[2]);

                subLine = streamReader.ReadLine();
                split = regex.Split(subLine)[2..5].Select(str => Double.Parse(str)).ToArray();
                Vector3 b = (scaleFactor * split[0], scaleFactor * split[1], scaleFactor * split[2]);
                
                subLine = streamReader.ReadLine();
                split = regex.Split(subLine)[2..5].Select(str => Double.Parse(str)).ToArray();
                Vector3 c = (scaleFactor * split[0], scaleFactor * split[1], scaleFactor * split[2]);

                triangles.Add(new Triangle3(a, b, c));
            }
        }
    }
}
class Camera
{
    Double x, y, z, pitch, roll, yaw;
    Double fov;
    Double width, height;
    
    public Camera(Double x = -10, Double y = 0, Double z = 0, Double pitch = 0, Double roll = 0, Double yaw = 0, Double fov = 1000, Double width = 100, Double height = 100)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.pitch = pitch;
        this.roll = roll;
        this.yaw = yaw;
        this.fov = fov;
        this.width = width;
        this.height = height;
    }
    public Double Distance(Vector3 vector)
    {
        Vector3 diff = (vector.X - x, vector.Y - y, vector.Z - z);
        return diff.Length;
    }
    public Boolean InView(Triangle3 triangle)
    {
        Double ZComponent(Vector3 vector)
        {
            var (x, y, z) = vector;
            Double x1 = x - this.x, y1 = y - this.y, z1 = z - this.z;

            x = x1 * Math.Cos(yaw) - z1 * Math.Sin(yaw);
            z = x1 * Math.Sin(yaw) + z1 * Math.Cos(yaw);

            x1 = x; z1 = z;

            y = y1 * Math.Cos(pitch) - z1 * Math.Sin(pitch);
            z = y1 * Math.Sin(pitch) + z1 * Math.Cos(pitch);

            y1 = y; z1 = z;

            y = y1 * Math.Cos(roll) - x1 * Math.Sin(roll);
            x = y1 * Math.Sin(roll) + x1 * Math.Cos(roll);

            return z;
        }
        return (ZComponent(triangle.A) > 0 || ZComponent(triangle.B) > 0 || ZComponent(triangle.C) > 0);
    }

    public Double Width { get => width; }
    public Double Height { get => height;}
    public double X { get => x; set => x = value; }
    public double Y { get => y; set => y = value; }
    public double Z { get => z; set => z = value; }
    public double Pitch { get => pitch; set => pitch = value; }
    public double Roll { get => roll; set => roll = value; }
    public double Yaw { get => yaw; set => yaw = value; }
    public double FOV { get => fov; set => fov = value; }

    public Vector2 Scale(Vector2 input, Int32 width, Int32 height)
    {
        return (input.X*width/this.width,input.Y*height/this.height);
    }

    public Vector2 ConvertVector(Vector3 vector)
    {
        //Program.STAT.Text($"Input Vector: {vector}");
        var (x, y, z) = vector;
        Double x1 = x - this.x, y1 = y - this.y, z1 = z - this.z;

        x = x1 * Math.Cos(yaw) - z1 * Math.Sin(yaw);
        z = x1 * Math.Sin(yaw) + z1 * Math.Cos(yaw);

        x1 = x; z1 = z;

        y = y1 * Math.Cos(pitch) - z1 * Math.Sin(pitch);
        z = y1 * Math.Sin(pitch) + z1 * Math.Cos(pitch);

        y1 = y; z1 = z;

        y = y1 * Math.Cos(roll) - x1 * Math.Sin(roll);
        x = y1 * Math.Sin(roll) + x1 * Math.Cos(roll);



        Vector2 v = ((width / 2 + fov * x / z), (height / 2 + fov *  -1 *y / z));
        //Program.STAT.Text($"Output Vector: {v}");
        return v;

    }
}
static class Extensions
{
    internal static void RasterizeTriangle(this Pixel[,] output, Triangle2 triangle, Terminal.ForegroundColor color, Char character)
    {
        Vector2 A = triangle.A;
        Vector2 B = triangle.B;
        Vector2 C = triangle.C;
        Int32 leftXBound     = (Int32) Math.Max(0, Math.Min(Math.Min(A.X, B.X), C.X));
        Int32 rightXBound    = (Int32) Math.Min(output.GetLength(0), Math.Max(Math.Max(A.X, B.X), C.X));
        Int32 topYBound      = (Int32) Math.Max(0, Math.Min(Math.Min(A.Y, B.Y), C.Y));
        Int32 bottomYBound   = (Int32) Math.Min(output.GetLength(1), Math.Max(Math.Max(A.Y, B.Y), C.Y));

        for (int x = leftXBound; x < rightXBound; x++)
        {
            for (int y = topYBound; y < bottomYBound; y++)
            {
                if (
                    (
                        (A.X - B.X) * (y + .5 - B.Y) - (A.Y - B.Y) * (x + .5 - B.X) < 0 &&
                        (B.X - C.X) * (y + .5 - C.Y) - (B.Y - C.Y) * (x + .5 - C.X) < 0 &&
                        (C.X - A.X) * (y + .5 - A.Y) - (C.Y - A.Y) * (x + .5 - A.X) < 0
                    ) || (
                        (A.X - B.X) * (y + .5 - B.Y) - (A.Y - B.Y) * (x + .5 - B.X) > 0 &&
                        (B.X - C.X) * (y + .5 - C.Y) - (B.Y - C.Y) * (x + .5 - C.X) > 0 &&
                        (C.X - A.X) * (y + .5 - A.Y) - (C.Y - A.Y) * (x + .5 - A.X) > 0
                    )
                    )
                {
                    output[x, y].Character = character;
                    output[x, y].Foreground = color;
                }

            }
        }
    }
}