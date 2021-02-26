using UnityEngine;

namespace JsonChatWithDatabase
{
    public class UserData : MonoBehaviour
    {
        public string UserID;
        public string Password;
        public string Name;
        public string CurrentRoom;

        public RoomManageer RM;
    }
}