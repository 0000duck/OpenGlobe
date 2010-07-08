#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 22) out;

uniform mat4 og_perspectiveProjectionMatrix;
uniform float og_pixelSizePerDistance;
uniform float og_perspectiveNearPlaneDistance;

void ClipLineSegmentToNearPlane(
    vec4 model0, 
    vec4 model1, 
    out vec4 clippedModel0, 
    out vec4 clippedModel1,
	inout int numPositions)
{
    float distanceTo0 = model0.z + og_perspectiveNearPlaneDistance;
    float distanceTo1 = model1.z + og_perspectiveNearPlaneDistance;
    if ((distanceTo0 * distanceTo1) < 0.0)
    {
	    //
		// One is front of and one is behind the near plane
		//
        float t = distanceTo0 / (distanceTo0 - distanceTo1);
        vec4 clipV = model0 + (t * (model1 - model0));
        if (distanceTo0 < 0.0)
        {
            clippedModel0 = model0;
            clippedModel1 = clipV;
			numPositions += 2;
        }
        else
        {
            clippedModel0 = clipV;
			++numPositions;
        }
    }
	else if (distanceTo0 < 0.0)
	{
	    //
		// Both are in front of the near plane
		//
        clippedModel0 = model0;
		++numPositions;
	}
}

void main()
{
    int numPositions = 0;
	vec4 clippedModel[5];
	ClipLineSegmentToNearPlane(gl_in[0].gl_Position, gl_in[1].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
	ClipLineSegmentToNearPlane(gl_in[1].gl_Position, gl_in[2].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
	ClipLineSegmentToNearPlane(gl_in[2].gl_Position, gl_in[3].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
	ClipLineSegmentToNearPlane(gl_in[3].gl_Position, gl_in[0].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);

    // normal
    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 cr = cross(v1, v0);
    cr = normalize(cr);

 //   if ((numPositions > 0) && (abs(cr.z) <= 0.017452406437283512819418978516316))
    if (numPositions > 0)
    {
        vec3 norm = vec3(cr.x, cr.y, 0.0);
		norm = normalize(norm);
		norm = norm * (50.0 * 0.5 * og_pixelSizePerDistance);

	    vec4 positions[10];
		vec3 n;
		int j = 0;
	    for (int i = 0; i < numPositions; ++i, j += 2)
		{
            n = norm * abs(clippedModel[i].z);
			positions[j] = og_perspectiveProjectionMatrix * vec4(clippedModel[i].xyz - n, 1.0);
			positions[j + 1] = og_perspectiveProjectionMatrix * vec4(clippedModel[i].xyz + n, 1.0);
		}

		//
		//
		//
		int limit = numPositions + numPositions;
		for (int i = 0; i < limit; i += 2)
		{
            gl_Position = positions[i + 1];
            EmitVertex();
            gl_Position = positions[i];
            EmitVertex();
		}
        gl_Position = positions[1];
        EmitVertex();
        gl_Position = positions[0];
        EmitVertex();
        EndPrimitive();

		//
		// Sides
		//
		if (numPositions == 4)
		{
            gl_Position = positions[2];
            EmitVertex();
            gl_Position = positions[0];
            EmitVertex();
            gl_Position = positions[4];
            EmitVertex();
            gl_Position = positions[6];
            EmitVertex();
			
	        EndPrimitive();

            gl_Position = positions[1];
            EmitVertex();
            gl_Position = positions[3];
            EmitVertex();
            gl_Position = positions[7];
            EmitVertex();
            gl_Position = positions[5];
            EmitVertex();

	        EndPrimitive();
		}
		else if (numPositions == 5)
		{
            gl_Position = positions[2];
            EmitVertex();
            gl_Position = positions[0];
            EmitVertex();
            gl_Position = positions[4];
            EmitVertex();
            gl_Position = positions[8];
            EmitVertex();
            gl_Position = positions[6];
            EmitVertex();
			
	        EndPrimitive();
			
            gl_Position = positions[1];
            EmitVertex();
            gl_Position = positions[3];
            EmitVertex();
            gl_Position = positions[9];
            EmitVertex();
            gl_Position = positions[5];
            EmitVertex();
            gl_Position = positions[7];
            EmitVertex();

	        EndPrimitive();
		}
		else // if (numPositions == 3)
		{
            gl_Position = positions[2];
            EmitVertex();
            gl_Position = positions[0];
            EmitVertex();
            gl_Position = positions[4];
            EmitVertex();
			
	        EndPrimitive();

            gl_Position = positions[1];
            EmitVertex();
            gl_Position = positions[3];
            EmitVertex();
            gl_Position = positions[5];
            EmitVertex();

	        EndPrimitive();
		}








        // vertex
  //      vec3 norm = cr * (0.5 * og_pixelSizePerDistance);
  //      vec4 vertices[8];

  //      vec3 n = norm * abs(gl_in[1].gl_Position.z);
   //     vertices[0] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz - n, 1.0);
  //      vertices[1] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz + n, 1.0);
  //      n = norm * abs(gl_in[0].gl_Position.z);
  //      vertices[2] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz + n, 1.0);
  //      vertices[3] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz - n, 1.0);
  //      n = norm * abs(gl_in[2].gl_Position.z);
  //      vertices[4] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz - n, 1.0);
  //      vertices[5] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz + n, 1.0);
 //       n = norm * abs(gl_in[3].gl_Position.z);
 //       vertices[6] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz + n, 1.0);
 //       vertices[7] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz - n, 1.0);
                    
  //      gl_Position = vertices[0];
  //      EmitVertex();
  //      gl_Position = vertices[1];
  //      EmitVertex();
  //      gl_Position = vertices[3];
  //      EmitVertex();
  //      gl_Position = vertices[2];
  //      EmitVertex();
  //      gl_Position = vertices[7];
  //      EmitVertex();
  //      gl_Position = vertices[6];
  //      EmitVertex();
  //      gl_Position = vertices[4];
  //      EmitVertex();
  //      gl_Position = vertices[5];
  //      EmitVertex();
  //      gl_Position = vertices[0];
  //      EmitVertex();
 //       gl_Position = vertices[1];
 //       EmitVertex();
  //      EndPrimitive();

//        gl_Position = vertices[2];
 //       EmitVertex();
//        gl_Position = vertices[1];
//        EmitVertex();
 //       gl_Position = vertices[6];
//        EmitVertex();
 //       gl_Position = vertices[5];
 //       EmitVertex();
 //       EndPrimitive();

  //      gl_Position = vertices[0];
  //      EmitVertex();
 //       gl_Position = vertices[3];
  //      EmitVertex();
  //      gl_Position = vertices[4];
  //      EmitVertex();
   //     gl_Position = vertices[7];
   //     EmitVertex();
   //     EndPrimitive();
    }
}








//#version 330 
////
//// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
////
//// Distributed under the Boost Software License, Version 1.0.
//// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
////

//layout(lines_adjacency) in;
//layout(triangle_strip, max_vertices = 18) out;

//uniform mat4 og_perspectiveProjectionMatrix;
//uniform float og_pixelSizePerDistance;

//void main()
//{
//    // normal
//    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
//    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
//    vec3 cr = cross(v1, v0);
//    cr = normalize(cr);

//    if (abs(cr.z) <= 0.017452406437283512819418978516316)
//    {
//        // vertex
//        vec3 norm = cr * (0.5 * og_pixelSizePerDistance);
//        vec4 vertices[8];

//        vec3 n = norm * abs(gl_in[1].gl_Position.z);
//        vertices[0] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz - n, 1.0);
//        vertices[1] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz + n, 1.0);
//        n = norm * abs(gl_in[0].gl_Position.z);
//        vertices[2] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz + n, 1.0);
//        vertices[3] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz - n, 1.0);
//        n = norm * abs(gl_in[2].gl_Position.z);
//        vertices[4] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz - n, 1.0);
//        vertices[5] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz + n, 1.0);
//        n = norm * abs(gl_in[3].gl_Position.z);
//        vertices[6] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz + n, 1.0);
//        vertices[7] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz - n, 1.0);
                    
//        gl_Position = vertices[0];
//        EmitVertex();
//        gl_Position = vertices[1];
//        EmitVertex();
//        gl_Position = vertices[3];
//        EmitVertex();
//        gl_Position = vertices[2];
//        EmitVertex();
//        gl_Position = vertices[7];
//        EmitVertex();
//        gl_Position = vertices[6];
//        EmitVertex();
//        gl_Position = vertices[4];
//        EmitVertex();
//        gl_Position = vertices[5];
//        EmitVertex();
//        gl_Position = vertices[0];
//        EmitVertex();
//        gl_Position = vertices[1];
//        EmitVertex();
//        EndPrimitive();

//        gl_Position = vertices[2];
//        EmitVertex();
//        gl_Position = vertices[1];
//        EmitVertex();
//        gl_Position = vertices[6];
//        EmitVertex();
//        gl_Position = vertices[5];
//        EmitVertex();
//        EndPrimitive();

//        gl_Position = vertices[0];
//        EmitVertex();
//        gl_Position = vertices[3];
//        EmitVertex();
//        gl_Position = vertices[4];
//        EmitVertex();
//        gl_Position = vertices[7];
//        EmitVertex();
//        EndPrimitive();
//    }
//}