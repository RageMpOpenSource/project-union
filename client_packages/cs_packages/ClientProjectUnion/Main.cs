using RAGE;
using RAGE.NUI;
using System.Collections.Generic;
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


            RAGE.Chat.Output("Character Identifiers " + characterIdentifiers.Length);


            int selectedCharacterIndex = characterIds[0];
            UIMenuListItem characterSelect = new UIMenuListItem("Character", characterNamesList, 0);
            characterSelectMenu.AddItem(characterSelect);

            UIMenuItem confirmButton = new UIMenuItem("Select Character");
            characterSelectMenu.AddItem(confirmButton);

            characterSelectMenu.OnItemSelect += (UIMenu sender, UIMenuItem selectedItem, int index) =>
            {
                if (sender == characterSelectMenu)
                {
                    if (selectedItem == confirmButton)
                    {
                        Chat.Show(true);
                        RAGE.Ui.Cursor.Visible = false;
                        RAGE.Events.CallRemote("CharacterSelected", selectedCharacterIndex);
                        characterSelectMenu.Visible = false;
                        characterSelectMenu.FreezeAllInput = false;
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
        }

    }
}
