﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
              
uniform float u_halfLineWidth;
uniform vec3 u_color;
noperspective in vec3 distanceToEdges;
out vec4 fragmentColor;

void main()
{
    float d = min(distanceToEdges.x, min(distanceToEdges.y, distanceToEdges.z));

    if (d > u_halfLineWidth + 1.0)
    {
        discard;
    }

    d = clamp(d - (u_halfLineWidth - 1.0), 0.0, 2.0);
    fragmentColor = vec4(u_color, exp2(-2.0 * d * d));
}