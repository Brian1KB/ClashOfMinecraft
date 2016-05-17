using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using MiNET;

namespace ClashOfMinecraft
{
    internal class CrLevelManager : LevelManager
    {
        private readonly Dictionary<string, SpawnLocations> _spawnLocationsCache = new Dictionary<string, SpawnLocations>(); 

        public Player GetPlayer(string name)
        {
            foreach (var level in Levels)
            {
                foreach (var player in level.Players.Values)
                {
                    if (player.Username.ToLower().Equals(name.ToLower()))
                    {
                        return player;
                    }
                }
            }

            return null;
        }

        public SpawnLocations GetSpawnLocations(string locationJson)
        {
            SpawnLocations locations;
            _spawnLocationsCache.TryGetValue(locationJson, out locations);

            if (locations == null)
            {
                try
                {
                    using (var stream = new FileStream(locationJson, FileMode.Open))
                    {
                        var ser = new DataContractJsonSerializer(typeof(SpawnLocationsJson));
                        var config = (SpawnLocationsJson)ser.ReadObject(stream);

                        locations = config.ParseLocations();
                        _spawnLocationsCache.Add(locationJson, locations);
                    }
                }
                catch (Exception e)
                {
                }
            }

            return locations;
        }
    }
}
