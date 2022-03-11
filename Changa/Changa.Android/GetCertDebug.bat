cd "C:\Program Files\Android\jdk\microsoft_dist_openjdk_1.8.0.25\bin"
keytool.exe -list -v -keystore "%LocalAppData%\Xamarin\Mono for Android\debug.keystore" -alias androiddebugkey -storepass android -keypass android
pause