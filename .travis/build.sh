#!/bin/bash

if [[ ${PLATFORM} == "osx" ]]; then
    /Applications/Unity/Unity.app/Contents/MacOS/Unity \
        -batchmode \
        -nographics \
        -silent-crashes \
        -logFile $(pwd)/unity.log \
        -projectpath $(pwd) \
        -buildOSXUniversalPlayer "$(pwd)/tangram-unity.app" \
        -quit

    echo 'Logs from build'
    cat $(pwd)/unity.log

    cd NativePlugin
    cmake . -Bbuild
    make -c build/
fi

if [[ ${PLATFORM} == "linux" ]]; then
    cd NativePlugin
    cmake . -Bbuild -DARCH=x86
    make -c build/
fi

if [[ ${PLATFORM} == "android" ]]; then
    cd NativePlugin
    ./gradlew earcut:assembleFullRelease
fi
