using System;
using System.Collections.Generic;
using System.Linq;
using MiNET.Worlds;
using System.Diagnostics;
using MiNET.Net;
using MiNET.Utils;
using log4net;

namespace ClashOfMinecraft
{
    internal class LevelFactory
    {
        private readonly Dictionary<string, AnvilWorldProvider> _providerCache =
            new Dictionary<string, AnvilWorldProvider>();

        private static readonly ILog Log = LogManager.GetLogger(typeof (CraftRoyale));

        public AnvilWorldProvider GetLevelProvider(string levelDir, bool readOnly = false)
        {
            AnvilWorldProvider provider;
            _providerCache.TryGetValue(levelDir, out provider);

            if (provider == null)
            {
                provider = new AnvilWorldProvider(levelDir);
                provider.Initialize();

                PlayerLocation spawnPoint = new PlayerLocation(provider.GetSpawnPoint());
                Stopwatch chunkLoading = new Stopwatch();

                chunkLoading.Start();

                int i = 0;
                foreach (
                    var chunk in
                        GenerateChunks(new ChunkCoordinates(spawnPoint), new Dictionary<Tuple<int, int>, McpeBatch>(),
                            5, provider))
                {
                    if (chunk != null) i++;
                }
                Log.InfoFormat("[LevelFactory] World pre-cache {0} chunks completed in {1}ms", i,
                    chunkLoading.ElapsedMilliseconds);

                provider.PruneAir();
                provider.MakeAirChunksAroundWorldToCompensateForBadRendering();

                _providerCache.Add(levelDir, provider);
            }

            if (readOnly)
            {
                return provider;
            }
            else
            {
                return (AnvilWorldProvider) provider.Clone();
            }
        }

        public IEnumerable<McpeBatch> GenerateChunks(ChunkCoordinates chunkPosition,
            Dictionary<Tuple<int, int>, McpeBatch> chunksUsed, double radius, AnvilWorldProvider worldProvider)
        {
            lock (chunksUsed)
            {
                Dictionary<Tuple<int, int>, double> newOrders = new Dictionary<Tuple<int, int>, double>();

                double radiusSquared = Math.Pow(radius, 2);

                int centerX = chunkPosition.X;
                int centerZ = chunkPosition.Z;

                for (double x = -radius; x <= radius; ++x)
                {
                    for (double z = -radius; z <= radius; ++z)
                    {
                        var distance = (x*x) + (z*z);
                        if (distance > radiusSquared)
                        {
                            //continue;
                        }
                        int chunkX = (int) (x + centerX);
                        int chunkZ = (int) (z + centerZ);
                        Tuple<int, int> index = new Tuple<int, int>(chunkX, chunkZ);
                        newOrders[index] = distance;
                    }
                }

                foreach (var chunkKey in chunksUsed.Keys.ToArray())
                {
                    if (!newOrders.ContainsKey(chunkKey))
                    {
                        chunksUsed.Remove(chunkKey);
                    }
                }

                foreach (var pair in newOrders.OrderBy(pair => pair.Value))
                {
                    if (chunksUsed.ContainsKey(pair.Key)) continue;

                    if (worldProvider == null) continue;

                    ChunkColumn chunkColumn =
                        worldProvider.GenerateChunkColumn(new ChunkCoordinates(pair.Key.Item1, pair.Key.Item2));
                    McpeBatch chunk = null;
                    if (chunkColumn != null)
                    {
                        chunk = chunkColumn.GetBatch();
                    }

                    chunksUsed.Add(pair.Key, chunk);

                    yield return chunk;
                }
            }
        }
    }
}
