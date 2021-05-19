using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// This is the main script of the game.
    /// It controls all terrain generation related stuff.
    /// </summary>
    public sealed class TerrainManager : MonoBehaviour
    {
        [SerializeField]
        private Transform player;

        private GameObject terrainObject;

        private TerrainConfig config;
        private TerrainChunksPool chunksPool;
        private TerrainChunkGenerator chunkGenerator;
        private TerrainModifier modifier;

        private Index3D playerCurrentChunk = Index3D.InvalidIndex;
        private readonly HashSet<Index3D> currentChunks = new HashSet<Index3D>();

        private bool updatingTerrain = false;

        public float LoadingProgress { get; private set; } = 0.0f;

        public event EventHandler FirstLoadFinished;

        private async void Start()
        {
            terrainObject = new GameObject("Terrain")
            {
                isStatic = true
            };

            config = GetComponent<TerrainConfig>();
            var blocksGenerator = GetComponent<TerrainBlocksGenerator>();
            chunksPool = GetComponent<TerrainChunksPool>();

            chunkGenerator = new TerrainChunkGenerator(blocksGenerator, chunksPool, config);

            modifier = GetComponent<TerrainModifier>();
            modifier.Setup(chunkGenerator, terrainObject.transform);

            await UpdateTerrain();
            OnFirstLoadFinished();
        }

        private void OnFirstLoadFinished()
        {
            Debug.Log("First load finished!");

            var handler = FirstLoadFinished;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private async void Update()
        {
            await UpdateTerrain();
        }

        private async Task UpdateTerrain()
        {
            if (!updatingTerrain)
            {
                var newPlayerChunk = config.GetChunkIndexAt(player.position);

                if (!newPlayerChunk.Equals(playerCurrentChunk))
                {
                    var newCurrentChunks = CalculateChunksAround(newPlayerChunk, config.VisibleChunksRadius);

                    var chunksToDestroy = currentChunks.Except(newCurrentChunks);
                    var chunksToCreate = newCurrentChunks.Except(currentChunks);

                    chunksPool.Deactivate(chunksToDestroy);

                    updatingTerrain = true;

                    await GenerateChunks(chunksToCreate)
                        .ContinueWith(_ =>
                        {
                            currentChunks.Clear();
                            currentChunks.UnionWith(newCurrentChunks);
                            playerCurrentChunk = newPlayerChunk;
                            updatingTerrain = false;
                        });
                }
            }
        }

        private static IEnumerable<Index3D> CalculateChunksAround(Index3D chunkIndex, int visibleChunksRadius)
        {
            const int maxHeight = 0;

            return chunkIndex
                .GetIndicesAround(new Vector3Int(visibleChunksRadius, 0, visibleChunksRadius))
                .Select(i => new Index3D(i.X, System.Math.Min(i.Y, maxHeight), i.Z));
        }

        private async Task GenerateChunks(IEnumerable<Index3D> chunksToCreate)
        {
            var numberOfChunksToCreate = chunksToCreate.Count();
            
            LoadingProgress = 0.0f;
            var i = 0;

            foreach (var chunkIndex in chunksToCreate)
            {
                LoadingProgress = (i++ + 1.0f) / numberOfChunksToCreate;

                chunkGenerator.Generate(chunkIndex, terrainObject.transform);
                await Task.Yield();
            }
        }

        public TerrainBlock.Type? RemoveBlock(Ray ray)
        {
            return modifier.RemoveBlock(ray);
        }

        public TerrainBlock AddBlock(Ray ray, TerrainBlock.Type blockType)
        {
            return modifier.AddBlock(ray, blockType);
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray ray)
        {
            return modifier.RaycastTerrainMesh(ray);
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray ray, float maxDistance)
        {
            return modifier.RaycastTerrainMesh(ray, maxDistance);
        }

        public TerrainExplosion Explode(PointOnTerrainMesh pointOnTerrain, float explosionRadius)
        {
            return modifier.Explode(pointOnTerrain, explosionRadius);
        }

        public TerrainBlock GetBlockAt(Vector3 pointInWorld)
        {
            return modifier.GetBlockAt(pointInWorld);
        }

        public bool MakeSimulatedBlock(Ray viewRay, bool addForce)
        {
            return modifier.MakeSimulatedBlock(viewRay, addForce);
        }
    }
}