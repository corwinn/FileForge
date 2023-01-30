# /**** BEGIN LICENSE BLOCK ****
#
# BSD 3-Clause License
#
# Copyright (c) 2022-2023, the wind.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
#
# 1. Redistributions of source code must retain the above copyright notice, this
#    list of conditions and the following disclaimer.
#
# 2. Redistributions in binary form must reproduce the above copyright notice,
#    this list of conditions and the following disclaimer in the documentation
#    and/or other materials provided with the distribution.
#
# 3. Neither the name of the copyright holder nor the names of its
#    contributors may be used to endorse or promote products derived from
#    this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
# DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
# FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
# DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
# SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
# CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
# OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
# OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

# **** END LICENCE BLOCK ****/

# the version I used ; feel free to comment this out, and face the consequences
cmake_minimum_required(VERSION 3.16)

# send ALL_BUILD to a folder specifying its purpose
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

set(APP FileForge)
project(${APP} LANGUAGES CSharp)

include(CSharpUtilities)

# disable ZERO_CHECK - "visual studio" has "cmake" support - use it instead
set(CMAKE_SUPPRESS_REGENERATION ON)

file(GLOB ${APP}_src RELATIVE ${CMAKE_SOURCE_DIR} "*.cs")
file(GLOB_RECURSE ${APP}_src2 RELATIVE ${CMAKE_SOURCE_DIR} "FFHexEditor/*.cs")
file(GLOB_RECURSE ${APP}_src3 RELATIVE ${CMAKE_SOURCE_DIR} "Selection/*.cs")
add_executable(${APP} ${${APP}_src} ${${APP}_src2} ${${APP}_src3}
    FileForge.config
)

set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT ${APP})

set_property(TARGET ${APP} PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.0")
# as strange as it may sound, this sets "Output type" to "Windowds Application"
set_property(TARGET ${APP} PROPERTY WIN32_EXECUTABLE TRUE)
set(CMAKE_CSharp_FLAGS "/d:TESTING_WORKS /langversion:4")

# TODO why is this needed?
csharp_set_windows_forms_properties(
    FormMain.cs
)

# add reference(es) - 3rd party only; there is another way to add the just built ones - I'm not joking
set_property(TARGET ${APP} PROPERTY
    VS_DOTNET_REFERENCE_WeifenLuo.WinFormsUI.Docking "3rd_party/docking/WeifenLuo.WinFormsUI.Docking.dll")
set_property(TARGET ${APP} PROPERTY
    VS_DOTNET_REFERENCE_Wind.Controls "../Library of the Wind/bin/Debug/Wind.Controls.dll")
set_property(TARGET ${APP} PROPERTY
    VS_DOTNET_REFERENCE_Wind.Debug "../Library of the Wind/bin/Debug/Wind.Debug.dll")
# set(WDEBUG Wind.Debug)
# target_link_libraries(${APP} ${WDEBUG})
# set(WCONTROLS Wind.Controls)
# target_link_libraries(${APP} ${WCONTROLS})

# populates the "References" sub-tree ; means you better have these in the GAC
set_property(TARGET ${APP} PROPERTY VS_DOTNET_REFERENCES
    "System"
    "System.Core"
    "System.Data"
    "System.Drawing"
    "System.Windows.Forms"
    "System.Xml"
    "System.Xml.Linq"
)
# _____________________________________________________________________________
#
# required, because "cmake" has no notion of "assembly"
set_property(TARGET ${APP} PROPERTY ENABLE_EXPORTS ON)

add_subdirectory("Testworks")
