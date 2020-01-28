using RAGE;
using RAGE.NUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ClientSelectCharacter
{
    public class Main : Events.Script
    {

        public Main()
        {
            Events.Add("SelectCharacter", ShowSelectCharacterMenu);
            Events.Add("StartPlayerSwitch", StartPlayerSwitch);
            Events.Add("PedCreated", OnPedCreated);
        }

        private void OnPedCreated(object[] args)
        {
            RAGE.Elements.Ped ped = new RAGE.Elements.Ped(((uint)(int)args[0]), (Vector3)args[1], (float)args[2]);
        }

        private RAGE.Elements.Ped ped = null;

        private void StartPlayerSwitch(object[] args)
        {
            int currentPedId = RAGE.Game.Player.GetPlayerPed();
            RAGE.Game.Streaming.StartPlayerSwitch(currentPedId, currentPedId, 1, 1);
        }

        private void ShowSelectCharacterMenu(object[] args)
        {

            RAGE.Chat.Output("Show select character menu!");

            MenuPool menuPool = new MenuPool();

            UIMenu characterSelectionMenu = new UIMenu("Character Select", "Select a character!");
            menuPool.Add(characterSelectionMenu);


            UIMenuItem createCharacterButton = new UIMenuItem("Create Character");
            characterSelectionMenu.AddItem(createCharacterButton);

            uint[] characterIds = (args[0].ToString()).Split(",").Select(e => UInt32.Parse(e)).ToArray();
            uint selectedCharacterId = characterIds[0];
            List<dynamic> characterNames = (args[1].ToString()).Split(",").ToList<dynamic>();

            UIMenuListItem characterNamesList = new UIMenuListItem("Character", characterNames, 0);
            UIMenuItem selectCharacterButton = new UIMenuItem("Select Character");

            if (characterIds.Length > 0)
            {
                characterSelectionMenu.AddItem(characterNamesList);
                characterSelectionMenu.AddItem(selectCharacterButton);
            }


            characterSelectionMenu.OnListChange += (UIMenu sender, UIMenuListItem listItem, int newIndex) =>
            {
                if (sender == characterSelectionMenu)
                {
                    if (listItem == characterNamesList)
                    {
                        selectedCharacterId = characterIds[newIndex];
                    }
                }
            };


            characterSelectionMenu.OnItemSelect += (UIMenu sender, UIMenuItem selectedItem, int index) =>
            {
                if (sender == characterSelectionMenu)
                {
                    if (selectedItem == createCharacterButton)
                    {
                        CloseMenu();
                        RAGE.Events.CallRemote("CreateCharacter");
                    }

                    if (selectedItem == selectCharacterButton)
                    {
                        CloseMenu();
                        RAGE.Events.CallRemote("SelectCharacter", selectedCharacterId);
                    }
                }
            };

            ShowMenu();

            void CloseMenu()
            {
                RAGE.Ui.Cursor.Visible = false;
                RAGE.Chat.Show(true);
                characterSelectionMenu.FreezeAllInput = false;
                characterSelectionMenu.Visible = false;
            }


            void ShowMenu()
            {
                RAGE.Ui.Cursor.Visible = true;
                RAGE.Chat.Show(false);

                characterSelectionMenu.FreezeAllInput = true;
                characterSelectionMenu.Visible = true;
                characterSelectionMenu.RefreshIndex();
            }



            Events.Tick += (name) =>
            {
                menuPool.ProcessMenus();
            };
        }
    }
}
