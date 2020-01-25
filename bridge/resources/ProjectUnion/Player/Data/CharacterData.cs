using System;
using GTANetworkAPI;

namespace ProjectUnion.Player.Data
{

    /// <summary>
    /// This class contains the Data for the characters a master account can hold.
    /// Every player can have x number of characters in 1 master account.
    /// </summary>

    public class CharacterData
    {
        /// <summary>The SQL ID for a character is stored in this attribute.</summary>
        public int Id { get; set; }
        /// <summary>The Master account ID which is linked to the current character is stored in this attribute.</summary>
        public int PlayerId { get; set; }
        /// <summary>Private attribute for spawning a character.</summary>
        private Vector3 _spawnPosition;
        /// <summary>
        /// This holds the position of the character's spawn location.
        /// <returns>The getter gets the <see cref="Vector3"/> last updated spawn poosition.</returns>
        /// <param>The setter sets the <see cref="Vector3"/> position of a player and updates the coloumn for the specific player.</param>
        /// </summary>
        public Vector3 SpawnPosition
        {
            get
            {
                return this._spawnPosition;
            }
            set
            {
                // TO-DO: make a function to save data and use it to update the location in the db //
                this._spawnPosition = value;
            }
        }
        /// <summary>Contains the cash the character of the player has.</summary>
        private long _cash;
        /// <summary>
        /// Contains the cash of the chatacter of the player.
        /// <param>Sets the cash of the player. And updates the DB data.</param>
        /// <returns>Gets the latest updated cash of the character.</returns>
        /// </summary>
        public long Cash
        {
            get
            {
                return this._cash;
            }
            set
            {
                // TO-DO: make a function for updating character data and update the cash by invoking the function //
                // And to update a scaleform as well //
                this._cash = value;
            }
        }

    }
}
