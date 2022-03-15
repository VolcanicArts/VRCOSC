// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Lists;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Drawables.Triangles;

// Taken and modified from https://github.com/ppy/osu/blob/master/osu.Game/Graphics/Backgrounds/Triangles.cs
public sealed class Triangles : Drawable
{
    private const float triangle_size = 100;
    private const float base_velocity = 50;

    /// <summary>
    ///     How many screen-space pixels are smoothed over.
    ///     Same behavior as Sprite's EdgeSmoothness.
    /// </summary>
    private const float edge_smoothness = 1;

    private readonly SortedList<TriangleParticle> parts = new(Comparer<TriangleParticle>.Default);
    private readonly Texture texture;

    private Color4 colourDark = Color4.Black;

    private Color4 colourLight = Color4.White;

    private IShader shader;

    private Random stableRandom;

    private float triangleScale = 1;

    /// <summary>
    ///     The relative velocity of the triangles. Default is 1.
    /// </summary>
    public float Velocity = 1;

    /// <summary>
    ///     Construct a new triangle visualisation.
    /// </summary>
    /// <param name="seed">
    ///     An optional seed to stabilise random positions / attributes. Note that this does not guarantee
    ///     stable playback when seeking in time.
    /// </param>
    public Triangles(int? seed = null)
    {
        if (seed != null)
            stableRandom = new Random(seed.Value);

        texture = Texture.WhitePixel;
    }

    public Color4 ColourLight
    {
        get => colourLight;
        set
        {
            if (colourLight == value) return;

            colourLight = value;
            updateColours();
        }
    }

    public Color4 ColourDark
    {
        get => colourDark;
        set
        {
            if (colourDark == value) return;

            colourDark = value;
            updateColours();
        }
    }

    /// <summary>
    ///     Whether we should create new triangles as others expire.
    /// </summary>
    private static bool CreateNewTriangles => true;

    /// <summary>
    ///     The amount of triangles we want compared to the default distribution.
    /// </summary>
    private static float SpawnRatio => 1;

    public float TriangleScale
    {
        get => triangleScale;
        set
        {
            var change = value / triangleScale;
            triangleScale = value;

            for (var i = 0; i < parts.Count; i++)
            {
                var newParticle = parts[i];
                newParticle.Scale *= change;
                parts[i] = newParticle;
            }
        }
    }

