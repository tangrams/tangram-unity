#!/bin/sh

if [[ ${PLATFORM} == "osx" ]]; then
    # grab a current link from: http://unity3d.com/get-unity/download/archive
    echo 'Downloading Unity 5.6.1f1: '
    curl -o Unity.pkg http://netstorage.unity3d.com/unity/2860b30f0b54/MacEditorInstaller/Unity-5.6.1f1.pkg

    echo 'Installing Unity.pkg'
    sudo installer -dumplog -package Unity.pkg -target /
fi
