using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainModifier : MonoBehaviour
    {
        private TerrainConfig config;
        private ObjectsPool rigidbodiesPool;
        private TerrainChunkGenerator chunkGenerator;
        private Transform chunksParent;

        private const float raycastOffset = 0.01f;
        private const float raycastMaxDistance = 3.0f;

        private void Start()
        {
            config = GetComponent<TerrainConfig>();
            rigidbodiesPool = GetComponent<ObjectsPool>();
        }

        public void Setup(TerrainChunkGenerator chunkGenerator, Transform chunksParent)
        {
            this.chunkGenerator = chunkGenerator;
            this.chunksParent = chunksParent;
        }

        public bool MakeSimulatedBlock(Ray viewRay, bool addForce)
        {
            var pointOnTerrainMesh = RaycastTerrainMesh(viewRay, raycastOffset, raycastMaxDistance);
            return pointOnTerrainMesh != null ? MakeSimulatedBlock(pointOnTerrainMesh, addForce) : false;
        }

        public bool MakeSimulatedBlock(PointOnTerrainMesh pointOnTerrainMesh, bool addForce)
        {
            var success = false;

            TerrainChunk chunk;
            var block = GetBlockAt(pointOnTerrainMesh.Point, out chunk);

            if (block != null && block.BlockType != TerrainBlock.Type.None)
            {
                var removedBlockType = SetBlockTypeAt(chunk, block.LocalIndex, TerrainBlock.Type.None);
                SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointOnTerrainMesh.Point, TerrainBlock.Type.None);

                var rigidbody = CreateBlockRigidbody(removedBlockType, block.Center);

                if (addForce)
                {
                    var force = CalculateForce(block.Center);
                    rigidbody.AddForce(force, ForceMode.Impulse);
                    //rigidbody.AddRelativeTorque(CalculateTorque(), ForceMode.Force);
                }

                success = true;
            }

            return success;
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray viewRay)
        {
            return RaycastTerrainMesh(viewRay, raycastOffset, raycastMaxDistance);
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray viewRay, float maxDistance)
        {
            return RaycastTerrainMesh(viewRay, raycastOffset, maxDistance);
        }

        private PointOnTerrainMesh RaycastTerrainMesh(Ray viewRay, float offset, float maxDistance)
        {
            return config.RaycastTerrainMesh(viewRay, offset, maxDistance);
        }

        public TerrainExplosion Explode(PointOnTerrainMesh pointOnTerrainMesh, float explosionRadius)
        {
            var chunksToGenerateMesh = new Dictionary<Index3D, TerrainChunk>();

            var blocksToExplode = CollectBlocksInsideSphere(pointOnTerrainMesh.Point, explosionRadius).ToList();
            var createdBlocksBodies = CreateRigidbodiesForSurfaceBlocks(blocksToExplode.Select(e => e.Item2)).ToList();

            foreach (var entry in blocksToExplode)
            {
                var chunk = entry.Item1;
                var block = entry.Item2;

                chunk.SetBlockType(block.LocalIndex, TerrainBlock.Type.None);
                chunksToGenerateMesh[chunk.Index] = chunk;

                foreach (var adjacentChunk in GetAdjacentChunksThatShareBlock(chunk, block.LocalIndex))
                {
                    adjacentChunk.SetBlockType(adjacentChunk.GetBlockLocalIndexAt(block.Center), TerrainBlock.Type.None);
                    chunksToGenerateMesh[adjacentChunk.Index] = adjacentChunk;
                }
            }

            foreach (var entry in chunksToGenerateMesh)
            {
                chunkGenerator.BuildMeshFor(entry.Value);
            }

            var explosionCenter = CalculateExplosionCenter(blocksToExplode);
            return new TerrainExplosion(pointOnTerrainMesh, explosionCenter, createdBlocksBodies, blocksToExplode.Count);
        }

        private IEnumerable<Rigidbody> CreateRigidbodiesForSurfaceBlocks(IEnumerable<TerrainBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (IsSurfaceBlock(block))
                {
                    yield return CreateBlockRigidbody(block.BlockType, block.Center);
                }
            }
        }

        private bool IsSurfaceBlock(TerrainBlock block)
        {
            var blockPosition = block.Center;

            return IsBlockEmpty(blockPosition + Vector3.down * config.BlockSize)
                || IsBlockEmpty(blockPosition + Vector3.up * config.BlockSize)
                || IsBlockEmpty(blockPosition + Vector3.left * config.BlockSize)
                || IsBlockEmpty(blockPosition + Vector3.right * config.BlockSize)
                || IsBlockEmpty(blockPosition + Vector3.back * config.BlockSize)
                || IsBlockEmpty(blockPosition + Vector3.forward * config.BlockSize);
        }

        private static Vector3 CalculateExplosionCenter(IEnumerable<Tuple<TerrainChunk, TerrainBlock>> blocks)
        {
            var sum = Vector3.zero;

            foreach (var entry in blocks)
            {
                sum += entry.Item2.Center;
            }

            return sum / blocks.Count();
        } 

        private IEnumerable<Tuple<TerrainChunk, TerrainBlock>> CollectBlocksInsideSphere(Vector3 sphereCenter, float sphereRadius)
        {
            var minPoint = sphereCenter - new Vector3(sphereRadius, sphereRadius, sphereRadius);
            var maxPoint = sphereCenter + new Vector3(sphereRadius, sphereRadius, sphereRadius);

            var minIndex = config.GetBlockGlobalIndexAt(minPoint);
            var maxIndex = config.GetBlockGlobalIndexAt(maxPoint);

            var blockSizeAsVector3 = new Vector3(config.BlockSize, config.BlockSize, config.BlockSize);
            var sphereRadius2 = sphereRadius * sphereRadius;

            for (var x = minIndex.X; x <= maxIndex.X; ++x)
            {
                for (var y = minIndex.Y; y <= maxIndex.Y; ++y)
                {
                    for (var z = minIndex.Z; z <= maxIndex.Z; ++z)
                    {
                        var blockCenter = Vector3.Scale(new Index3D(x, y, z).AsVector3(), blockSizeAsVector3);
                        var distance2 = Vector3.SqrMagnitude(blockCenter - sphereCenter);

                        if (distance2 <= sphereRadius2)
                        {
                            var block = GetBlockAt(blockCenter, out TerrainChunk chunk);

                            if (block != null && block.BlockType != TerrainBlock.Type.None)
                            {
                                yield return new Tuple<TerrainChunk, TerrainBlock>(chunk, block);
                            }
                        }
                    }
                }
            }
        }

        private Rigidbody CreateBlockRigidbody(TerrainBlock.Type blockType, Vector3 position)
        {
            var rigidbodyObject = rigidbodiesPool.Instantiate(position);
            rigidbodyObject.GetComponent<MeshFilter>().mesh = BuildBlockMesh(blockType);
            return rigidbodyObject.GetComponent<Rigidbody>();
        }

        private Vector3 CalculateForce(Vector3 blockPosition)
        {
            if (IsBlockEmpty(blockPosition + Vector3.down * config.BlockSize))
            {
                return Vector3.zero;
            }

            var forceMagnitude = UnityEngine.Random.Range(10.0f, 20.0f);

            if (IsBlockEmpty(blockPosition + Vector3.up * config.BlockSize))
            {
                return Vector3.up * forceMagnitude;
            }

            var force = Vector3.zero;

            if (IsBlockEmpty(blockPosition + Vector3.left * config.BlockSize))
            {
                force += Vector3.left;
            }
            else if (IsBlockEmpty(blockPosition + Vector3.right * config.BlockSize))
            {
                force += Vector3.right;
            }

            if (IsBlockEmpty(blockPosition + Vector3.back * config.BlockSize))
            {
                force += Vector3.back;
            }
            else if (IsBlockEmpty(blockPosition + Vector3.forward * config.BlockSize))
            {
                force += Vector3.forward;
            }

            if (force.sqrMagnitude >= 0.01)
            {
                force.Normalize();
            }

            return force * forceMagnitude;
        }

        private bool IsBlockEmpty(Vector3 pointInWorld)
        {
            return GetBlockAt(pointInWorld)?.IsEmpty() ?? false;
        }

        //private static Vector3 CalculateTorque()
        //{
        //    return new Vector3(
        //        Random.Range(-50.0f, 50.0f),
        //        Random.Range(-50.0f, 50.0f),
        //        Random.Range(-50.0f, 50.0f)
        //    );
        //}

        private Mesh BuildBlockMesh(TerrainBlock.Type blockType)
        {
            return new TerrainChunkMeshBuilder(config).BuildBlockMesh(blockType);
        }

        public TerrainBlock AddBlock(Ray ray, TerrainBlock.Type blockType)
        {
            TerrainBlock block = null;
            var pointOnTerrainMesh = RaycastTerrainMesh(ray, -raycastOffset, raycastMaxDistance)?.Point;

            if (pointOnTerrainMesh.HasValue)
            {
                block = GetBlockAt(pointOnTerrainMesh.Value, out TerrainChunk chunk);

                if (block != null)
                {
                    SetBlockTypeAt(chunk, block.LocalIndex, blockType);
                    SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointOnTerrainMesh.Value, blockType);
                }
            }

            return block;
        }

        public TerrainBlock.Type? RemoveBlock(Ray ray)
        {
            TerrainBlock.Type? removedBlockType = null;

            var pointOnTerrainMesh = RaycastTerrainMesh(ray, raycastOffset, raycastMaxDistance)?.Point;

            if (pointOnTerrainMesh.HasValue)
            {
                removedBlockType = RemoveBlockAt(pointOnTerrainMesh.Value);
            }

            return removedBlockType;
        }

        private TerrainBlock.Type RemoveBlockAt(Vector3 pointInWorld)
        {
            var removedBlockType = TerrainBlock.Type.None;

            var block = GetBlockAt(pointInWorld, out TerrainChunk chunk);
            
            if (block != null && block.BlockType != TerrainBlock.Type.None)
            {
                removedBlockType = SetBlockTypeAt(chunk, block.LocalIndex, TerrainBlock.Type.None);
                SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointInWorld, TerrainBlock.Type.None);
            }

            return removedBlockType;
        }

        public TerrainBlock GetBlockAt(Vector3 pointInWorld)
        {
            return GetBlockAt(pointInWorld, out _);
        }

        private TerrainBlock GetBlockAt(Vector3 pointInWorld, out TerrainChunk chunk)
        {
            var chunkIndex = config.GetChunkIndexAt(pointInWorld);
            chunk = chunkGenerator.GetOrGenerateEmpty(chunkIndex, chunksParent);
            var blockLocalIndex = chunk.GetBlockLocalIndexAt(pointInWorld);

            return chunk.GetBlock(blockLocalIndex);
        }

        private void SetTypeOfSameBlockInAdjacentChunks(TerrainChunk chunk, Index3D blockLocalIndex, Vector3 pointOnTerrainMesh, TerrainBlock.Type blockNewType)
        {
            foreach (var adjacentChunk in GetAdjacentChunksThatShareBlock(chunk, blockLocalIndex))
            {
                SetBlockTypeAt(adjacentChunk, adjacentChunk.GetBlockLocalIndexAt(pointOnTerrainMesh), blockNewType);
            }
        }

        /// <summary>
        /// This is highly dependent on TerrainChunk internal details. (See "GetDirectionsToLook" method.)
        /// A TerrainChunk stores some blocks of adjacent chunks in order to simplify mesh generation.
        /// Therefore, when we change a block type, we have to update the block type in adjacent chunks as well.
        /// This method gathers the adjacent chunks that need to be updated.
        /// </summary>
        private IEnumerable<TerrainChunk> GetAdjacentChunksThatShareBlock(TerrainChunk chunk, Index3D blockLocalIndex)
        {
            foreach (var d in GetDirectionsToLook(chunk, blockLocalIndex))
            {
                yield return chunkGenerator.GetOrGenerateEmpty(chunk.Index.Step(d), chunksParent);
            }
        }

        private IEnumerable<Vector3> GetDirectionsToLook(TerrainChunk chunk, Index3D blockLocalIndex)
        {
            var directionsToUpdate = new List<Vector3>();

            if (blockLocalIndex.X <= chunk.MinBlockIndex.X)
            {
                directionsToUpdate.Add(Vector3.left);
            }
            else if (blockLocalIndex.X >= chunk.MaxBlockIndex.X)
            {
                directionsToUpdate.Add(Vector3.right);
            }

            if (blockLocalIndex.Y <= chunk.MinBlockIndex.Y)
            {
                directionsToUpdate.Add(Vector3.down);
            }
            else if (blockLocalIndex.Y >= chunk.MaxBlockIndex.Y)
            {
                directionsToUpdate.Add(Vector3.up);
            }

            if (blockLocalIndex.Z <= chunk.MinBlockIndex.Z)
            {
                directionsToUpdate.Add(Vector3.back);
            }
            else if (blockLocalIndex.Z >= chunk.MaxBlockIndex.Z)
            {
                directionsToUpdate.Add(Vector3.forward);
            }

            var numberOfDirections = directionsToUpdate.Count;

            for (var i = 0; i < Mathf.CeilToInt(numberOfDirections / 2.0f); ++i)
            {
                for (var j = i + 1; j < numberOfDirections; ++j)
                {
                    directionsToUpdate.Add(directionsToUpdate[i] + directionsToUpdate[j]);
                }
            }

            return directionsToUpdate;
        }

        private TerrainBlock.Type SetBlockTypeAt(TerrainChunk chunk, Index3D blockLocalIndex, TerrainBlock.Type blockNewType)
        {
            var blockType = chunk.GetBlock(blockLocalIndex).BlockType;
            chunk.SetBlockType(blockLocalIndex, blockNewType);
            chunkGenerator.BuildMeshFor(chunk);
            return blockType;
        }
    }
}