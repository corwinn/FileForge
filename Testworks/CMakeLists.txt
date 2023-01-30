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

cmake_minimum_required(VERSION 3.16)

set(TEST_SUITE Testworks)
project(${TEST_SUITE} LANGUAGES CSharp)
file(GLOB_RECURSE ${TEST_SUITE}_src RELATIVE ${CMAKE_CURRENT_SOURCE_DIR} "*.cs")
add_library(${TEST_SUITE} SHARED ${${TEST_SUITE}_src}
    Testworks.config
)

#set_property(TARGET ${TEST_SUITE} PROPERTY VS_CONFIGURATION_TYPE DynamicLibrary)
set_property(TARGET ${TEST_SUITE} PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.0")
set(CMAKE_CSharp_FLAGS "/d:FF_VALIDATION_SUITE /langversion:4")
set_property(TARGET ${TEST_SUITE} PROPERTY
    VS_DOTNET_REFERENCE_nunit.framework "${CMAKE_SOURCE_DIR}/3rd_party/testing/bin/net40/nunit.framework.dll")
# add reference(es) - the ones I  wasn't joking about
target_link_libraries(${TEST_SUITE} ${APP})

# populates the "References" sub-tree ; means you better have these in the GAC
set_property(TARGET ${TEST_SUITE} PROPERTY VS_DOTNET_REFERENCES
    "System"
    "System.Core"
    "System.Data"
    "System.Drawing"
    "System.Windows.Forms"
    "System.Xml"
    "System.Xml.Linq"
)
