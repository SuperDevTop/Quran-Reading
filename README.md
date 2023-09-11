# Quran-Reading
Android/ios reading room creation - real time voice chatting

ios

Download AudioPluginInterface.h from the Unity native audio plugin SDK and add it to your XCode project.
add #import "AudioPluginInterface.h"; to UnityAppController.mm in XCode.
Find the preStartUnity method and add the line UnityRegisterAudioPlugin(&UnityGetAudioEffectDefinitions);
