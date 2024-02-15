namespace Game.Events
{
    public class EventNames
    {
        /*
        args
        {
            AvaliableScene scene
        }
        */
        public static EventName loadScene = new EventName("LOAD_SCENE", false);
        /*
        args
        {
            bool isWin
        }
        */
        public static EventName onGameEnd = new EventName("ON_GAME_END", false);
        /*
        args
        {
            string text
        }
        */
        public static EventName presentDialogue = new EventName("PRESENT_DIALOGUE", false);
    }
}