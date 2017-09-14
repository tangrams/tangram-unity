#!/bin/bash

set -e
set -o pipefail

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
