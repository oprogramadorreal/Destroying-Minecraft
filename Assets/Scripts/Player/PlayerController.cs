using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private TerrainManager terrainManager;

        [SerializeField]
        private Transform playerHead;

        [SerializeField]
        private PlayerLook playerLook;

        [SerializeField]
        private TerrainConfig config;

        private Collider playerCollider;

        private readonly Stack<TerrainBlock.Type> blocksInventory = new Stack<TerrainBlock.Type>();

        private Kamehameha kamehameha;

        private void Start()
        {
            playerCollider = GetComponentInChildren<Collider>();
            kamehameha = GetComponentInChildren<Kamehameha>();
        }

        private void Update()
        {
            if (gameState.IsPaused)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RemoveBlock();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    AddBlock();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    kamehameha.Charge(terrainManager, GetViewRay);
                }
            }

            if (transform.position.y < -100.0f)
            {
                OnKilled();
            }

            //if (Input.GetMouseButtonDown(0))
            //{
            //    RemoveBlock();
            //}
            //else if (Input.GetMouseButtonDown(1))
            //{
            //    AddBlock();
            //}
            //else if (Input.GetMouseButtonDown(2))
            //{
            //    kamehameha.Charge(terrainManager, GetViewRay);
            //}
        }

        private void AddBlock()
        {
            if (blocksInventory.Count != 0)
            {
                var block = terrainManager.AddBlock(GetViewRay(), blocksInventory.Pop());

                if (block != null)
                {
                    if (block.GetBounds().Intersects(playerCollider.bounds))
                    {
                        transform.position += Vector3.up * config.BlockSize;
                    }

                    if (IsSimulationKeyPressed())
                    {
                        terrainManager.MakeSimulatedBlock(GetViewRay(), false);
                    }
                }
            }
        }

        private void RemoveBlock()
        {
            if (IsSimulationKeyPressed())
            {
                terrainManager.MakeSimulatedBlock(GetViewRay(), true);
            }
            else
            {
                RemoveBlockAndAddToInventory(GetViewRay());
            }
        }

        private static bool IsSimulationKeyPressed()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private void RemoveBlockAndAddToInventory(Ray viewRay)
        {
            var removedBlockType = terrainManager.RemoveBlock(viewRay);

            if (removedBlockType.HasValue)
            {
                blocksInventory.Push(removedBlockType.Value);
            }
        }

        private Ray GetViewRay()
        {
            return new Ray(playerHead.position, playerHead.forward);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Foe"))
            {
                var foeBody = TryToFindFoeBody(other);

                if (foeBody != null)
                {
                    var feetToHead = foeBody.GetHeadPosition() - foeBody.GetFeetPosition();
                    playerLook.SetKillerPosition(foeBody.GetFeetPosition() + feetToHead * 0.7f);
                }

                OnKilled();
            }
        }

        private static IPlayerBody TryToFindFoeBody(Collider collider)
        {
            var foeBody = collider.GetComponentInParent<IPlayerBody>();

            if (foeBody != null)
            {
                return foeBody;
            }

            return collider.GetComponentInChildren<IPlayerBody>();
        }

        public event EventHandler Killed;

        private void OnKilled()
        {
            var handler = Killed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}