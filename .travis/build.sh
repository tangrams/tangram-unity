#!/bin/bash

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
