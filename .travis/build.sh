#!/bin/bash

set -e
set -o pipefail

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

    mkdir NativePlugin/build
    pushd NativePlugin/build
    cmake .. && make
    popd
fi

if [[ ${PLATFORM} == "linux" ]]; then
    mkdir NativePlugin/build
    pushd NativePlugin/build
    cmake .. -DARCH=x86 && make
    popd
fi

if [[ ${PLATFORM} == "android" ]]; then
    pushd NativePlugin
    ./gradlew earcut:assembleRelease
    popd
fi
