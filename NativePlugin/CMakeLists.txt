cmake_minimum_required(VERSION 2.8.12)
project(Earcut)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -O3")
set(NATIVE_PLUGIN Earcut)
set(UNITY_ASSET_PATH ${PROJECT_SOURCE_DIR}/../Assets/Plugins)

file(GLOB SOURCES "code/*.h" "code/*.cpp")

if(IOS)
  message(STATUS "iOS native plugin files will be copied to ${UNITY_ASSET_PATH}/iOS")
  file(COPY ${SOURCES} DESTINATION ${UNITY_ASSET_PATH}/iOS/)
elseif(ANDROID)
  add_library(${NATIVE_PLUGIN} SHARED ${SOURCES})
elseif(APPLE)
  add_library(${NATIVE_PLUGIN} MODULE ${SOURCES})
  set_target_properties(${NATIVE_PLUGIN} PROPERTIES
    BUNDLE TRUE
    MACOSX_BUNDLE_INFO_PLIST ${PROJECT_SOURCE_DIR}/Info.plist)
  install(TARGETS ${NATIVE_PLUGIN} DESTINATION ${UNITY_ASSET_PATH})
elseif(WIN32)
  add_library(${NATIVE_PLUGIN} MODULE ${SOURCES})
  install(TARGETS ${NATIVE_PLUGIN} DESTINATION ${UNITY_ASSET_PATH}/x86_64/)
else()
  message(SEND_ERROR "Unsupported platform")
endif()
