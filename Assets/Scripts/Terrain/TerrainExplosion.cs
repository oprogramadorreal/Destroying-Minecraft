using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainExplosion
    {
        public TerrainExplosion(PointOnTerrainMesh pointOnTerrain, Vector3 explosionCenter, IEnumerable<Rigidbody> createdBlocksBodies, int numberOfBlocks)
        {
            PointOnTerrain = pointOnTerrain;
            ExplosionCenter = explosionCenter;
            CreatedBlocksBodies = createdBlocksBodies;
            NumberOfBlocks = numberOfBlocks;
        }

        public PointOnTerrainMesh PointOnTerrain { get; }

        public Vector3 ExplosionCenter { get; }

        public IEnumerable<Rigidbody> CreatedBlocksBodies { get; }

        public int NumberOfBlocks { get; }
    }
}