using RAGE;
using RAGE.NUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientSelectCharacter
{
    public class ClientSelectCharacter : Events.Script
    {

        private RAGE.Ui.HtmlWindow createCharacterWindow = null;

        public ClientSelectCharacter()
        {
            Events.Add("SelectCharacter", ShowSelectCharacterMenu);

            Events.Add("ToggleCreateCharacterMenu", ToggleCreateCharacterMenu);
            Events.Add("CharacterInfoFromCEF", SendCreateCharInfoToServer);
            Events.Add("CharacterNameExists", OnCharacterNameExists);
            Events.Add("GoBackToCharacterSelection", OnGoBackToCharacterSelection);


            createCharacterWindow = new RAGE.Ui.HtmlWindow("package://cs_packages/ClientSelectCharacter/SelectCharacter.html");
            createCharacterWindow.Active = false;
        }


        private void OnGoBackToCharacterSelection(object[] args)
        {
            RAGE.Events.CallRemote("GoBackToCharacterSelection");
        }

        private void OnCharacterNameExists(object[] args)
        {
            createCharacterWindow.ExecuteJs("CharacterNameExists()");
        }

        private void SendCreateCharInfoToServer(object[] args)
        {
            ToggleCreateCharacterMenu(new object[] { false });
            RAGE.Events.CallRemote("CreateCharacter", (string)args[0]);
        }

        private void ToggleCreateCharacterMenu(object[] args)
        {
            bool flag = (bool)args[0];
            RAGE.Chat.Show(!flag);
            RAGE.Ui.Cursor.Visible = flag;
            createCharacterWindow.Active = flag;
            RAGE.Elements.Player.LocalPlayer.FreezePosition(flag);
            RAGE.Elements.Player.LocalPlayer.FreezeCameraRotation();
        }

        private void ShowSelectCharacterMenu(object[] args)
        {
            MenuPool menuPool = new MenuPool();

            UIMenu characterSelectionMenu = new UIMenu("Character Select", "Select a character!");
            menuPool.Add(characterSelectionMenu);


            UIMenuItem createCharacterButton = new UIMenuItem("Create Character");
            characterSelectionMenu.AddItem(createCharacterButton);


            uint[] characterIds = null;
            List<dynamic> characterNames = null;
            UIMenuListItem characterNamesList = null;
            UIMenuItem selectCharacterButton = null;
            uint selectedCharacterId = 0;

            if (string.IsNullOrEmpty(args[0].ToString()) == false)
            {
                characterIds = (args[0].ToString()).Split(",").Select(e => UInt32.Parse(e)).ToArray();
                characterNames = (args[1].ToString()).Split(",").ToList<dynamic>();
                selectedCharacterId = characterIds[0];

                characterNamesList = new UIMenuListItem("Character", characterNames, 0);
                selectCharacterButton = new UIMenuItem("Select Character");

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
                        ToggleCreateCharacterMenu(new object[] { true });
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
                RAGE.Elements.Player.LocalPlayer.FreezePosition(false);
            }


            void ShowMenu()
            {
                RAGE.Ui.Cursor.Visible = true;
                RAGE.Chat.Show(false);

                characterSelectionMenu.FreezeAllInput = true;
                characterSelectionMenu.Visible = true;
                characterSelectionMenu.RefreshIndex();
                RAGE.Elements.Player.LocalPlayer.FreezePosition(true);
            }



            Events.Tick += (name) =>
            {
                menuPool.ProcessMenus();
            };

            characterSelectionMenu.OnMenuClose += (UIMenu sender) =>
            {
                if (sender == characterSelectionMenu)
                {
                    ShowMenu();
                }
            };
        }


    }
}
