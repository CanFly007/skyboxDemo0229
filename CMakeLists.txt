cmake_minimum_required (VERSION 3.0)
cmake_policy(VERSION 3.0)

project(SkyboxDemoTest)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS ON)

IF(NOT CMAKE_BUILD_TYPE)
  SET(CMAKE_BUILD_TYPE Debug CACHE STRING "Choose the type of build (Debug or Release)" FORCE)
ENDIF(NOT CMAKE_BUILD_TYPE)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_SOURCE_DIR}/cmake/")

if(WIN32)
	set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/bin)
endif(WIN32)

link_directories(${CMAKE_SOURCE_DIR}/lib)

find_package(GLM REQUIRED)
message(STATUS "GLM included at ${GLM_INCLUDE_DIR}")
find_package(GLFW3 REQUIRED)
message(STATUS "Found GLFW3 in ${GLFW3_INCLUDE_DIR}")
find_package(ASSIMP REQUIRED)
message(STATUS "Found ASSIMP in ${ASSIMP_INCLUDE_DIR}")
if(APPLE)
  find_package(Freetype REQUIRED)
  message(STATUS "Found Freetype in ${FREETYPE_INCLUDE_DIRS}")
endif(APPLE)

if(WIN32)
    set(LIBS glfw3 opengl32 assimp freetype irrKlang)
    add_definitions(-D_CRT_SECURE_NO_WARNINGS)
elseif(UNIX AND NOT APPLE)
    set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -Wall")
    find_package(OpenGL REQUIRED)
    add_definitions(${OPENGL_DEFINITIONS})
    find_package(X11 REQUIRED)
    # note that the order is important for setting the libs
    # use pkg-config --libs $(pkg-config --print-requires --print-requires-private glfw3) in a terminal to confirm
    set(LIBS ${GLFW3_LIBRARY} X11 Xrandr Xinerama Xi Xxf86vm Xcursor GL dl pthread freetype ${ASSIMP_LIBRARY})
    set (CMAKE_CXX_LINK_EXECUTABLE "${CMAKE_CXX_LINK_EXECUTABLE} -ldl")
elseif(APPLE)
    INCLUDE_DIRECTORIES(/System/Library/Frameworks)
    FIND_LIBRARY(COCOA_LIBRARY Cocoa)
    FIND_LIBRARY(OpenGL_LIBRARY OpenGL)
    FIND_LIBRARY(IOKit_LIBRARY IOKit)
    FIND_LIBRARY(CoreVideo_LIBRARY CoreVideo)
    MARK_AS_ADVANCED(COCOA_LIBRARY OpenGL_LIBRARY)
    SET(APPLE_LIBS ${COCOA_LIBRARY} ${IOKit_LIBRARY} ${OpenGL_LIBRARY} ${CoreVideo_LIBRARY})
    SET(APPLE_LIBS ${APPLE_LIBS} ${GLFW3_LIBRARY} ${ASSIMP_LIBRARY} ${FREETYPE_LIBRARIES})
    set(LIBS ${LIBS} ${APPLE_LIBS})
else()
    set(LIBS )
endif(WIN32)

configure_file(configuration/root_directory.h.in configuration/root_directory.h)
include_directories(${CMAKE_BINARY_DIR}/configuration)
include_directories(${CMAKE_SOURCE_DIR}/includes)

add_library(STB_IMAGE "src/stb_image.cpp")
add_library(GLAD "src/glad.c")
set(LIBS ${LIBS} STB_IMAGE GLAD)


# ${chapter}
# 6.pbr

# ${demo}
# 2.1.2.ibl_irradiance

# ${NAME}
# SkyboxDemoTest

file(GLOB SOURCE
    "src/skyboxDemo/*.h"
    "src/skyboxDemo/*.cpp"
    "src/skyboxDemo/*.vs"
    "src/skyboxDemo/*.fs"
    "src/skyboxDemo/*.compute"
)
add_executable(SkyboxDemoTest ${SOURCE})
target_link_libraries(SkyboxDemoTest ${LIBS})

if(MSVC)
	target_compile_options(SkyboxDemoTest PRIVATE /std:c++17 /MP)
    target_link_options(SkyboxDemoTest PUBLIC /ignore:4099)
endif(MSVC)

if(WIN32)
    set_target_properties(SkyboxDemoTest PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/bin/6.pbr")
    set_target_properties(SkyboxDemoTest PROPERTIES VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/bin/6.pbr/Debug")
elseif(UNIX AND NOT APPLE)
    set_target_properties(SkyboxDemoTest PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/bin/6.pbr")
elseif(APPLE)
    set_target_properties(SkyboxDemoTest PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/bin/6.pbr")
    set_target_properties(SkyboxDemoTest PROPERTIES RUNTIME_OUTPUT_DIRECTORY_DEBUG "${CMAKE_SOURCE_DIR}/bin/6.pbr")
    set_target_properties(SkyboxDemoTest PROPERTIES RUNTIME_OUTPUT_DIRECTORY_RELEASE "${CMAKE_SOURCE_DIR}/bin/6.pbr")
endif(WIN32)

file(GLOB SHADERS
    "src/skyboxDemo/*.vs"
    "src/skyboxDemo/*.fs"
    "src/skyboxDemo/*.compute"
)

file(GLOB DLLS "dlls/*.dll")

macro(makeLink src dest target)
    add_custom_command(TARGET ${target} POST_BUILD COMMAND ${CMAKE_COMMAND} -E create_symlink ${src} ${dest}  DEPENDS  ${dest} COMMENT "mklink ${src} -> ${dest}")
endmacro()

foreach(SHADER ${SHADERS})
    if(WIN32)
        add_custom_command(TARGET SkyboxDemoTest PRE_BUILD COMMAND ${CMAKE_COMMAND} -E copy ${SHADER} $<TARGET_FILE_DIR:SkyboxDemoTest>)
		add_custom_command(TARGET SkyboxDemoTest PRE_BUILD COMMAND ${CMAKE_COMMAND} -E copy ${DLLS} $<TARGET_FILE_DIR:SkyboxDemoTest>)
    elseif(UNIX AND NOT APPLE)
        file(COPY ${SHADER} DESTINATION ${CMAKE_SOURCE_DIR}/bin/6.pbr)
    elseif(APPLE)
        get_filename_component(SHADERNAME ${SHADER} NAME)
        makeLink(${SHADER} ${CMAKE_SOURCE_DIR}/bin/6.pbr/${SHADERNAME} SkyboxDemoTest)
    endif(WIN32)
endforeach(SHADER)

if(MSVC)
    configure_file(${CMAKE_SOURCE_DIR}/configuration/visualstudio.vcxproj.user.in ${CMAKE_CURRENT_BINARY_DIR}/SkyboxDemoTest.vcxproj.user @ONLY)
endif(MSVC)