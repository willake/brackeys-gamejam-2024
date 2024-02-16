using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Game.Audios;

namespace Game.Gameplay
{
    public class MenuScene : GameScene
    {
        private void Start()
        {
            WrappedAudioClip musicMainTheme = ResourceManager.instance.audioResources.backgroundAudios.musicMainTheme;
            AudioManager.instance.PlayMusic(musicMainTheme.clip, musicMainTheme.volume);
            UIManager.instance.OpenUI(AvailableUI.MenuPanel);
            // ModalPanel modalPanel =
            // await UIManager.instance.OpenUIAsync(AvailableUI.ModalPanel) as ModalPanel;
            // modalPanel.SetContent(@"By playing this game, you have confirm to share your play data with us. To Note that we do not collect any personal information.");
        }
    }
}
