
## Linux

#### Dependencies

To build the Unity plugin libraries for Linux, make sure to have those dependencies installed:

```sh
$ sudo apt-get install gcc-multilib g++-multilib cmake g++
```

#### Build

Then generate the Unity native plugin file for Linux for each of the architecture (`x86` and `x86_64`):

```sh
$ cmake . -Bbuild -DARCH=x86
$ make -c build/
$ make -c build/ install
```
```
$ cmake . -Bbuild -DARCH=x86_64
$ make -c build/
$ make -c build/ install
```

## Windows

#### Dependencies

Install CMake for Windows: [https://cmake.org/download/](https://cmake.org/download/)

#### Build

Generate the Visual Studio solution, open the project and run the build for each of the architectures (`x86` and `x86_64`):

```sh
$ cmake . -Bbuild -G "Visual Studio 14 2015 Win64" -DARCH=x86_64
```

```
$ cmake . -Bbuild -G "Visual Studio 14 2015 Win32" -DARCH=x86
```

## OSX

#### Dependencies

To generate the `.bundle` file needed by Unity on OSX you will need to first install CMake:

```sh
$ brew install cmake
```

#### Build

```sh
$ cmake . -Bbuild
$ make -c build/
$ make -c build/ install
```

## iOS

For iOS, Unity native plugin file will be injected to the XCode project and directly built with the rest of the Unity project files. The only step needed is to move those files under `Assets/Plugings/iOS` in the Unity project, which is done through an `install` step.

#### Build

```sh
$ cmake . -Bbuild -DIOS=1
$ make -c build/ install
```

## Android

#### Dependencies

To build the Unity plugin for Android you will need [Android Studio](https://developer.android.com/studio/index.html) version 2.2 or newer on Mac OS X, Ubuntu, or Windows 10. Using the Android Studio SDK Manager, install or update the 'CMake', 'LLDB', and 'NDK' packages from the 'SDK Tools'  tab.

After installing dependencies in Android Studio, you can execute Android builds from either the command line or the Android Studio interface.

#### Build (command line)

After installing dependencies in Android Studio, you can execute Android builds from either the command line or the Android Studio interface.

>Note: Make sure to open Android studio once to generate `local.properties`, this file is needed for gradle to find Android SDK and NDK paths.

For the build only target:

```sh
$ ./gradlew earcut:assembleFullRelease
```

To install and build the libraries:

```sh
$ ./gradlew earcut:installLibraries
```

>Note: If you have several versions of the JDKs installed on your machine, make sure that the Java version used in the Android builds is the same as the one used by Unity. (Can be checked at _Unity_ -> _Preferences_ -> _External Tools_).

## WebGL

Install emscripten SDK.

```sh
$ cmake . -Bbuild -DWEBGL=1 -DCMAKE_TOOLCHAIN_FILE=../emscripten.toolchain.cmake
$ make -c build/
$ make -c build/ install
```
