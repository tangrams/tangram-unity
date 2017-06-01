cmake_minimum_required(VERSION 3.0)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -O3")
set(NATIVE_PLUGIN Earcut)
set(SOURCES ${PROJECT_SOURCE_DIR}/unity_earcut.cpp)

if(APPLE)
  add_library(${NATIVE_PLUGIN} MODULE ${SOURCES})
  set_target_properties(${NATIVE_PLUGIN} PROPERTIES BUNDLE TRUE)
  install(TARGETS ${NATIVE_PLUGIN} DESTINATION ${PROJECT_SOURCE_DIR}/../Assets/Plugins/)
else()
  message(SEND_ERROR "Unsupported platform")
endif()
