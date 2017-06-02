cmake_minimum_required(VERSION 2.8.12)
project(Earcut)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -O3")
set(NATIVE_PLUGIN Earcut)
set(SOURCES ${PROJECT_SOURCE_DIR}/unity_earcut.cpp)

add_library(${NATIVE_PLUGIN} MODULE ${SOURCES})

if(APPLE)
  set_target_properties(${NATIVE_PLUGIN} PROPERTIES BUNDLE TRUE)
  install(TARGETS ${NATIVE_PLUGIN} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins/)
elseif(WIN32)
  install(TARGETS ${NATIVE_PLUGIN} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins/x86_64/)
else()
  message(SEND_ERROR "Unsupported platform")
endif()
