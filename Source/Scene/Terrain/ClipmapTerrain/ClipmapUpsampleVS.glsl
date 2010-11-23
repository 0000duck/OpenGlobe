﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec2 position;

out vec2 fsTextureCoordinates;

uniform mat4 og_viewportOrthographicMatrix;
uniform vec2 u_sourceOrigin;
uniform vec2 u_updateSize;
uniform vec2 u_destinationOffset;
uniform vec2 u_oneOverHeightMapSize;

void main()                     
{
    vec2 scaledPosition = position * u_updateSize;
    gl_Position = og_viewportOrthographicMatrix * vec4(scaledPosition + u_destinationOffset, 0.0, 1.0);
    fsTextureCoordinates = (scaledPosition * 0.5 + u_sourceOrigin) * u_oneOverHeightMapSize;
}