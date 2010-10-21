﻿#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in float height;
in vec3 normalFS;
in vec3 positionToLightFS;
in vec3 positionToEyeFS;
in float alphaScalar;
                 
out vec3 fragmentColor;

uniform vec4 og_diffuseSpecularAmbientShininess;
uniform vec3 u_color;

float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
{
    vec3 toReflectedLight = reflect(-toLight, normal);

    float diffuse = max(dot(toLight, normal), 0.0);
    float specular = max(dot(toReflectedLight, toEye), 0.0);
    specular = pow(specular, diffuseSpecularAmbientShininess.w);

    return (diffuseSpecularAmbientShininess.x * diffuse) +
            (diffuseSpecularAmbientShininess.y * specular) +
            diffuseSpecularAmbientShininess.z;
}

void main()
{
    vec3 normal = normalize(normalFS);
    vec3 positionToLight = normalize(positionToLightFS);
    vec3 positionToEye = normalize(positionToEyeFS);

	float intensity = LightIntensity(normal, positionToLight, positionToEye, og_diffuseSpecularAmbientShininess);
	
	fragmentColor = mix(vec3(u_color * intensity), vec3(0.0, 0.0, intensity), alphaScalar);
}
