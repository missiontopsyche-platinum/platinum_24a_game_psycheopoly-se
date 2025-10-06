using UnityEngine;
using System;

namespace PsycheOpoly.Events
{
    //Completes task 88(subscribe), 89(unsubscribe), 90 (event to handle)
    public class GameEvents
    {
        //args (playerId, spacesToMove)
        //tasks 88 and 89, event BoardManger will subscribe to in OnEnable()
        public static event Action<int,int> PlayerMoved;

        //Task 90: Helped to raise the event when it is called
        public static void RaisePlayerMoved(int playerId, int spacesToMove)
            => PlayerMoved?.Invoke(playerId, spacesToMove);
    }

}