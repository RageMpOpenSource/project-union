using RAGE;
using RAGE.NUI;
using System.Linq;

namespace ClientProjectUnion
{
    public class Main : Events.Script
    {
        public Main()
        {
            Events.Add("ShowCharacterSelectMenu", OnShowCharacterSelectMenu);
        }

        private void OnShowCharacterSelectMenu(object[] args)
        {
            Chat.Show(true);
            RAGE.Ui.Cursor.Visible = false;

            MenuPool menuPool = new MenuPool();
            UIMenu characterSelectMenu = new UIMenu($"Welcome {RAGE.Elements.Player.LocalPlayer.Name}!", "Please select a Character");

            menuPool.Add(characterSelectMenu);

            string[] characterIdentifiers = args[0].ToString().Split(",");
            int[] characterIds = characterIdentifiers.Select(e => int.Parse(e)).ToArray();

            dynamic[] characterNames = args[1].ToString().Split(",");
            var characterNamesList = characterNames.ToList();
            int selectedCharacterIndex = characterIds[0];

            UIMenuListItem characterSelect = new UIMenuListItem("Character", characterNamesList, 0);
            UIMenuItem createCharacterButton = new UIMenuItem("Create Character");
            UIMenuItem confirmButton = new UIMenuItem("Select Character");

            characterSelectMenu.AddItem(createCharacterButton);

            if (characterIds.Length > 0)
            {
                characterSelectMenu.AddItem(characterSelect);
                characterSelectMenu.AddItem(confirmButton);
            }

            characterSelectMenu.OnItemSelect += (UIMenu sender, UIMenuItem selectedItem, int index) =>
            {
                if (sender == characterSelectMenu)
                {
                    if (selectedItem == confirmButton)
                    {
                        RAGE.Events.CallRemote("CharacterSelected", selectedCharacterIndex);
                        CloseMenu();
                    }

                    if (selectedItem == createCharacterButton)
                    {
                        RAGE.Events.CallRemote("CharacterCreated");
                        CloseMenu();
                    }
                }
            };


            characterSelectMenu.OnListChange += (UIMenu sender, UIMenuListItem listItem, int newIndex) =>
            {
                if (sender == characterSelectMenu)
                {
                    if (listItem == characterSelect)
                    {
                        selectedCharacterIndex = characterIds[newIndex];
                    }
                }
            };

            characterSelectMenu.Visible = true;
            characterSelectMenu.FreezeAllInput = true;

            characterSelectMenu.RefreshIndex();

            Events.Tick += (name) =>
            {
                menuPool.ProcessMenus();
            };

            void CloseMenu()
            {
                Chat.Show(true);
                RAGE.Ui.Cursor.Visible = false;
                characterSelectMenu.Visible = false;
                characterSelectMenu.FreezeAllInput = false;

            }
        }


    }
}