    private int AimCount { get; set; }

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders)
    {
        shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        addTriangles(true);
    }

    protected override void Update()
    {
        base.Update();

        Invalidate(Invalidation.DrawNode);

        if (CreateNewTriangles)
            addTriangles(false);

        var adjustedAlpha = MathF.Pow(DrawColourInfo.Colour.AverageColour.Linear.A, 3);

        var elapsedSeconds = (float)Time.Elapsed / 1000;
        // Since position is relative, the velocity needs to scale inversely with DrawHeight.
        // Since we will later multiply by the scale of individual triangles we normalize by
        // dividing by triangleScale.
        var movedDistance = -elapsedSeconds * Velocity * base_velocity / (DrawHeight * triangleScale);

        for (var i = 0; i < parts.Count; i++)
        {
            var newParticle = parts[i];

            // Scale moved distance by the size of the triangle. Smaller triangles should move more slowly.
            newParticle.Position.Y += Math.Max(0.5f, parts[i].Scale) * movedDistance;
            newParticle.Colour.A = adjustedAlpha;

            parts[i] = newParticle;

            var bottomPos = parts[i].Position.Y + triangle_size * parts[i].Scale * 0.866f / DrawHeight;
            if (bottomPos < 0)
                parts.RemoveAt(i);
        }
    }

    /// <summary>
    ///     Clears and re-initialises triangles according to a given seed.
    /// </summary>
    /// <param name="seed">
    ///     An optional seed to stabilise random positions / attributes. Note that this does not guarantee
    ///     stable playback when seeking in time.
    /// </param>
    public void Reset(int? seed = null)
    {
        if (seed != null)
            stableRandom = new Random(seed.Value);

        parts.Clear();
        addTriangles(true);
    }

    private void addTriangles(bool randomY)
    {
        // limited by the maximum size of QuadVertexBuffer for safety.
        const int max_triangles = QuadVertexBuffer<TexturedVertex2D>.MAX_QUADS;

        AimCount = (int)Math.Min(max_triangles, DrawWidth * DrawHeight * 0.002f / (triangleScale * triangleScale) * SpawnRatio);

        for (var i = 0; i < AimCount - parts.Count; i++)
            parts.Add(createTriangle(randomY));
    }

    private TriangleParticle createTriangle(bool randomY)
    {
        var particle = CreateTriangle();

        particle.Position = new Vector2(nextRandom(), randomY ? nextRandom() : 1);
        particle.ColourShade = nextRandom();
        particle.Colour = CreateTriangleShade(particle.ColourShade);

        return particle;
    }

    /// <summary>
    ///     Creates a triangle particle with a random scale.
    /// </summary>
    /// <returns>The triangle particle.</returns>
    private TriangleParticle CreateTriangle()
    {
        const float std_dev = 0.16f;
        const float mean = 0.5f;

        var u1 = 1 - nextRandom(); //uniform(0,1] random floats
        var u2 = 1 - nextRandom();
        var randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
        var scale = Math.Max(triangleScale * (mean + std_dev * randStdNormal), 0.1f); // random normal(mean,stdDev^2)

        return new TriangleParticle { Scale = scale };
    }

    /// <summary>
    ///     Creates a shade of colour for the triangles.
    /// </summary>
    /// <returns>The colour.</returns>
    private Color4 CreateTriangleShade(float shade)
    {
        return Interpolation.ValueAt(shade, colourDark, colourLight, 0, 1);
    }

    private void updateColours()
    {
        for (var i = 0; i < parts.Count; i++)
        {
            var newParticle = parts[i];
            newParticle.Colour = CreateTriangleShade(newParticle.ColourShade);
            parts[i] = newParticle;
        }
    }

    private float nextRandom()
    {
        return (float)(stableRandom?.NextDouble() ?? RNG.NextSingle());
    }

    protected override DrawNode CreateDrawNode()
    {
        return new TrianglesDrawNode(this);
    }

    private class TrianglesDrawNode : DrawNode
    {
        private readonly List<TriangleParticle> parts = new();

        private IShader shader;
        private Vector2 size;
        private Texture texture;

        private QuadBatch<TexturedVertex2D> vertexBatch;

        public TrianglesDrawNode(IDrawable source)
            : base(source)
        {
        }

        private new Triangles Source => (Triangles)base.Source;

        public override void ApplyState()
        {
            base.ApplyState();

            shader = Source.shader;
            texture = Source.texture;
            size = Source.DrawSize;

            parts.Clear();
            parts.AddRange(Source.parts);
        }

        public override void Draw(Action<TexturedVertex2D> vertexAction)
        {
            base.Draw(vertexAction);

            if (Source.AimCount > 0 && (vertexBatch == null || vertexBatch.Size != Source.AimCount))
            {
                vertexBatch?.Dispose();
                vertexBatch = new QuadBatch<TexturedVertex2D>(Source.AimCount, 1);
            }

            shader.Bind();

            var localInflationAmount = edge_smoothness * DrawInfo.MatrixInverse.ExtractScale().Xy;

            foreach (var particle in parts)
            {
                var offset = triangle_size * new Vector2(particle.Scale * 0.5f, particle.Scale * 0.866f);

                var triangle = new Triangle(
                    Vector2Extensions.Transform(particle.Position * size, DrawInfo.Matrix),
                    Vector2Extensions.Transform(particle.Position * size + offset, DrawInfo.Matrix),
                    Vector2Extensions.Transform(particle.Position * size + new Vector2(-offset.X, offset.Y), DrawInfo.Matrix)
                );

                var colourInfo = DrawColourInfo.Colour;
                colourInfo.ApplyChild(particle.Colour);

                DrawTriangle(
                    texture,
                    triangle,
                    colourInfo,
                    null,
                    vertexBatch.AddAction,
                    Vector2.Divide(localInflationAmount, new Vector2(2 * offset.X, offset.Y)));
            }

            shader.Unbind();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            vertexBatch?.Dispose();
        }
    }

    private struct TriangleParticle : IComparable<TriangleParticle>
    {
        /// <summary>
        ///     The position of the top vertex of the triangle.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        ///     The colour shade of the triangle.
        ///     This is needed for colour recalculation of visible triangles when <see cref="ColourDark" /> or
        ///     <see cref="ColourLight" /> is changed.
        /// </summary>
        public float ColourShade;

        /// <summary>
        ///     The colour of the triangle.
        /// </summary>
        public Color4 Colour;

        /// <summary>
        ///     The scale of the triangle.
        /// </summary>
        public float Scale;

        /// <summary>
        ///     Compares two <see cref="TriangleParticle" />s. This is a reverse comparer because when the
        ///     triangles are added to the particles list, they should be drawn from largest to smallest
        ///     such that the smaller triangles appear on top.
        /// </summary>
        /// <param name="other"></param>
        public int CompareTo(TriangleParticle other)
        {
            return other.Scale.CompareTo(Scale);
        }
    }
}
