using System;
using UnityEngine;

namespace PsycheOpoly.Board
{
    [Serializable]
    public abstract class Space
    {
        //Task 72 in user story 15
        //Adding name property to space
        [SerializeField] private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        protected Space(string name) => this.name = name;

        //Task 73 in user story 15
        //Added abstract OnLanded(player) method
        public abstract void OnLanded(Player player);
    }
}
