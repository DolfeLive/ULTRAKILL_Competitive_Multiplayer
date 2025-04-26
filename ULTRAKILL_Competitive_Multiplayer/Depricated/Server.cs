//using Steamworks;
//using System.Collections.Generic;
//using UnityEngine;
//using Clogger = UltraIDK.Logger;

//namespace UltraIDK
//{
//    public class Serveier // Read it like its french, also yes i named it this on purpose
//    {
//        public List<Friend> besties = new List<Friend>(); // People in lobby

//        public Serveier()
//        {
//            SteamManager.instance.dataLoop = SteamManager.instance.StartCoroutine(SteamManager.instance.DataLoopInit());
//        }
//        public void Send(DataPacket data)
//        {
//            byte[] serializedData = data.Serialize();

//            foreach (var bestie in besties)
//            {
//                var peerId = bestie.Id;
//                bool success = SteamNetworking.SendP2PPacket(
//                    peerId,
//                    serializedData,
//                    serializedData.Length,
//                    0,
//                    P2PSend.Reliable
//                );

//                if (!success)
//                {
//                    Clogger.LogError($"Failed to send P2P packet to {peerId}", false);
//                }
//            }
//        }
//    }


//}
