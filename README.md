# Quran-Reading
Android/ios reading room creation - real time voice chatting

ios settings


1) Dissonance settings


1.Coppy AudioPluginInterface.h from Assets/Plugins and add it to your XCode project.
2.add #import "AudioPluginInterface.h"; to UnityAppController.mm in XCode.
3.Find the preStartUnity method and add the line UnityRegisterAudioPlugin(&UnityGetAudioEffectDefinitions);
