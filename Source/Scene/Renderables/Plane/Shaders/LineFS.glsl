﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

out vec3 fragmentColor;
uniform vec3 u_color;

void main()
{
    fragmentColor = u_color;
}