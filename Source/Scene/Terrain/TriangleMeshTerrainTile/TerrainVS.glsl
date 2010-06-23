﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec3 position;
                  
out float height;

uniform mat4 og_modelViewPerspectiveProjectionMatrix;
uniform float u_heightExaggeration;

void main()
{
    vec4 exaggeratedPosition = vec4(position.xy, position.z * u_heightExaggeration, 1.0);
    gl_Position = og_modelViewPerspectiveProjectionMatrix * exaggeratedPosition;
    height = exaggeratedPosition.z;
}