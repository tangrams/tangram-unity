#!/bin/sh

set -e
set -o pipefail

if [[ ${PLATFORM} == "osx" ]]; then
    # grab a current link from: http://unity3d.com/get-unity/download/archive
    echo 'Downloading Unity 5.6.1f1: '
    curl -o Unity.pkg http://netstorage.unity3d.com/unity/2860b30f0b54/MacEditorInstaller/Unity-5.6.1f1.pkg

    echo 'Installing Unity.pkg'
    sudo installer -dumplog -package Unity.pkg -target /
fi

if [[ ${PLATFORM} == "android" ]]; then
    # Note: the right way to download these packages is through the Android Studio SDK manager,
    # these steps should be removed when/if ndk-bundle and cmake become available from the
    # command-line SDK update tool.

    # Download android ndk
    echo "Downloading ndk..."
    curl -L https://dl.google.com/android/repository/android-ndk-r13b-linux-x86_64.zip -o ndk.zip
    echo "Done."

    # Extract android ndk
    echo "Extracting ndk..."
    unzip -qq ndk.zip
    echo "Done."

    # Update PATH
    echo "Updating PATH..."
    export ANDROID_NDK_HOME=${PWD}/android-ndk-r13b
    export PATH=${PATH}:${ANDROID_HOME}/tools:${ANDROID_NDK_HOME}
    echo $PATH
    echo "Done."

    # Copy a license file into the SDK (needed to install CMake).
    mkdir -p ${ANDROID_HOME}/licenses
    cp .travis/android-sdk-license ${ANDROID_HOME}/licenses/
fi
