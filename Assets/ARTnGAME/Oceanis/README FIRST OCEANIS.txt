OCEANIS Pro Water Framework

Quick Setup Guide

- A sample URP pipeline and a URP Forward Renderer that has pre-setup the three image effects (Fog, Volume Lighting, Distortion)
is included in the following folder
"Assets -> ARTnGAME -> Oceanis URP -> Installation -> Sample URP Pipeline -> UniversalRenderPipelineAsset OCEANIS"

- To setup the system, create an empty gameobject, rename to OCEANIS and add the "GlobalOceanisControllerURP" script to the object.
Then refer to the manual for details on how to setup the individual modules and tothe tutorial video playlist
https://www.youtube.com/watch?v=uRD08e8e-HU&list=PLJQvoQM6t9GejhKWf4NgDl3EKlERRcSU3&index=1

- Also may directly insert a fully ready to use version of the system as prefab, found in the following folder
"Assets -> ARTnGAME -> Oceanis URP -> PREFABS -> FULL OCEAN PREFABS"

- Add a layer called "WaterMask", the "Ocean mesh MASK" mesh must be in this layer and the "AIR-WATER MASK Camera" camera must render
only this layer. The main camera must not render this layer. Refer to the manual for more details.

- The water reflections can also be controlled by layer, add a "Terrain" layer and use on land and big objects near water to get them
reflected and avoid reflect anything else for performance. The layer must also be assigned to the "Water Reflections" section, 
in the "Settings RELF" -> "Reflect Layers" menu. The reflections will only render the layers chosen in this pull down menu.