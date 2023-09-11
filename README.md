# Quran-Reading
Android/ios reading room creation - real time voice chatting

iOS settings


1. Dissonance settings

a. Coppy AudioPluginInterface.h from Assets/Plugins and add it to your XCode project.

b. add #import "AudioPluginInterface.h"; to UnityAppController.mm in XCode.

c. Find the preStartUnity method and add the line UnityRegisterAudioPlugin(&UnityGetAudioEffectDefinitions);


2. Admob ios resolver

https://github.com/googlesamples/unity-jar-resolver#ios-resolver-usage
