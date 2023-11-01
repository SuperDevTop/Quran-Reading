# Quran-Reading

Android/ios reading room creation - real time voice chatting

Google Play : https://play.google.com/store/apps/details?id=com.company.QuranReading

Apple Store : https://apps.apple.com/app/quran-reading-live/id6466713587

//////////////////////////////////////////////////

iOS settings


1. Dissonance settings

a. Coppy AudioPluginInterface.h from Assets/Plugins and add it to your XCode project.

b. add #import "AudioPluginInterface.h"; to UnityAppController.mm in XCode.

c. Find the preStartUnity method and add the line UnityRegisterAudioPlugin(&UnityGetAudioEffectDefinitions);


2. Admob ios resolver

https://github.com/googlesamples/unity-jar-resolver#ios-resolver-usage

3. Deep links

https://www.youtube.com/watch?v=bKiwNfNqKAc

https://docs.unity3d.com/Manual/deep-linking.html

sample invitation link

https://app.quranreadinglive.com???Guest#8271???All

4. Voxel Buster

https://assetstore.unity.com/packages/tools/integration/cross-platform-native-plugins-essential-kit-free-version-140137
