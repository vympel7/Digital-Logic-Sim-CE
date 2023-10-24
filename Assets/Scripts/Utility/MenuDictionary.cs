using System;

namespace Assets.Scripts.Utility
{
    [Serializable]
    public class MenuDictionary : SerializableDictionary<UI.MenuType, UI.Menu.UIMenu>
    {
    }
}