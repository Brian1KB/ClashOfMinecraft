using System;
using System.Runtime.Serialization;
using MiNET.Utils;

namespace ClashOfMinecraft
{
    [DataContract]
    internal class SpawnLocationsJson
    {
        [DataMember]
        public string LocationOne { get; set; }
        [DataMember]
        public string LocationTwo { get; set; }

        public SpawnLocations ParseLocations()
        {
            float[] locationOneSplit = Array.ConvertAll(LocationOne.Split(':'), float.Parse);
            float[] locationTwoSplit = Array.ConvertAll(LocationTwo.Split(':'), float.Parse);

            var playerLocationOne = new PlayerLocation(locationOneSplit[0], locationOneSplit[1], locationOneSplit[2])
            {
                Pitch = locationOneSplit[3],
                Yaw = locationOneSplit[4],
                HeadYaw = locationOneSplit[4]
            };

            var playerLocationTwo = new PlayerLocation(locationTwoSplit[0], locationTwoSplit[1], locationTwoSplit[2])
            {
                Pitch = locationTwoSplit[3],
                Yaw = locationTwoSplit[4],
                HeadYaw = locationTwoSplit[4]
            };

            return new SpawnLocations(playerLocationOne, playerLocationTwo);
        }
    }

    internal class SpawnLocations
    {
        public PlayerLocation PlayerLocationOne;
        public PlayerLocation PlayerLocationTwo;

        public SpawnLocations(PlayerLocation playerLocationOne, PlayerLocation playerLocationTwo)
        {
            PlayerLocationOne = playerLocationOne;
            PlayerLocationTwo = playerLocationTwo;
        }
    }
}
