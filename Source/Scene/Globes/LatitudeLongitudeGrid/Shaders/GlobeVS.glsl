﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec4 position;
out vec3 worldPosition;
out vec3 positionToLight;
out vec3 positionToEye;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_cameraEye;
uniform vec3 og_cameraLightPosition;

void main()                     
{
    gl_Position = og_modelViewPerspectiveMatrix * position; 

    worldPosition = position.xyz;
    positionToLight = og_cameraLightPosition - worldPosition;
    positionToEye = og_cameraEye - worldPosition;
}