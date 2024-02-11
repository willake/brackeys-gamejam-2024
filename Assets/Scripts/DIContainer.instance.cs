using Game.Events;
using WillakeD.CommonPatterns;
using Game.Screens;
using Game.Saves;

namespace Game
{
    public partial class DIContainer : Singleton<DIContainer>
    {
        private void Awake()
        {
            SetUp();
        }

        protected void SetUp()
        {
            Register<EventManager>(() => new EventManager());
            Register<ScreenAspectManager>(() => new ScreenAspectManager());
        }
    }
}